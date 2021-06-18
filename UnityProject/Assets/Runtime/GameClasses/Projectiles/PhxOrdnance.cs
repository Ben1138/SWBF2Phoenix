using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


/*
All ordnances need some basic things. 
HOWEVER, some ordnance is implicitly defined in weapon classes
such as "grenade."  It should be easy to change this to account
for that though.
*/

public abstract class PhxOrdnance : MonoBehaviour
{
    public class ClassProperties : PhxClass 
    {
        public PhxProp<float> MaxDamage = new PhxProp<float>(1f);

        public PhxProp<float> VehicleScale =  new PhxProp<float>(1f);
        public PhxProp<float> ShieldScale =   new PhxProp<float>(1f);
        public PhxProp<float> PersonScale =   new PhxProp<float>(1f);
        public PhxProp<float> AnimalScale =   new PhxProp<float>(1f);
        public PhxProp<float> DroidScale =    new PhxProp<float>(1f);
        public PhxProp<float> BuildingScale = new PhxProp<float>(1f);

        public PhxProp<string> GeometryName = new PhxProp<string>(null);

        public PhxProp<float> LifeSpan = new PhxProp<float>(3f);
    }


    protected static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    protected static PhxRuntimeMatch MTC => PhxGameRuntime.GetMatch();
    protected static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    protected static PhxCamera CAM => PhxGameRuntime.GetCamera();


    /*
    Should be PhxClass NOT PhxOrdnance.ClassProperties, since 'grenade'
    is a weapon class, but will be used here to initialize ordnance.
    */
    protected PhxClass OrdnanceClass;


    [System.NonSerialized]
    public bool IsInitialized;

    /*
    Previously this was kept in PhxPool, but will need to be updated
    differently for different ordnances.
    */
    protected float TimeAlive;

    /* 
    Needed because changes to the weapon can affect changes in
    ordnance behavior.  E.g. a beam moves with it's weapon's firepoint
    but a bolt doesn't, a guided missile lasts until it's weapon is inactive
    but a standard missile flies regardless of its owner's status
    */ 
    protected IPhxWeapon OwnerWeapon;
    
    /* 
    Should be kept apart from weapon, since the weapon/owning
    entity could be destroyed and but the controller 
    behind the owner will persist.
    */
    protected PhxPawnController Owner;     
    
    // Class level setup, e.g. components, collision. Called once.
    public abstract void Init(PhxClass Class);

    /*
    Use-specific setup, might need a better name.
    Originally had Rotation/Position params, but these can now be
    obtained from IPhxWeapon.  Given the wildly varying needs of different ordnances,
    the parameter list would've grown quite long, thus IPhxWeapon should just be expanded
    to cover those needs.  Perhaps a parameter: Vector3 Target (in world space) will be added. 
    */  
    public abstract void Setup(IPhxWeapon Originator);

    // Clear use-specific data,  release back to the manager instance
    protected abstract void Release();
}
