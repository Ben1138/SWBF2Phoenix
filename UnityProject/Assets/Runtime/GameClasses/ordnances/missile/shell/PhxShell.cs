using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using LibSWBF2.Enums;


/*
Not sure how shells differ from missiles yet,
but they appear to use gravity, while missiles don't.

I suppose they also can't heatseek
*/


public class PhxShellClass : PhxMissileClass
{
    // Probably in Gs
    public PhxProp<float> Gravity = new PhxProp<float>(1f);
}



//[RequireComponent(typeof(Rigidbody), typeof(Light))]
public class PhxShell : PhxMissile
{
    PhxShellClass ShellClass;

    public override void Init()
    {
        base.Init();
        ShellClass = OrdnanceClass as PhxShellClass;
    }


    public override void Setup(IPhxWeapon OriginatorWeapon, Vector3 Position, Quaternion Rotation)
    {
        OwnerWeapon = OriginatorWeapon;
        Owner = OwnerWeapon.GetOwnerController();

        Body.constraints = RigidbodyConstraints.None;

        // Can be null of course
        // Target = OwnerWeapon.GetLockedTarget();

        TimeAlive = 0f;

        gameObject.SetActive(true);

        //OwnerWeapon.GetFirePoint(out Vector3 Pos, out Quaternion Rot);
        transform.position = Position;
        transform.rotation = Rotation;

        Body.velocity = transform.forward * ShellClass.Velocity.Get();

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


    public override void TickPhysics(float deltaTime)
    {
        Body.AddForce(9.8f * ShellClass.Gravity * Vector3.down, ForceMode.Acceleration);
        base.TickPhysics(deltaTime);
    }
}
