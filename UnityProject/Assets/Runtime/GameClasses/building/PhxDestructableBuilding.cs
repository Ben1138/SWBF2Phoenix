
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Enums;
using LibSWBF2.Wrappers;
using System.Runtime.ExceptionServices;


public class PhxDestructableBuilding : PhxInstance<PhxDestructableBuilding.ClassProperties>, IPhxTickable
{
    protected static PhxScene SCENE => PhxGame.GetScene();

    public class ClassProperties : PhxClass 
    {
        public PhxProp<float> MaxHealth = new PhxProp<float>(100.0f);

        public PhxProp<PhxClass> ExplosionName = new PhxProp<PhxClass>(null);

        // This is the reason for the addition of the IsGeometryManuallyInitialized check in ClassLoader.
        // The different geometries will be attached as children of the root and activated/deactivated
        // when building is destroyed or repaired
        public PhxProp<string> GeometryName = new PhxProp<string>("");
        public PhxProp<string> DestroyedGeometryName = new PhxProp<string>("");
    }

    public PhxProp<float> CurHealth = new PhxProp<float>(1f);

    public List<PhxDamageEffect> DamageEffects = new List<PhxDamageEffect>();

    public GameObject BuiltGeometry;
    public GameObject DestroyedGeometry;

    protected bool IsBuilt = true;



    public override void Init()
    {
        // This is so colliding ordnance can easily get the root object and add damage
        Rigidbody Body = gameObject.AddComponent<Rigidbody>();
        Body.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("BuildingAll");


        BuiltGeometry = ModelLoader.Instance.GetGameObjectFromModel(C.GeometryName.Get(), null);

        if (BuiltGeometry != null)
        {
            SWBFModel BuiltModelMapping = ModelLoader.Instance.GetModelMapping(BuiltGeometry, C.GeometryName.Get());

            if (BuiltModelMapping != null)
            {
                BuiltModelMapping.GameRole = SWBFGameRole.Building;
                BuiltModelMapping.ExpandMultiLayerColliders();
                BuiltModelMapping.SetColliderLayerFromMaskAll();                
            }

            BuiltGeometry.transform.SetParent(transform);
            BuiltGeometry.transform.localPosition = Vector3.zero;
            BuiltGeometry.transform.localRotation = Quaternion.identity;
            BuiltGeometry.SetActive(true);            
        }
        

        DestroyedGeometry = ModelLoader.Instance.GetGameObjectFromModel(C.DestroyedGeometryName.Get(), null);

        if (DestroyedGeometry != null)
        {
            SWBFModel DestroyedModelMapping = ModelLoader.Instance.GetModelMapping(DestroyedGeometry, C.DestroyedGeometryName.Get());
            if (DestroyedModelMapping != null)
            {
                DestroyedModelMapping.GameRole = SWBFGameRole.Building;
                DestroyedModelMapping.ExpandMultiLayerColliders();
                DestroyedModelMapping.SetColliderLayerFromMaskAll(); 
            }            

            DestroyedGeometry.transform.SetParent(transform);
            DestroyedGeometry.transform.localPosition = Vector3.zero;
            DestroyedGeometry.transform.localRotation = Quaternion.identity;
            DestroyedGeometry.SetActive(false);
        }

        CurHealth.Set(C.MaxHealth.Get());

        EntityClass EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        PhxDamageEffect CurrDamageEffect = null;

        int i = 0;
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("DamageStartPercent"))
            {
                CurrDamageEffect = new PhxDamageEffect();
                DamageEffects.Add(CurrDamageEffect);

                CurrDamageEffect.DamageStartPercent = float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture) / 100f;
            }
            else if (properties[i] == HashUtils.GetFNV("DamageStopPercent"))
            {
                CurrDamageEffect.DamageStopPercent = float.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture) / 100f;
            }
            else if (properties[i] == HashUtils.GetFNV("DamageEffect"))
            {
                CurrDamageEffect.Effect = SCENE.EffectsManager.LendEffect(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("DamageAttachPoint"))
            {
                CurrDamageEffect.DamageAttachPoint = UnityUtils.FindChildTransform(transform, values[i]);
            }

            i++;
        }
    }


    public virtual void Tick(float deltaTime)
    {
        float HealthPercent = CurHealth.Get() / C.MaxHealth.Get();

        if (HealthPercent > 0.0001f)
        {
            if (!IsBuilt)
            {
                BuiltGeometry.SetActive(true);
                DestroyedGeometry.SetActive(false);
                
                IsBuilt = true;
            }

            foreach (PhxDamageEffect DamageEffect in DamageEffects)
            {
                DamageEffect.Update(HealthPercent);
            }
        }
        else 
        {
            if (IsBuilt)
            {
                PhxExplosionManager.AddExplosion(null, C.ExplosionName.Get() as PhxExplosionClass, transform.position, transform.rotation);
                
                BuiltGeometry.SetActive(false);
                DestroyedGeometry.SetActive(true);

                IsBuilt = false;
            }
        }
    }

    public override void Destroy(){}
}
