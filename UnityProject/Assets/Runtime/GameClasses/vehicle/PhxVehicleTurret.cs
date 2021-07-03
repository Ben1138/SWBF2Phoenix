using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;



public class PhxVehicleTurret : IPhxTrackable
{    
    public PhxHover OwnerVehicle = null;

    public PhxSoldier Occupant = null;

    public bool CanExit = true;


    public Transform PilotPosition = null;
    public Transform TurretNode = null;


    public Vector3 EyePointOffset;
    public Vector3 TrackCenter;
    public Vector3 TrackOffset;

    int Index;


    private List<PhxAimer> Aimers;


    public Vector3 GetCameraPosition()
    {
        return TurretNode.TransformPoint(EyePointOffset + TrackCenter);
    }

    public Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(TurretNode.forward, Vector3.up);
    }


    public PhxVehicleTurret(uint[] properties, string[] values, int i, Transform parentVehicle, int index)
    {
        Index = index;
        Aimers = new List<PhxAimer>();

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
                CurrAimer.Init();

                if (Aimers.Count > 0 && Aimers[Aimers.Count - 1].HierarchyLevel > CurrAimer.HierarchyLevel)
                {
                    Aimers[Aimers.Count - 1].ChildAimer = CurrAimer;
                }
                else 
                {
                    Aimers.Add(CurrAimer);
                }


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




    public bool SetOccupant(PhxSoldier s) 
    {
        if (Occupant != null) return false;
        
        Occupant = s;

        return true;
    }


    Vector3 ViewDirection;
    float PitchAccum = 0.0f;
    float YawAccum = 0.0f;


    public void Update()
    {
        if (Occupant == null) return;

        var Controller = Occupant.GetController();

        if (Controller.TryEnterVehicle)
        {
            if (CanExit)
            {
                Occupant.SetFree(OwnerVehicle.transform.position + Vector3.up * 2.0f);
                Occupant = null;
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
