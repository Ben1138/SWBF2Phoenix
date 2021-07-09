using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;

/*
All VehicleSections have camera properties, movement ranges, and pilot configurations.
Many have Weapons and related Aimers.
They can be followed by cameras, entered, exited, hopped to/from by different pilots,
and each maintains a ref to its parent vehicle.

As of now, the Lua inaccessible properties are read directly from the EntityClass properties and values,
which are passed by the parent vehicle to the constructor.  The current reflection based assignment system
doesn't map well to implied groups eg Aimers, Springs. Lua accessible properties will use the 
reflection system and but will be limited to the parent vehicle.

Each frame the parent vehicle will call Update() on each section. 
*/

public abstract class PhxVehicleSection : IPhxTrackable
{    
    static PhxCamera CAM => PhxGameRuntime.GetCamera();


    protected PhxHover OwnerVehicle;
    protected Transform BaseTransform;

    public PhxSoldier Occupant;
    private PhxInstance Aim;


    public bool CanExit = true;


    protected Transform PilotPosition;
    protected string PilotAnimation = "";


    protected Vector3 EyePointOffset;
    protected Vector3 TrackCenter;
    protected Vector3 TrackOffset;
    protected float TiltValue;

    protected Vector2 PitchLimits;
    protected Vector2 YawLimits;


    protected int Index;


    // Accumulators for view control
    protected float PitchAccum;
    protected float YawAccum;


    // View position in local space
    protected Vector3 ViewPoint;

    // View direction in local space 
    protected Vector3 ViewDirection = Vector3.forward;

    public Vector3 GetCameraPosition()
    {
        return BaseTransform.transform.TransformPoint(ViewPoint);
    }

    public Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(BaseTransform.TransformDirection(ViewDirection), Vector3.up);
    }


    protected List<PhxWeaponSystem> WeaponSystems;



    public virtual void Update()
    {
        PhxPawnController Controller;
        if (Occupant == null || ((Controller = Occupant.GetController()) == null)) 
        {
            return; 
        }

        if (Controller.SwitchSeat && OwnerVehicle.TrySwitchSeat(Index))
        {
            Occupant = null;
            Controller.SwitchSeat = false;
            return;
        }

        if (Controller.TryEnterVehicle && OwnerVehicle.Eject(Index))
        {
            Occupant = null;
            Controller.TryEnterVehicle = false;
            return;
        }
        

        if (Controller.ShootPrimary)
        {   
            if (WeaponSystems.Count > 0)
            {
                WeaponSystems[0].Fire();                
            }
        }

        if (Controller.ShootSecondary)
        {
            if (WeaponSystems.Count > 1)
            {
                WeaponSystems[1].Fire();
            }
        }

        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, PitchLimits.x, PitchLimits.y);

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, YawLimits.x, YawLimits.y);        


        // These need work, camera behaviour is slightly off and NormalDirection 
        // hasn't been incorporated yet.  But I'm satisfied for now.  The commented
        // aat's and Darth D.U.C.K's tut are both incomplete and wrong in some places w.r.t
        // camera behaviour...
        Vector3 CameraOffset = TrackOffset;
        CameraOffset.z *= -1f;

        ViewPoint = TrackCenter + Quaternion.Euler(3f * PitchAccum, 0f, 0f) * CameraOffset;
        ViewDirection =  Quaternion.Euler(3f * PitchAccum, 0f, 0f) * Quaternion.Euler(-TiltValue, 0f, 0f) * Vector3.forward;


        Vector3 TargetPos = BaseTransform.transform.TransformPoint(30f * ViewDirection + ViewPoint);

        
        if (Physics.Raycast(TargetPos, TargetPos - CAM.transform.position, out RaycastHit hit, 1000f))
        {
            TargetPos = hit.point;

            PhxInstance GetInstance(Transform t)
            {
                PhxInstance inst = t.gameObject.GetComponent<PhxInstance>();
                if (inst == null && t.parent != null)
                {
                    return GetInstance(t.parent);
                }
                return inst;
            }

            Aim = GetInstance(hit.collider.gameObject.transform);
        }
        

        foreach (PhxWeaponSystem System in WeaponSystems)
        {
            System.Update(TargetPos);
        }
    }


    public bool HopToNextSeat()
    {
        if (OwnerVehicle == null) {return false;}

        bool r = OwnerVehicle.TrySwitchSeat(Index);
        if (r)
        {
            ClearOccupant();
        }
        return r;
    }


    public void ClearOccupant()
    {
        Occupant = null;
    }


    public bool SetOccupant(PhxSoldier s) 
    {
        if (Occupant != null) return false;
        
        Occupant = s;

        s.SetPilot(PilotPosition,"");

        return true;
    }
}
