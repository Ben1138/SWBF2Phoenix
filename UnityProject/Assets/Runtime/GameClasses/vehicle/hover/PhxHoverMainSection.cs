
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHoverMainSection : PhxVehicleSection
{
    static PhxCamera CAM => PhxGameRuntime.GetCamera();


    public override Vector3 GetCameraPosition()
    {
        return OwnerVehicle.transform.TransformPoint(TrackCenter);
    }

    public override Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(OwnerVehicle.transform.TransformDirection(ViewDirection), Vector3.up);
    }


    List<PhxWeaponSystem> Weapons;

    public PhxHoverMainSection(uint[] properties, string[] values, ref int i, PhxHover hv, bool print = false)
    {
        Index = 0;

        PhxAimer CurrAimer = new PhxAimer();

        OwnerVehicle = hv;

        //Weapons = new List<PhxVehicleWeapon>();
        int WeaponIndex = 0;

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

            if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
            {
                WeaponIndex = Int32.Parse(values[i]) - 1;

                if (WeaponIndex == 1)
                {
                    AddAimer(CurrAimer);
                    CurrAimer = new PhxAimer();
                }

            }            

            if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                properties[i] == HashUtils.GetFNV("CHUNKSECTION") || 
                properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                if (WeaponIndex == 0) AddAimer(CurrAimer);

                //AddAimer(CurrAimer);
                
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

        ViewDirection = TrackOffset - TrackCenter;


        //foreach (var aimer in Aimers)
        //{
        //    Debug.Log(aimer.ToString());
        //}
    } 



    // Current top-level Aimer
    int CurrAimer = 0;
    PhxPawnController Controller;

    private PhxInstance Aim;

    public override void Update()
    {
        if (Occupant == null) return; 

        Controller = Occupant.GetController();

        if (Controller == null) return;


        

        if (Controller.SwitchSeat && OwnerVehicle.TrySwitchSeat(Index))
        {
            Occupant = null;
            return;
        }

        if (Controller.TryEnterVehicle && OwnerVehicle.Eject(0)) return;
        


        if (Controller.ShootPrimary)
        {
            if (Aimers.Count > 0)
            {
                if (Aimers[CurrAimer].Fire())
                {
                    CurrAimer++;
                }

                if (CurrAimer >= Aimers.Count)
                {
                    CurrAimer = 0;
                }
            }
        }


        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, -8f, 25f);

        bool printAimer = Controller.mouseY < 0f;

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, 0f, 0f);        

        ViewDirection = Quaternion.Euler(3f * PitchAccum, 0f, 0f) * TrackOffset;


        Vector3 TargetPos = OwnerVehicle.transform.TransformPoint(30f * ViewDirection);

        /*
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
        */


        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos, printAimer);
            Aimer.UpdateBarrel();
        }
    }



    public Vector3 GetDriverInput()
    {
        if (Occupant == null)
        {
            return Vector3.zero;
        }
        else 
        {
            Controller = Occupant.GetController();
            return new Vector3(Controller.MoveDirection.x, Controller.MoveDirection.y, Controller.mouseX);
        }
    }
}
