using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;



public class PhxOrdnanceClass : PhxClass 
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


public abstract class PhxOrdnance : PhxComponent
{
    public PhxOrdnanceClass OrdnanceClass;

    protected static PhxGame GAME => PhxGame.Instance;
    protected static PhxMatch MTC => PhxGame.GetMatch();
    protected static PhxScene SCENE => PhxGame.GetScene();
    protected static PhxCamera CAM => PhxGame.GetCamera();


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

    /*
    Use-specific setup, might need a better name.
    Given the wildly varying needs of different ordnances,
    the parameter list would've grown quite long, thus IPhxWeapon should just be expanded
    to cover those needs.
    */  
    public abstract void Setup(IPhxWeapon Originator, Vector3 Position, Quaternion Rotation);
}
