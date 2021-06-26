
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Enums;
using System.Runtime.ExceptionServices;


public class PhxProp : PhxInstance<PhxProp.ClassProperties>
{
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    public class ClassProperties : PhxClass 
    {
        public PhxMultiProp SoldierCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp BuildingCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp VehicleCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp OrdnanceCollision = new PhxMultiProp(typeof(string));
        public PhxMultiProp TargetableCollision = new PhxMultiProp(typeof(string));
        public PhxProp<string> GeometryName = new PhxProp<string>("");
    }

    [Serializable]
    public class AttachedODF
    {
        public string ObjectClass;
        public GameObject Object;
    }

    public List<AttachedODF> AttachedODFs = new List<AttachedODF>();



    public override void Init()
    {
        SWBFModel ModelMapping = ModelLoader.Instance.GetModelMapping(gameObject, C.GeometryName); 

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

        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        PhxClass CurrentODFToAttach = null;
        for (int i = 0; i < properties.Length && i < values.Length; i++)
        {
            if (properties[i] == HashUtils.GetFNV("AttachOdf"))
            {
                CurrentODFToAttach = SCENE.GetClass(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AttachToHardPoint"))
            {
                var newODF = new AttachedODF();
                AttachedODFs.Add(newODF);

                Transform ChildTx = UnityUtils.FindChildTransform(transform, values[i]);
                GameObject AttachedODFObject = SCENE.InstantiateClass(CurrentODFToAttach, true, ChildTx);

                if (AttachedODFObject != null && ChildTx != null)
                {
                    newODF.ObjectClass = CurrentODFToAttach.EntityClass.Name;
                    newODF.Object = AttachedODFObject;                    
                }                
            }
        }
    }

    public override void Tick(float deltaTime){}
    public override void TickPhysics(float deltaTime){}
    public override void BindEvents(){}
}
