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

    // Max 2, min 1
    protected List<PhxWeaponSystem> WeaponSystems;

    // Vehicle to which section belongs
    protected PhxVehicle OwnerVehicle;

    // Transform that moves with the pilot's input 
    // (will have to build in MountPoint soon for turrets)
    protected Transform BaseTransform;

    public PhxSoldier Occupant;
    private PhxInstance Aim;

    // eg with flyers one cannot exit
    public bool CanExit = true;

    // Pilot params common to all sections
    public Transform PilotPosition { get; protected set; }
    public string PilotAnimation { get; protected set; }
    public string Pilot9Pose { get; protected set; }

    // Camera control values, common to all sections
    protected Vector3 EyePointOffset;
    protected Vector3 TrackCenter;
    protected Vector3 TrackOffset;
    protected float TiltValue;

    protected Vector2 PitchLimits = new Vector2(0f,360f);
    protected Vector2 YawLimits = new Vector2(0f,360f);

    // Index in OwnerVehicle's Sections list
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
        // AAT and Darth D.U.C.K's tut are both incomplete and wrong in some places w.r.t
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

        return true;
    }


    public bool IsOccupied()
    {
        return Occupant != null;
    }



    protected PhxVehicleSection(uint[] properties, string[] values, ref int i, PhxVehicle v, int SectionIndex)
    {
        Index = SectionIndex;

        OwnerVehicle = v;
        BaseTransform = v.gameObject.transform;

        WeaponSystems = new List<PhxWeaponSystem>();
        WeaponSystems.Add(new PhxWeaponSystem(this));
        int WeaponIndex = 0;
        
        PhxAimer CurrAimer = new PhxAimer();


        while (++i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("EyePointOffset"))
            {
                EyePointOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TrackCenter"))
            {
                TrackCenter = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("YawLimits"))
            {
                YawLimits = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("PitchLimits"))
            {
                PitchLimits = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TiltValue"))
            {
                TiltValue = float.Parse(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TrackOffset"))
            {
                TrackOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("HierarchyLevel"))
            {
                CurrAimer.HierarchyLevel = Int32.Parse(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerPitchLimits"))
            {
                CurrAimer.PitchRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerYawLimits"))
            {
                CurrAimer.YawRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("BarrelNodeName"))
            {
                CurrAimer.BarrelNode = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);
            }     
            else if (properties[i] == HashUtils.GetFNV("AimerNodeName"))
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("FirePointName"))
            {
                CurrAimer.FirePoint = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                WeaponSystems[WeaponIndex].AddAimer(CurrAimer);
                CurrAimer = new PhxAimer();
            }
            else if (properties[i] == HashUtils.GetFNV("PilotPosition"))
            {
                PilotPosition = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);   
            }  
            else if (properties[i] == HashUtils.GetFNV("PilotAnimation"))
            {
                PilotAnimation = values[i];
            }            
            else if (properties[i] == HashUtils.GetFNV("Pilot9Pose"))
            {
                Pilot9Pose = values[i];
            }
            else if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
            {
                int newSlot = Int32.Parse(values[i]);
                
                if (newSlot > WeaponSystems.Count)
                {
                    WeaponSystems[WeaponIndex].AddAimer(CurrAimer);

                    WeaponSystems.Add(new PhxWeaponSystem(this));
                    WeaponIndex = newSlot - 1;

                    // New WeapSec indicates that the last defined aimer should be added to
                    // the previous WeapSec
                    CurrAimer = new PhxAimer();                  
                }

            }   
            else if (properties[i] == HashUtils.GetFNV("WeaponName"))
            {
                WeaponSystems[WeaponIndex].SetWeapon(values[i]);
            }         
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("CHUNKSECTION")  || 
                    properties[i] == HashUtils.GetFNV("WALKERSECTION") ||
                    properties[i] == HashUtils.GetFNV("TURRETSECTION") ||
                    properties[i] == HashUtils.GetFNV("BUILDINGSECTION")||
                    i == properties.Length - 1)
            {
                WeaponSystems[WeaponIndex].AddAimer(CurrAimer);  
                break;   
            }
        }
    } 
}
