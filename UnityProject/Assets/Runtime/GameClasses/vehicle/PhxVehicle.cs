using System;
using System.Reflection;
using UnityEngine;
using LibSWBF2.Wrappers;


public abstract class PhxVehicle<T> : PhxControlableInstance<T> where T : PhxClass
{
    public Action<PhxInstance> OnDeath;

    public PhxSoldier[] Occupants = null;

    private float SliceProgress = 0.0f;

    public void IncrementSlice()
    {
        SliceProgress += .1f;
    }

    public bool AddSoldier(PhxSoldier soldier)
    {
        // Check how many seats available, return true if one is...
        return true;
    }
}



public class PhxVehicleProperties : PhxClass
{
    public PhxPropertySection ChunkSection = new PhxPropertySection(
    	"CHUNKSECTION",
    	("ChunkGeometryName", new PhxProp<string>(null)),
    	("ChunkNodeName", new PhxProp<string>(null)),
    	("ChunkTerrainCollisions", new PhxProp<int>(1)),
    	("ChunkTerrainEffect", new PhxProp<string>("")),
    	("ChunkPhysics", new PhxProp<string>("")),
    	("ChunkOmega", new PhxMultiProp(typeof(float), typeof(float), typeof(float))), //not sure how to handle this yet...
    	("ChunkBounciness", new PhxProp<float>(0.0f)),
    	("ChunkStickiness", new PhxProp<float>(0.0f)),
    	("ChunkSpeed", new PhxProp<float>(1.0f)),
    	("ChunkUpFactor", new PhxProp<float>(0.5f)),
    	("ChunkTrailEffect", new PhxProp<string>("")),
    	("ChunkSmokeEffect", new PhxProp<string>("")),
    	("ChunkSmokeNodeName", new PhxProp<string>(""))
    );

    public PhxProp<string> HealthType = new PhxProp<string>("vehicle");
	public PhxProp<float>  MaxHealth = new PhxProp<float>(100.0f);


    public PhxProp<string> AnimationName = new PhxProp<string>("");

    // Pilot poses, most relevant for speeders, where pilot's body is fully visible
    public PhxProp<string> FinAnimation = new PhxProp<string>("");


    public PhxMultiProp SoldierCollision = new PhxMultiProp(typeof(string));


    public PhxPropertySection Flyer = new PhxPropertySection(
        "FLYERSECTION",
        ("VehicleType",   new PhxProp<string>(null)),
        ("WeaponName",    new PhxProp<string>(null)),
        ("WeaponAmmo",    new PhxProp<int>(0))
    );

    public PhxPropertySection Weapons = new PhxPropertySection(
        "WEAPONSECTION",
        ("WeaponName",    new PhxProp<string>(null)),
        ("WeaponAmmo",    new PhxProp<int>(0)),
        ("WeaponChannel", new PhxProp<int>(0))
    );

    public PhxProp<string> VehicleType = new PhxProp<string>("light");
    public PhxProp<string> AISizeType =  new PhxProp<string>("MEDIUM");
}














