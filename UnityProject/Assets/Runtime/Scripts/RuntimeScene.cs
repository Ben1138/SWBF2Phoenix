using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;

public class RuntimeScene
{
    Dictionary<string, ISWBFClass>    Classes   = new Dictionary<string, ISWBFClass>();
    Dictionary<string, ISWBFInstance> Instances = new Dictionary<string, ISWBFInstance>();

    Container EnvCon;
    bool bTerrainImported = false;

    Dictionary<string, GameObject> LoadedSkydomes = new Dictionary<string, GameObject>();
    Dictionary<string, Region>     Regions  = new Dictionary<string, Region>();

    public RuntimeScene(Container c)
    {
        EnvCon = c;
    }

    public void AssignProp<T1, T2>(T1 instOrClass, string propName, Ref<T2> value) where T1 : ISWBFProperties
    {
        if (instOrClass.GetProperty(propName, out string outVal))
        {
            value.Set((T2)Convert.ChangeType(outVal, typeof(T2), CultureInfo.InvariantCulture));
        }
    }

    public void AssignProp<T1>(T1 instOrClass, string propName, Ref<Region> value) where T1 : ISWBFProperties
    {
        if (instOrClass.GetProperty(propName, out string outVal))
        {
            value.Set(GetRegion(outVal));
        }
    }

    public void AssignProp<T1>(T1 instOrClass, string propName, int argIdx, Ref<AudioClip> value) where T1 : ISWBFProperties
    {
        if (instOrClass.GetProperty(propName, out string outVal))
        {
            string[] args = outVal.Split(' ');
            value.Set(SoundLoader.LoadSound(args[argIdx]));
        }
    }

    public void SetProperty(string instName, string propName, object propValue)
    {
        if (Instances.TryGetValue(instName, out ISWBFInstance inst))
        {
            inst.P.SetProperty(propName, propValue);
            return;
        }
        Debug.LogWarningFormat("Could not find instance '{0}' to set property '{1}'!", instName, propName);
    }

    public void SetClassProperty(string className, string propName, object propValue)
    {
        if (Classes.TryGetValue(className, out ISWBFClass cl))
        {
            cl.P.SetProperty(propName, propValue);
            return;
        }
        Debug.LogWarningFormat("Coukd not find odf class '{0}' to set class property '{1}'!", className, propName);
    }

    public Region GetRegion(string name)
    {
        if (Regions.TryGetValue(name, out Region region))
        {
            return region;
        }
        return null;
    }

    public void Import(World[] worldLayers)
    {
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
                    Regions.Add(region.Key, region.Value.gameObject.AddComponent<Region>());
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
        }
    }

    ISWBFClass GetClass(EntityClass ec)
    {
        ISWBFClass odf = null;
        if (!Classes.TryGetValue(ec.Name, out odf))
        {
            EntityClass rootClass = ClassLoader.GetRootClass(ec);
            Type classType = OdfRegister.GetClassType(rootClass.BaseClassName);
            if (classType != null)
            {
                odf = (ISWBFClass)Activator.CreateInstance(classType);
                odf.InitClass(ec);
                Classes.Add(ec.Name, odf);
            }
        }
        return odf;
    }

    List<GameObject> ImportInstances(Instance[] instances)
    {
        List<GameObject> instanceObjects = new List<GameObject>();

        foreach (Instance inst in instances)
        {
            GameObject instanceObject = ClassLoader.Instance.LoadInstance(inst);
            EntityClass rootClass = ClassLoader.GetRootClass(inst.EntityClass);

            Type instType = OdfRegister.GetInstanceType(rootClass.BaseClassName);
            if (instType != null)
            {
                ISWBFInstance script = (ISWBFInstance)instanceObject.AddComponent(instType);
                ISWBFClass odf = GetClass(inst.EntityClass);
                script.InitInstance(inst, odf);
                Instances.Add(inst.Name, script);
            }

            instanceObject.transform.rotation = UnityUtils.QuatFromLibWorld(inst.Rotation);
            instanceObject.transform.position = UnityUtils.Vec3FromLibWorld(inst.Position);
            instanceObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            instanceObjects.Add(instanceObject);
        }

        return instanceObjects;
    }
}
