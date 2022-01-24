using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;
using LibSWBF2.Utils;


/*
Implement if you want object to be followed by the camera.
Added so that PhxVehicleSeats, which are not Lua-accessible instances,
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




public abstract class PhxVehicle : PhxControlableInstance<PhxVehicleProperties>, 
                                    IPhxTrackable, 
                                    IPhxSeatable,
                                    IPhxTickablePhysics,
                                    IPhxTickable 
{
    protected static PhxGame GAME => PhxGame.Instance;
    protected static PhxMatch MTC => PhxGame.GetMatch();
    protected static PhxScene SCENE => PhxGame.GetScene();
    protected static PhxCamera CAM => PhxGame.GetCamera();

    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);



    public Action<PhxInstance> OnDeath;

    protected float SliceProgress;


    protected PhxSoldier Driver;
    protected PhxInstance Aim;

    protected List<PhxSeat> Seats;


    protected SWBFModel ModelMapping = null;



    public override void Init()
    {
        /*
        COLLISION
        */

        ModelMapping = ModelLoader.Instance.GetModelMapping(gameObject, C.GeometryName); 

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
        ModelMapping.ConvexifyMeshColliders();

        C.EntityClass.GetAllProperties(out uint[] properties, out string[] values);
    }


    public virtual void Tick(float deltaTime){}
    public virtual void TickPhysics(float deltaTime){}


    protected void SetIgnoredCollidersOnAllWeapons()
    {
        List<Collider> R = ModelMapping.GetCollidersByLayer(LibSWBF2.Enums.ECollisionMaskFlags.Ordnance);

        foreach (PhxSeat Seat in Seats)
        {
            foreach (PhxWeaponSystem WeaponSystem in Seat.WeaponSystems)
            {
                if (WeaponSystem.Weapon != null)
                {
                    WeaponSystem.Weapon.SetIgnoredColliders(R);
                }
            }
        }
    }





    // Will return true if vehicle is sliceable.  Out param is SliceProgress
    public virtual bool IncrementSlice(out float progress)
    {
        SliceProgress += 5.0f;
        progress = SliceProgress;
        return true;
    }


    // Dealing with SWBF's use of concave colliders on physics objects will be a major challenge
    // unless we can reliably use the primitives found on each imported model...
    protected List<MeshCollider> GetMeshColliders(Transform tx)
    {
        List<MeshCollider> Result = new List<MeshCollider>();
        MeshCollider coll = tx.gameObject.GetComponent<MeshCollider>();
        if (coll != null && coll.convex)
        {
            Result.Add(coll);
        }

        for (int j = 0; j < tx.childCount; j++)
        {
            Result.AddRange(GetMeshColliders(tx.GetChild(j)));
        }

        return Result;
    }


    public bool HasAvailableSeat()
    {
        return GetNextAvailableSeat() != -1;
    }


    public int GetNextAvailableSeat(int startIndex = -1)
    {
        int numSeats = Seats.Count;

        if (numSeats == 0)
        {
            // This isn't error worthy, plenty of armedbuildings dont have any seats
            return -1;
        }


        int i = (startIndex + 1) % numSeats;
        
        while (i != startIndex)
        {
            if (Seats[i].Occupant == null)
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
            Seats[seat].SetOccupant(Seats[index].Occupant);
            Seats[seat].Occupant.SetPilot(Seats[seat]);
            Seats[index].Occupant = null;
            CAM.Track(Seats[seat]);

            return true;
        }
    }


    public PhxSeat TryEnterVehicle(PhxSoldier soldier)
    {
        // Find first available seat
        int seat = GetNextAvailableSeat();

        if (seat == -1)
        {
            return null;
        }
        else 
        {
            Seats[seat].SetOccupant(soldier);
            PhxGame.GetCamera().Track(Seats[seat]);
            
            return Seats[seat];
        }
    }


    public bool Eject(int i)
    {
        if (i < Seats.Count || Seats[i] != null || Seats[i].Occupant != null)
        {
            Seats[i].Occupant.SetFree(transform.position + Vector3.up * 2.0f);
            CAM.Follow(Seats[i].Occupant);
            Seats[i].Occupant = null;

            return true;
        }
        else 
        {
            return false;
        }
    }

    public Transform GetRootTransform()
    {
        return transform;
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
        return Seats[0].GetCameraPosition();
    }

    public virtual Quaternion GetCameraRotation()
    {
        return Seats[0].GetCameraRotation();
    }

    public override void Destroy()
    {

    }


    // Not sure if/how some of these should be implemented
    public override void Fixate(){}
    public override IPhxWeapon GetPrimaryWeapon(){ return null; }
    public void AddAmmo(float amount){}
    public override void PlayIntroAnim(){}
    public PhxInstance GetAim(){ return Aim; }
    void StateFinished(int layer){}
    public void AddHealth(float amount){}
}
