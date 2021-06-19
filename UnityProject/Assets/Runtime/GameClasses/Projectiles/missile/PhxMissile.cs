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


    public override void Init(PhxOrdnanceClass OClass)
    {
        IsInitialized = true;

        MissileClass = OClass as PhxMissileClass;

        Light.color = MissileClass.LightColor;
        Light.range = MissileClass.LightRadius;
        Light.intensity = 3f;

        Body.useGravity = false;
        Body.drag = 0f;
        Body.mass = .0000000001f;
        Body.angularDrag = 0f;

        SWBFModel Mapping = ModelLoader.Instance.GetModelMapping(gameObject, MissileClass.GeometryName.Get());
        Mapping.SetColliderMaskAll(ECollisionMaskFlags.Ordnance);
        Mapping.GameRole = SWBFGameRole.Ordnance;
        Mapping.SetColliderLayerFromMaskAll();
        Colliders = Mapping.GetCollidersByLayer(ECollisionMaskFlags.Ordnance); 

        TrailEffect = SCENE.EffectsManager.LendEffect(MissileClass.TrailEffect.Get());
        if (TrailEffect != null)
        {
            TrailEffect.SetLooping();
            TrailEffect.SetParent(transform);
            TrailEffect.SetLocalTransform(Vector3.zero, Quaternion.identity);
        } 
    }


    public override void Setup(IPhxWeapon OriginatorWeapon)
    {        
        OwnerWeapon = OriginatorWeapon;
        Owner = OwnerWeapon.GetOwnerController();

        // Can be null of course
        // Target = OwnerWeapon.GetLockedTarget();

        TimeAlive = 0f;

        gameObject.SetActive(true);

        OwnerWeapon.GetFirePoint(out Vector3 Pos, out Quaternion Rot);
        transform.position = Pos;
        transform.rotation = Rot;

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


    protected override void Release()
    {
        IgnoredColliders = null;

        Owner = null;
        Target = null;

        TrailEffect?.Stop();

        gameObject.SetActive(false);
    }



    void Update()
    {
        float deltaTime = Time.deltaTime;

        TimeAlive += deltaTime;
        if (TimeAlive > MissileClass.LifeSpan)
        {
            Release();
            return;
        }

        // Manual gravity, since it varies
        Body.AddForce(deltaTime * MissileClass.Gravity * Vector3.down, ForceMode.Acceleration);

        if (Vector3.Magnitude(Body.velocity) > MissileClass.Velocity)
        {
            Body.velocity = MissileClass.Velocity * Vector3.Normalize(Body.velocity);
        }
        else 
        {
            Body.AddForce(deltaTime * MissileClass.Acceleration * transform.forward, ForceMode.Acceleration);
        }

        if (Target != null)
        {
            // Handle Turn towards Target
        }
    }


    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("OrdnanceAll");
        
        Body = GetComponent<Rigidbody>();
        Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; 

        Light = GetComponent<Light>();
        Light.type = LightType.Point;
    }

    void OnCollisionEnter(Collision coll)
    {
        Release();
    }
}
