
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Enums;
using System.Runtime.ExceptionServices;


public class PhxProp : PhxInstance<PhxProp.ClassProperties>, IPhxTickable
{
    protected static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    public class ClassProperties : PhxClass 
    {
        public PhxMultiProp SoldierCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp BuildingCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp VehicleCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp OrdnanceCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp TargetableCollision = new PhxMultiProp(typeof(string));
        public PhxProp<string> GeometryName = new PhxProp<string>("");
    }


    public string EntityClassName;

    [Serializable]
    public class AttachedODF
    {
        public string ObjectClass;
        public GameObject Object;
    }

    public List<AttachedODF> AttachedODFs = new List<AttachedODF>();

    [Serializable]
    public class AttachedEffect
    {
        public string EffectName;
        public float RespawnDelay = 0f;
        public float RespawnDelayTimer = 0f;        
        public GameObject EffectObject;
        public PhxEffect Effect;
    }

    public List<AttachedEffect> AttachedEffects = new List<AttachedEffect>();


    


    public override void Init()
    {
        SWBFModel ModelMapping = ModelLoader.Instance.GetModelMapping(gameObject, C.GeometryName); 

        EntityClassName = C.EntityClass.Name;

        if (ModelMapping != null)
        {
            void SetODFCollision(PhxMultiProp Props, ECollisionMaskFlags Flag)
            {
                foreach (object[] values in Props.Values)
                {
                    ModelMapping.SetColliderMask(values[0] as string, Flag);
                }
            }

            SetODFCollision(C.SoldierCollision,  ECollisionMaskFlags.Soldier);
            SetODFCollision(C.BuildingCollision, ECollisionMaskFlags.Building);
            SetODFCollision(C.OrdnanceCollision, ECollisionMaskFlags.Ordnance);
            SetODFCollision(C.VehicleCollision,  ECollisionMaskFlags.Vehicle);

            foreach (object[] TCvalues in C.TargetableCollision.Values)
            {
                ModelMapping.EnableCollider(TCvalues[0] as string, false);
            }

            ModelMapping.GameRole = SWBFGameRole.Building;
            ModelMapping.ExpandMultiLayerColliders();
            ModelMapping.SetColliderLayerFromMaskAll();            
        }

        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        PhxClass CurrentODFToAttach = null;
        string CurrentEffectToAttach = null;

        for (int i = 0; i < properties.Length && i < values.Length; i++)
        {
            if (properties[i] == 0xa9d0d48b /*AttachOdf*/)
            {
                CurrentODFToAttach = SCENE.GetClass(values[i]);
                CurrentEffectToAttach = null;
            }
            else if (properties[i] == 0x6a6c7e0d /*AttachEffect*/)
            {
                CurrentEffectToAttach = values[i]; 
                CurrentODFToAttach = null;
            }
            else if (properties[i] == 0x3be7b80a /*AttachToHardPoint*/)
            {
                string[] Parts = values[i].Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);

                // AttachToHardPoint can have a second float parameter that determines how the long the
                // AttachedEffect will take to respawn

                string HpName = Parts[0];

                float RespawnTime = 0f;
                try {
                    RespawnTime = Parts.Length > 1 ? PhxUtils.FloatFromString(Parts[1]) : 0f;
                }
                catch 
                {
                    Debug.LogErrorFormat("Failed to parse float from: {0}", Parts[1]);
                }


                Transform ChildTx = UnityUtils.FindChildTransform(transform, HpName);

                if (ChildTx != null)
                {
                    if (CurrentODFToAttach != null)
                    {
                        var newODF = new AttachedODF();
                        AttachedODFs.Add(newODF);

                        PhxInstance attachedInst = SCENE.CreateInstance(CurrentODFToAttach, true, ChildTx);
                        if (attachedInst != null)
                        {
                            GameObject AttachedODFObject = attachedInst.gameObject;
                            newODF.ObjectClass = CurrentODFToAttach.EntityClass.Name;
                            newODF.Object = AttachedODFObject;   
                        }                
                    }

                    if (CurrentEffectToAttach != null)
                    {
                        PhxEffect Effect = SCENE.EffectsManager.LendEffect(CurrentEffectToAttach);

                        if (Effect == null || Effect.EffectObject == null) return;

                        Effect.SetParent(ChildTx);
                        Effect.SetLooping(false);
                        Effect.Stop();

                        AttachedEffect NewAttachedEffect = new AttachedEffect();
                        NewAttachedEffect.RespawnDelay = RespawnTime;
                        NewAttachedEffect.RespawnDelayTimer = RespawnTime + UnityEngine.Random.Range(0f, RespawnTime + .001f);
                        NewAttachedEffect.Effect = Effect;
                        NewAttachedEffect.EffectObject = Effect.EffectObject;
                        NewAttachedEffect.EffectName = NewAttachedEffect.EffectObject.name;

                        AttachedEffects.Add(NewAttachedEffect);  
                    }
                }              
            }
        }
    }


    public virtual void Tick(float deltaTime)
    {
        foreach (AttachedEffect Effect in AttachedEffects)
        {
            if (!Effect.Effect.IsStillPlaying())
            {
                Effect.RespawnDelayTimer -= deltaTime;
            }

            if (Effect.RespawnDelayTimer < 0f)
            {
                Effect.Effect.Play();
                Effect.RespawnDelayTimer = Effect.RespawnDelay;
            }
        }
    }

    public override void Destroy(){}
}
