using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;

public class PhxRuntimeScene
{
    struct RTTransform
    {
        public Vector3    Position;
        public Quaternion Rotation;
    }

    Dictionary<string, PhxClass>    Classes   = new Dictionary<string, PhxClass>();
    Dictionary<string, PhxInstance> Instances = new Dictionary<string, PhxInstance>();

    PhxRuntimeEnvironment ENV;
    Container EnvCon;
    bool bTerrainImported = false;

    Dictionary<string, GameObject> LoadedSkydomes = new Dictionary<string, GameObject>();
    Dictionary<string, PhxRegion>     Regions  = new Dictionary<string, PhxRegion>();

    List<GameObject> WorldRoots = new List<GameObject>();
    List<RTTransform> CameraShots = new List<RTTransform>();

    public PhxRuntimeScene(PhxRuntimeEnvironment env, Container c)
    {
        ENV = env;
        EnvCon = c;
    }

    public void SetProperty(string instName, string propName, object propValue)
    {
        if (Instances.TryGetValue(instName, out PhxInstance inst))
        {
            inst.P.SetProperty(propName, propValue);
            return;
        }
        Debug.LogWarningFormat("Could not find instance '{0}' to set property '{1}'!", instName, propName);
    }

    public void SetClassProperty(string className, string propName, object propValue)
    {
        if (Classes.TryGetValue(className, out PhxClass cl))
        {
            cl.P.SetProperty(propName, propValue);
            return;
        }
        Debug.LogWarningFormat("Coukd not find odf class '{0}' to set class property '{1}'!", className, propName);
    }

    public PhxRegion GetRegion(string name)
    {
        if (Regions.TryGetValue(name, out PhxRegion region))
        {
            return region;
        }
        return null;
    }

    public void AddCameraShot(float quatX, float quatY, float quatZ, float quatW, float posX, float posY, float posZ)
    {
        // TODO: space conversion
        CameraShots.Add(new RTTransform 
        {
            Position = new Vector3(posX, posY, posZ),
            Rotation = new Quaternion(quatX, quatY, quatZ, quatW)
        });
    }

    public void Import(World[] worldLayers)
    {
        if (WorldRoots.Count > 0)
        {
            Debug.LogError("Create a new RuntimeScene instance!");
            return;
        }

        MaterialLoader.UseHDRP = true;
        Loader.ResetAllLoaders();

        foreach (World world in worldLayers)
        {
            GameObject worldRoot = new GameObject(world.Name);

            //Regions
            //Import before instances, since instances will reference regions
            var regionsRoot = WorldLoader.Instance.ImportRegions(world.GetRegions());
            regionsRoot.transform.parent = worldRoot.transform;
            foreach (var region in WorldLoader.Instance.LoadedRegions)
            {
                if (!Regions.ContainsKey(region.Key))
                {
                    PhxRegion reg = region.Value.gameObject.AddComponent<PhxRegion>();

                    // invoke Lua events
                    reg.OnEnter += (PhxInstance obj) => GameLuaEvents.Invoke(GameLuaEvents.Event.OnEnterRegion, region.Key, region.Key, obj.name);
                    reg.OnLeave += (PhxInstance obj) => GameLuaEvents.Invoke(GameLuaEvents.Event.OnLeaveRegion, region.Key, region.Key, obj.name);

                    Regions.Add(region.Key, reg);
                }
            }

            //Instances
            GameObject instancesRoot = new GameObject("Instances");
            instancesRoot.transform.parent = worldRoot.transform;

            List<GameObject> instances = ImportInstances(world.GetInstances());
            foreach (GameObject instanceObject in instances)
            {
                instanceObject.transform.SetParent(instancesRoot.transform);
            }

            //Terrain
            var terrain = world.GetTerrain();
            if (terrain != null && !bTerrainImported)
            {
                GameObject terrainGameObject;
                terrainGameObject = WorldLoader.Instance.ImportTerrainAsMeshHDRP(terrain);

                terrainGameObject.transform.parent = worldRoot.transform;
                bTerrainImported = true;
            }


            //Lighting
            var lightingRoots = WorldLoader.Instance.ImportLights(EnvCon.FindConfig(ConfigType.Lighting, world.Name));
            foreach (var lightingRoot in lightingRoots)
            {
                lightingRoot.transform.parent = worldRoot.transform;
            }


            //Skydome, check if already loaded first
            if (!LoadedSkydomes.ContainsKey(world.SkydomeName))
            {
                var skyRoot = WorldLoader.Instance.ImportSkydome(EnvCon.FindConfig(ConfigType.Skydome, world.SkydomeName));
                if (skyRoot != null)
                {
                    skyRoot.transform.parent = worldRoot.transform;
                }

                LoadedSkydomes[world.SkydomeName] = skyRoot;
            }

            WorldRoots.Add(worldRoot);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < WorldRoots.Count; ++i)
        {
            UnityEngine.Object.Destroy(WorldRoots[i]);
        }
    }

