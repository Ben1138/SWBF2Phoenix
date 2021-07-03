using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;



public class PhxVehicleTurret : PhxVehicleSection
{    


    public override Vector3 GetCameraPosition()
    {
        return TurretNode.transform.TransformPoint(EyePointOffset + TrackCenter);
    }

    public override Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(ViewDirection, Vector3.up);
    }



    //IPhxWeapon Weapon;
    Transform TurretNode = null;

    public PhxVehicleTurret(uint[] properties, string[] values, ref int i, Transform parentVehicle, int sectionIndex)
    {
        Index = sectionIndex;

        PhxAimer CurrAimer = new PhxAimer();

        OwnerVehicle = parentVehicle.GetComponent<PhxHover>();

        while (++i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("PilotPosition"))
            {
                PilotPosition = UnityUtils.FindChildTransform(parentVehicle, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TurretNodeName"))
            {
                TurretNode = UnityUtils.FindChildTransform(parentVehicle, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("EyePointOffset"))
            {
                EyePointOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TrackCenter"))
            {
                TrackCenter = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("TrackOffset"))
            {
                TrackOffset = PhxUtils.Vec3FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("HierarchyLevel"))
            {
                CurrAimer.HierarchyLevel = 1;
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
                CurrAimer.BarrelNode = UnityUtils.FindChildTransform(parentVehicle, values[i]);
            }     
            else if (properties[i] == HashUtils.GetFNV("AimerNodeName"))
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(parentVehicle, values[i]);
            }            
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("CHUNKSECTION") || 
                    properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                AddAimer(CurrAimer);

                if (properties[i] == HashUtils.GetFNV("NextAimer"))
                {
                    CurrAimer = new PhxAimer();
                }
                else 
                {
                    break;
                }
            }
        }
    }



    public override void Update()
    {
        if (Occupant == null) return;

        var Controller = Occupant.GetController();

        if (Controller.TryEnterVehicle)
        {
            if (CanExit)
            {
                Occupant.SetFree(OwnerVehicle.transform.position + Vector3.up * 2.0f);
                Occupant = null;
                return;
            }
        }


        if (Controller.SwitchSeat && OwnerVehicle.TrySwitchSeat(Index))
        {
            Occupant = null;
            return;
        }


        if (Controller.ShootPrimary)
        {
            // fire
            if (Aimers.Count > 0)
            {
                Aimers[0].Fire();
            }
        }

        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, -8f, 25f);
        ViewDirection = Quaternion.Euler(3f * PitchAccum, 0f, 0f) * TrackOffset;
        Vector3 TargetPos = TurretNode.TransformPoint(ViewDirection);

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, -180f, 180f);  
        TurretNode.rotation = Quaternion.Euler(new Vector3(0f,YawAccum,0f));

        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos);
            Aimer.UpdateBarrel();
        }
    }
}
