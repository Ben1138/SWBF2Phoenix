using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;



public interface IPhxTrackable
{
    public Vector3 GetCameraPosition();
    public Quaternion GetCameraRotation();
}



public abstract class PhxVehicle<T> : PhxControlableInstance<T> where T : PhxClass 
{
    public Action<PhxInstance> OnDeath;

    private float SliceProgress = 0.0f;


    protected PhxSoldier Driver = null;


    public void IncrementSlice() { SliceProgress += .1f; }


    public bool AddSoldier(PhxSoldier soldier) { return false; }


    

    public void FillSections(PhxPropertySection sections){}


    // Dealing with SWBF's use of concave colliders on physics objects will be a major challenge
    // unless we can reliably use the primitives found on each imported model...
    protected void PruneMeshColliders(Transform tx)
    {
        MeshCollider coll = tx.gameObject.GetComponent<MeshCollider>();
        if (coll != null)
        {
            Destroy(coll);
        }

        for (int j = 0; j < tx.childCount; j++)
        {
            PruneMeshColliders(tx.GetChild(j));
        }
    }


    

    public void EjectOccupant(PhxSoldier occupant)
    {
        //occupant.SetFree(transform.position + Vector3.up * 2.0f);
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
        ("WeaponAmmo",    new PhxProp<int>(0)),
        ("PilotPosition", new PhxProp<string>(null)),
        ("Pilot9Pose",    new PhxProp<string>(null)),
        ("EyePointOffset",new PhxProp<Vector3>(Vector3.zero)),
        ("TrackCenter",   new PhxProp<Vector3>(Vector3.zero)),
        ("TrackOffset",   new PhxProp<Vector3>(Vector3.zero))
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














