using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using LibSWBF2.Enums;


public class PhxMissileClass : PhxOrdnanceClass
{
    public PhxProp<float> LightRadius = new PhxProp<float>(3f);
    public PhxProp<Color> LightColor = new PhxProp<Color>(Color.white);

    public PhxProp<float> MinSpeed = new PhxProp<float>(10f);
    public PhxProp<float> Acceleration = new PhxProp<float>(50f);
    public PhxProp<float> Gravity = new PhxProp<float>(0f);
    public PhxProp<float> Rebound = new PhxProp<float>(0f);
    public PhxProp<float> TurnRate = new PhxProp<float>(0f);

    public PhxProp<float> Velocity = new PhxProp<float>(100f);

    public PhxProp<string> TrailEffect = new PhxProp<string>(null);
    public PhxProp<PhxClass> ExplosionName = new PhxProp<PhxClass>(null);

    public PhxProp<PhxClass> ExplosionImpact = new PhxProp<PhxClass>(null);
    public PhxProp<PhxClass> ExplosionExpire = new PhxProp<PhxClass>(null);    
}



[RequireComponent(typeof(Rigidbody), typeof(Light))]
public class PhxMissile : PhxOrdnance
{

    // for heatseeking
    PhxInstance Target; 

    PhxMissileClass MissileClass;
   
    Rigidbody Body;
    Light Light;

    List<Collider> Colliders;
    List<Collider> IgnoredColliders;


    PhxEffect TrailEffect;


    public override void Init()
    {
        gameObject.layer = LayerMask.NameToLayer("OrdnanceAll");

        Body = GetComponent<Rigidbody>();
        Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        Light = GetComponent<Light>();
        Light.type = LightType.Point;

        MissileClass = OrdnanceClass as PhxMissileClass;

        Light.color = MissileClass.LightColor;
        Light.range = MissileClass.LightRadius;
        Light.intensity = 3f;

        Body.useGravity = false;
        Body.drag = 0f;
        Body.mass = .0000000001f;
        Body.angularDrag = 0f;
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SWBFModel Mapping = ModelLoader.Instance.GetModelMapping(gameObject, MissileClass.GeometryName.Get());
        if (Mapping != null)
        {
            Mapping.ConvexifyMeshColliders();
            Mapping.SetColliderMaskAll(ECollisionMaskFlags.Ordnance);
            Mapping.GameRole = SWBFGameRole.Ordnance;
            Mapping.SetColliderLayerFromMaskAll();
            Colliders = Mapping.GetCollidersByLayer(ECollisionMaskFlags.Ordnance); 
        }
        else 
        {
            Colliders = new List<Collider>();
            Colliders.Add(GetComponent<SphereCollider>());
        }

        TrailEffect = SCENE.EffectsManager.LendEffect(MissileClass.TrailEffect.Get());
        if (TrailEffect != null)
        {
            TrailEffect.SetLooping();
            TrailEffect.SetParent(transform);
            TrailEffect.SetLocalTransform(Vector3.zero, Quaternion.identity);
        } 
    }

    public override void Setup(IPhxWeapon OriginatorWeapon, Vector3 Position, Quaternion Rotation)
    {
        OwnerWeapon = OriginatorWeapon;
        Owner = OwnerWeapon.GetOwnerController();

        // Can be null of course
        // Target = OwnerWeapon.GetLockedTarget();

        gameObject.SetActive(true);

        //OwnerWeapon.GetFirePoint(out Vector3 Pos, out Quaternion Rot);
        transform.position = Position;
        transform.rotation = Rotation;

        Body.velocity = transform.forward * MissileClass.MinSpeed.Get();

        // Will need to unignore these in Release, but how to check
        // if they still exist?  Points to per-weapon pools
        // so this can be done easily when weapon is reused or detached, etc
        IgnoredColliders = OriginatorWeapon.GetIgnoredColliders();
        foreach (Collider IgnoredCollider in IgnoredColliders)
        {
            foreach (Collider MissileCollider in Colliders)
            {
                //Debug.LogFormat("Ignoring collider objects: {0}, {1}", IgnoredCollider.gameObject.name, MissileCollider.gameObject.name);
                Physics.IgnoreCollision(MissileCollider, IgnoredCollider);
            }                   
        }

        TrailEffect?.Play();
    }

    public override void Destroy()
    {
        //IgnoredColliders.Clear();

        Owner = null;
        Target = null;

        TrailEffect?.Stop();
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void TickPhysics(float deltaTime)
    {
        // Manual gravity, since it varies
        Body.AddForce(9.8f * MissileClass.Gravity * Vector3.down, ForceMode.Acceleration);

        if (Vector3.Magnitude(Body.velocity) > MissileClass.Velocity)
        {
            Body.velocity = MissileClass.Velocity * Vector3.Normalize(Body.velocity);
        }
        else 
        {
            Body.AddForce(MissileClass.Acceleration * transform.forward, ForceMode.Acceleration);
        }

        if (Target != null)
        {
            // Handle Turn towards Target
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (gameObject.activeSelf)
        {
            ContactPoint Contact = coll.GetContact(0);

            PhxClass Exp = MissileClass.ExplosionImpact.Get();
            if (Exp == null)
            {
                Exp = MissileClass.ExplosionName.Get();
            }
            

            Debug.LogFormat("Missile hit collider: {0}", Contact.otherCollider.name);

            PhxExplosionManager.AddExplosion(null, Exp as PhxExplosionClass, Contact.point, Quaternion.LookRotation(Contact.normal, Vector3.up));

            ParentPool.Free(this);
        }
    }
}
