using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;




public enum PilotAnimationType : int 
{
    NinePose,
    FivePose,
    StaticPose,
    None
}


public interface IPhxSeatable
{
    public bool HasAvailableSeat();

    public int GetNextAvailableSeat(int startIndex = -1);

    public bool TrySwitchSeat(int index);

    public PhxSeat TryEnterVehicle(PhxSoldier soldier);

    public bool Eject(int i);

    public Transform GetRootTransform();
}
    



public abstract class PhxSeat : IPhxTrackable, IPhxTickable
{    
    protected static PhxCamera CAM => PhxGame.GetCamera();

    // Max 2, min 1
    public List<PhxWeaponSystem> WeaponSystems;

    // Vehicle to which section belongs
    public IPhxSeatable Owner { get; protected set; }

    // Transform that moves with the pilot's input 
    // (will have to build in MountPoint soon for turrets)
    protected Transform BaseTransform;

    public PhxSoldier Occupant;
    protected PhxInstance Aim;

    // eg with flyers one cannot exit
    public bool CanExit = true;

    // Pilot params common to all sections
    public Transform PilotPosition { get; protected set; }
    public string PilotAnimation { get; protected set; }
    public string Pilot9Pose { get; protected set; }

    // How to interpret pilot anim fields
    public PilotAnimationType PilotAnimationType { get; protected set; } = PilotAnimationType.None;

    // Camera control values, common to all sections
    protected Vector3 EyePointOffset;
    protected Vector3 TrackCenter;
    protected Vector3 TrackOffset;
    protected float TiltValue;

    protected Vector2 PitchLimits = new Vector2(0f,0f);
    protected Vector2 YawLimits = new Vector2(0f,0f);

    // Index in OwnerVehicle's Sections list
    protected int Index;

    // Accumulators for view control
    protected float PitchAccum;
    protected float YawAccum;

    // View position in local space
    protected Vector3 ViewPoint;

    // View direction in local space 
    protected Vector3 ViewDirection = Vector3.forward;

    public virtual Vector3 GetCameraPosition()
    {
        return BaseTransform.transform.TransformPoint(ViewPoint);
    }

    public virtual Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(BaseTransform.TransformDirection(ViewDirection), Vector3.up);
    }


    public virtual void Tick(float deltaTime)
    {
        PhxPawnController Controller;
        if (Occupant == null || ((Controller = Occupant.GetController()) == null)) 
        {
            return; 
        }

        if (Controller.SwitchSeat && Owner.TrySwitchSeat(Index))
        {
            Occupant = null;
            Controller.SwitchSeat = false;
            return;
        }

        if (Controller.Enter && Owner.Eject(Index))
        {
            Occupant = null;
            Controller.Enter = false;
            return;
        }
        

        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, PitchLimits.x, PitchLimits.y);

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, YawLimits.x, YawLimits.y);        


        // These need work, camera behaviour is slightly off and NormalDirection 
        // hasn't been incorporated yet.  But I'm satisfied for now.  The commented
        // AAT and Darth D.U.C.K's tut are both incomplete and wrong in some places...
        Vector3 CameraOffset = TrackOffset;
        CameraOffset.z *= -1f;

        ViewPoint = TrackCenter + Quaternion.Euler(3f * PitchAccum, 0f, 0f) * CameraOffset;
        ViewDirection =  Quaternion.Euler(3f * PitchAccum, 0f, 0f) * Quaternion.Euler(-TiltValue, 0f, 0f) * Vector3.forward;


        Vector3 TargetPos = BaseTransform.transform.TransformPoint(30000f * ViewDirection + ViewPoint);

        
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

        // Update aimers for each weapon system
        foreach (PhxWeaponSystem System in WeaponSystems)
        {
            System.Update(TargetPos);
        }


        if (Controller.ShootPrimary)
        {   
            if (WeaponSystems.Count > 0)
            {
                WeaponSystems[0].Fire(TargetPos);                
            }
        }

        if (Controller.ShootSecondary)
        {
            if (WeaponSystems.Count > 1)
            {
                WeaponSystems[1].Fire(TargetPos);
            }
        }
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


    protected PhxSeat(IPhxSeatable v, int index)
    {
        Owner = v;
        BaseTransform = v.GetRootTransform();
        Index = index;
    }


    public virtual void InitManual(EntityClass EC, int StartIndex, string HeaderName, string HeaderValue)
    {
        EC.GetAllProperties(out uint[] properties, out string[] values);

        WeaponSystems = new List<PhxWeaponSystem>();
        WeaponSystems.Add(new PhxWeaponSystem(this));
        int WeaponIndex = 0;
        bool WeaponsSet = false;
        
        int i = StartIndex;

        while (i < properties.Length)
        {
            if (properties[i] == 0x41568c97 /*EyePointOffset*/)
            {
                EyePointOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == 0xe85d5895 /*TrackCenter*/)
            {
                TrackCenter = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == 0x2c3e8078 /*YawLimits*/)
            {
                YawLimits = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == 0x3403b139 /*PitchLimits*/)
            {
                PitchLimits = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == 0x359d5227 /*TiltValue*/)
            {
                TiltValue =  PhxUtils.FloatFromString(values[i]);
            }
            else if (properties[i] == 0xfd3d9507 /*TrackOffset*/)
            {
                TrackOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == 0x51ca39a6 /*PilotPosition*/)
            {
                PilotPosition = UnityUtils.FindChildTransform(Owner.GetRootTransform(), values[i]);   
            }  
            else if (properties[i] == 0x6e4fc069 /*PilotAnimation*/)
            {
                PilotAnimation = values[i];
                PilotAnimationType = PilotAnimationType.StaticPose;
            }            
            else if (properties[i] == 0xa976d065 /*Pilot9Pose*/)
            {
                Pilot9Pose = values[i];
                PilotAnimationType = PilotAnimationType.NinePose;
            }
            else if (properties[i] == 0xd0329e80 /*WEAPONSECTION*/)
            {
                WeaponsSet = true;

                int newSlot = Int32.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
                
                if (newSlot > WeaponSystems.Count)
                {
                    WeaponSystems.Add(new PhxWeaponSystem(this));
                    WeaponIndex = newSlot - 1;
                }

                WeaponSystems[WeaponIndex].InitManual(EC, i, values[i]);                
            }          
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("WALKERSECTION") ||
                    //properties[i] == HashUtils.GetFNV("TURRETSECTION") ||
                    properties[i] == HashUtils.GetFNV("BUILDINGSECTION"))
            {
                if (properties[i] == HashUtils.GetFNV(HeaderName) &&
                    values[i].Equals(HeaderValue, StringComparison.OrdinalIgnoreCase))
                {
                    // nada
                }
                else
                {
                    break;   
                }
            }

            i++;
        }

        if (!WeaponsSet)
        {
            WeaponSystems[0].InitManual(EC, StartIndex + 1);
        }
    } 
}
