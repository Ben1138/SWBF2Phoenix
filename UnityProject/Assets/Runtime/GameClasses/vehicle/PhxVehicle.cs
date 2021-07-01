using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;



public abstract class PhxVehicle<T> : PhxControlableInstance<T> where T : PhxClass
{
    public Action<PhxInstance> OnDeath;

    private float SliceProgress = 0.0f;


    protected PhxSoldier Driver = null;


    public void IncrementSlice()
    {
        SliceProgress += .1f;
    }


    public bool AddSoldier(PhxSoldier soldier)
    {
        if (Driver == null)
        {
            Driver = soldier;
            return true;
        }

        return false;
    }

    /*
    public PhxVehicleSection AddSoldier(PhxSoldier soldier)
    {
        // Check how many seats available, return true if one is...

        int i = 0;
        foreach (PhxVehicleSection section in Sections)
        {
            if (!section.HasOccupant())
            {
                section.SetOccupant(soldier);

                if (i == 0)
                {
                    Driver = soldier;
                }

                return section;
            }

            i++;
        }

        return null;
    }
    */



    //List<PhxVehicleSection> Sections = new List<PhxVehicleSection>();

    public void FillSections(PhxPropertySection sections)
    {
        foreach (Dictionary<string, IPhxPropRef> section in sections)
        {
            //var NewSection = AddComponent<PhxVehicleSection>();
            //NewSection.SetProperties(section, this);
            //Sections.Add(NewSection);
        }
    }



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



    public bool CameraFollow()
    {
        PhxGameRuntime.GetCamera().FollowVehicle(this, new Vector3(0.0f, 2.0f, 0.0f) + new Vector3(0.0f, 2.0f, -5.0f), new Vector3(0.0f, .5f, 6.0f));



        return true;
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














