using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;

public class RuntimeScene
{
    Dictionary<string, ISWBFClass>    Classes   = new Dictionary<string, ISWBFClass>();
    Dictionary<string, ISWBFInstance> Instances = new Dictionary<string, ISWBFInstance>();

    Container EnvCon;
    bool bTerrainImported = false;

    Dictionary<string, GameObject> loadedSkydomes = new Dictionary<string, GameObject>();
    Dictionary<string, Collider> LoadedRegions = new Dictionary<string, Collider>();

    public RuntimeScene(Container c)
    {
        EnvCon = c;
    }

    public void Import(World[] worldLayers)
    {
        MaterialLoader.UseHDRP = true;

        foreach (World world in worldLayers)
        {
            GameObject worldRoot = new GameObject(world.Name);

            //Regions - Import before instances, since instances may reference regions
            var regionsRoot = WorldLoader.Instance.ImportRegions(world.GetRegions());
            regionsRoot.transform.parent = worldRoot.transform;

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
            if (!loadedSkydomes.ContainsKey(world.SkydomeName))
            {
                var skyRoot = WorldLoader.Instance.ImportSkydome(EnvCon.FindConfig(ConfigType.Skydome, world.SkydomeName));
                if (skyRoot != null)
                {
                    skyRoot.transform.parent = worldRoot.transform;
                }

                loadedSkydomes[world.SkydomeName] = skyRoot;
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
                odf = (ISWBFClass)Activator.CreateInstance(OdfRegister.GetClassType(rootClass.BaseClassName));
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
            }

            instanceObject.transform.rotation = UnityUtils.QuatFromLibWorld(inst.Rotation);
            instanceObject.transform.position = UnityUtils.Vec3FromLibWorld(inst.Position);
            instanceObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            instanceObjects.Add(instanceObject);
        }

        return instanceObjects;
    }
}