    public PhxInstance CreateInstance(PhxClass cl, string instName, Vector3 position, Quaternion rotation, Transform parent=null)
    {
        return CreateInstance(cl.EntityClass, instName, position, rotation, parent).GetComponent<PhxInstance>();
    }

    public PhxClass GetClass(string odfClassName)
    {
        if (Classes.TryGetValue(odfClassName, out PhxClass odf))
        {
            return odf;
        }
        EntityClass ec = ENV.Find<EntityClass>(odfClassName);
        if (ec != null)
        {
            return GetClass(ec);
        }
        return null;
    }

    PhxClass GetClass(EntityClass ec)
    {
        PhxClass odf = null;
        if (!Classes.TryGetValue(ec.Name, out odf))
        {
            EntityClass rootClass = ClassLoader.GetRootClass(ec);
            Type classType = PhxClassRegister.GetPhxClassType(rootClass.BaseClassName);
            if (classType != null)
            {
                odf = (PhxClass)Activator.CreateInstance(classType);
                odf.InitClass(ec);
                Classes.Add(ec.Name, odf);
            }
        }
        return odf;
    }

    GameObject CreateInstance(ISWBFProperties instOrClass, string instName, Vector3 position, Quaternion rotation, Transform parent=null)
    {
        if (instOrClass == null)
        {
            Debug.LogWarning("Called 'CreateInstance' with NULL!");
            return null;
        }

        EntityClass ec = instOrClass.GetType() == typeof(Instance) ? ((Instance)instOrClass).EntityClass : ((EntityClass)instOrClass);

        GameObject instanceObject = ClassLoader.Instance.Instantiate(instOrClass, instName);
        EntityClass rootClass = ClassLoader.GetRootClass(ec);

        if (rootClass == null)
        {
            Debug.LogWarning($"Could not find root class of '{ec.Name}'!");
            return null;
        }

        Type instType = PhxClassRegister.GetPhxInstanceType(rootClass.BaseClassName);
        if (instType != null)
        {
            PhxClass odf = GetClass(ec);
            PhxInstance script = (PhxInstance)instanceObject.AddComponent(instType);
            script.InitInstance(instOrClass, odf);
            Instances.Add(instanceObject.name, script);
        }

        instanceObject.transform.SetParent(parent);
        instanceObject.transform.localRotation = rotation;
        instanceObject.transform.localPosition = position;
        instanceObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        return instanceObject;
    }

    List<GameObject> ImportInstances(Instance[] instances)
    {
        ModelLoader.Instance.PhyMat = PhxGameRuntime.Instance.GroundPhyMat;

        List<GameObject> instanceObjects = new List<GameObject>();
        foreach (Instance inst in instances)
        {
            instanceObjects.Add(
                CreateInstance(
                    inst,
                    inst.Name,
                    UnityUtils.Vec3FromLibWorld(inst.Position),
                    UnityUtils.QuatFromLibWorld(inst.Rotation)
                )
            );
        }
        return instanceObjects;
    }
}
