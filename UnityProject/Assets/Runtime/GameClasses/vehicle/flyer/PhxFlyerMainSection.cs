
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Wrappers;
using System.Runtime.ExceptionServices;

public class PhxFlyerMainSection : PhxSeat
{
	public float PitchRate;
	public float TurnRate;


    public PhxFlyerMainSection(PhxFlyer Hover) : base(Hover, 0){} 

    public override void InitManual(EntityClass EC, int StartIndex, string Header, string HeaderValue)
    {
		EC.GetAllProperties(out uint[] properties, out string[] values);

        WeaponSystems = new List<PhxWeaponSystem>();
        WeaponSystems.Add(new PhxWeaponSystem(this));
        int WeaponIndex = 0;
        bool WeaponsSet = false;

        Transform OwnerTx = (Owner as MonoBehaviour).gameObject.transform;
        var Bounds = UnityUtils.GetMaxBounds(OwnerTx.gameObject);
        TrackOffset = new Vector3(0f, Bounds.max.y, Bounds.max.z) - new Vector3(0f, OwnerTx.position.y, OwnerTx.position.z);
        TrackOffset *= 5f;

        bool IsTrackOffsetSet = false;
        
        //int i = StartIndex;

        bool Set = true;

        for (int i = StartIndex; i < properties.Length; i++)
        {
        	if (properties[i] == HashUtils.GetFNV(Header))
            {
                if (HeaderValue == "BODY")
                {
                	Set = true;
                }
                else 
                {
                	Set = false;
                }
            }

            if (!Set)
            {
            	continue;
            }

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
            else if (properties[i] == HashUtils.GetFNV("PitchRate"))
            {
                PitchRate = PhxUtils.FloatFromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TurnRate"))
            {
                TurnRate = PhxUtils.FloatFromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TiltValue"))
            {
                TiltValue = float.Parse(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TrackOffset"))
            {
                TrackOffset = PhxUtils.Vec3FromString(values[i]);
                IsTrackOffsetSet = true;
            }
            else if (properties[i] == HashUtils.GetFNV("PilotPosition"))
            {
                PilotPosition = UnityUtils.FindChildTransform(Owner.GetRootTransform(), values[i]);   
            }  
            else if (properties[i] == HashUtils.GetFNV("PilotAnimation"))
            {
                PilotAnimation = values[i];
                PilotAnimationType = PilotAnimationType.StaticPose;
            }            
            else if (properties[i] == HashUtils.GetFNV("Pilot9Pose"))
            {
                Pilot9Pose = values[i];
                PilotAnimationType = PilotAnimationType.NinePose;
            }
            else if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
            {
                WeaponsSet = true;

                int newSlot = Int32.Parse(values[i]);
                
                if (newSlot > WeaponSystems.Count)
                {
                    WeaponSystems.Add(new PhxWeaponSystem(this));
                    WeaponIndex = newSlot - 1;
                }

                WeaponSystems[WeaponIndex].InitManual(EC, i, values[i]);                
            }          
        }

        if (!WeaponsSet)
        {
            WeaponSystems[0].InitManual(EC, StartIndex + 1);
        }    
    }


    public void Update()
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
        

        //PitchAccum += Controller.mouseY;
        //PitchAccum = Mathf.Clamp(PitchAccum, PitchLimits.x, PitchLimits.y);

        //YawAccum += Controller.mouseX;
        //YawAccum = Mathf.Clamp(YawAccum, YawLimits.x, YawLimits.y);        


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


    public override Vector3 GetCameraPosition()
    {
        return BaseTransform.transform.TransformPoint(ViewPoint);
    }

    public override Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(BaseTransform.TransformDirection(Vector3.forward), BaseTransform.up);
    }



    public PhxPawnController GetController()
    {
        return Occupant == null ? null : Occupant.GetController();
    }
}
