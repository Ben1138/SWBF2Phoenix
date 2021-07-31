using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;


/*
Implement if you want object to be followed by the camera.
Added so that PhxVehicleSections, which are not Lua-accessible instances,
could be followed.
*/

public interface IPhxTrackable
{
    public Vector3 GetCameraPosition();
    public Quaternion GetCameraRotation();
}


/*
All vehicles have control sections (FLYERSECTION/WALKERSECTION {BODY, TURRET1, TURRET2}, etc),
chunks, and some unique class properties.  Though the vast majority of those properties
are not accessible in Lua.

They can be entered, exited, sliced, repaired by soldiers

*/

public abstract class PhxVehicle : PhxControlableInstance<PhxVehicleProperties>, IPhxTrackable 
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeMatch MTC => PhxGameRuntime.GetMatch();
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    static PhxCamera CAM => PhxGameRuntime.GetCamera();

    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);



    public Action<PhxInstance> OnDeath;

    protected float SliceProgress;


    protected PhxSoldier Driver;
    protected PhxInstance Aim;

    protected List<PhxVehicleSection> Sections;


    protected SWBFModel ModelMapping = null;



    public override void Init()
    {
        /*
        COLLISION
        */

        ModelMapping = ModelLoader.Instance.GetModelMapping(gameObject, C.GeometryName); 
        //ModelMapping.StripMeshCollider();

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

        ModelMapping.GameRole = SWBFGameRole.Vehicle;
        ModelMapping.ExpandMultiLayerColliders();
        ModelMapping.SetColliderLayerFromMaskAll();
    }





    // Will return true if vehicle is sliceable.  Out param is SliceProgress
    public virtual bool IncrementSlice(out float progress)
    {
        SliceProgress += 5.0f;
        progress = SliceProgress;
        return true;
    }



    public bool HasAvailableSeat()
    {
        return GetNextAvailableSeat() != -1;
    }


    protected int GetNextAvailableSeat(int startIndex = -1)
    {
        int numSeats = Sections.Count;
        int i = (startIndex + 1) % numSeats;
        
        while (i != startIndex)
        {
            if (Sections[i].Occupant == null)
            {
                //Debug.LogFormat("Found open seat at index {0}", i);
                return i;
            }

            if (startIndex == -1 && i == numSeats - 1)
            {
                return -1;
            }

            i = (i + 1) % numSeats;
        }

        return -1;
    }


    public bool TrySwitchSeat(int index)
    {
        // Get next index
        int seat = GetNextAvailableSeat(index);

        if (seat == -1)
        {
            return false;
        }
        else
        {
            Sections[seat].SetOccupant(Sections[index].Occupant);
            Sections[seat].Occupant.SetPilot(Sections[seat]);
            Sections[index].Occupant = null;
            CAM.Track(Sections[seat]);

            return true;
        }
    }


    public PhxVehicleSection TryEnterVehicle(PhxSoldier soldier)
    {
        // Find first available seat
        int seat = GetNextAvailableSeat();

        if (seat == -1)
        {
            return null;
        }
        else 
        {
            Sections[seat].SetOccupant(soldier);
            PhxGameRuntime.GetCamera().Track(Sections[seat]);
            
            return Sections[seat];
        }
    }


    public bool Eject(int i)
    {
        if (i < Sections.Count || Sections[i] != null || Sections[i].Occupant != null)
        {
            Sections[i].Occupant.SetFree(transform.position + Vector3.up * 2.0f);
            CAM.Follow(Sections[i].Occupant);
            Sections[i].Occupant = null;

            return true;
        }
        else 
        {
            return false;
        }
    }


    public List<Collider> GetAllColliders()
    {
        List<Collider> Colliders = new List<Collider>();
        List<Transform> ChildTransforms = UnityUtils.GetChildTransforms(transform);

        ChildTransforms.Add(transform);

        foreach (Transform childTx in ChildTransforms)
        {
            foreach (Collider coll in childTx.gameObject.GetComponents<Collider>())
            {
                Colliders.Add(coll);
            }
        }

        return Colliders;
    }


    protected void SetupEnterTrigger()
    {
        BoxCollider EnterTrigger = gameObject.AddComponent<BoxCollider>();
        EnterTrigger.size = UnityUtils.GetMaxBounds(gameObject).extents * 2.5f;
        EnterTrigger.isTrigger = true;
    }


    // So projectiles don't hit the vehicles that shot them 
    public List<Collider> GetOrdnanceColliders()
    {
        if (ModelMapping != null)
        {
            List<Collider> R = ModelMapping.GetCollidersByLayer(LibSWBF2.Enums.ECollisionMaskFlags.Ordnance);
            if (R.Count == 0)
            {
                return null;
            }
            else 
            {
                return R;
            }
        }
        return null;
    }




    public virtual Vector3 GetCameraPosition()
    {
        return Sections[0].GetCameraPosition();
    }

    public virtual Quaternion GetCameraRotation()
    {
        return Sections[0].GetCameraRotation();
    }


    // Not sure if/how some of these should be implemented
    public override void BindEvents(){}
    public override void Fixate(){}
    public override IPhxWeapon GetPrimaryWeapon(){ return null; }
    public void AddAmmo(float amount){}
    public override void PlayIntroAnim(){}
    public PhxInstance GetAim(){ return Aim; }
    void StateFinished(int layer){}
    public void AddHealth(float amount){}
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

    public PhxPropertySection Weapons = new PhxPropertySection(
        "WEAPONSECTION",        
        ("WeaponName",    new PhxProp<string>(null)),
        ("WeaponAmmo",    new PhxProp<int>(0)),
        ("WeaponChannel", new PhxProp<int>(0))
        
    );

    public PhxProp<string> HealthType = new PhxProp<string>("vehicle");
	public PhxProp<float>  MaxHealth = new PhxProp<float>(100.0f);

    // In the "human" bank with name: "human_{Pilot9Pose}" ?
    public PhxProp<string> Pilot9Pose = new PhxProp<string>("");

    // animation bank containing vehicle 9Pose
    public PhxProp<string> AnimationName = new PhxProp<string>("");

    // vehicle 9Pose
    public PhxProp<string> FinAnimation = new PhxProp<string>("");

    public PhxProp<string> GeometryName = new PhxProp<string>("");


    public PhxMultiProp SoldierCollision = new PhxMultiProp(typeof(string));
    public PhxMultiProp BuildingCollision = new PhxMultiProp(typeof(string));
    public PhxMultiProp VehicleCollision = new PhxMultiProp(typeof(string));
    public PhxMultiProp OrdnanceCollision = new PhxMultiProp(typeof(string));
    public PhxMultiProp TargetableCollision = new PhxMultiProp(typeof(string));

    public PhxProp<string> VehicleType = new PhxProp<string>("light");
    public PhxProp<string> AISizeType =  new PhxProp<string>("MEDIUM");
}














