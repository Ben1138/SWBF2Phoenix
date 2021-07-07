
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
        return OwnerVehicle.transform.TransformPoint(new Vector3(0f, 3f, -6f));
    }

    public override Quaternion GetCameraRotation()
    {
        return Quaternion.LookRotation(OwnerVehicle.transform.TransformDirection(ViewDirection), Vector3.up);
    }


    PhxWeaponSystem[] WeaponSystems;

    public PhxHoverMainSection(uint[] properties, string[] values, ref int i, PhxHover hv, bool print = false)
    {
        Index = 0;

        OwnerVehicle = hv;

        WeaponSystems = new PhxWeaponSystem[2];
        WeaponSystems[0] = new PhxWeaponSystem(this);
        WeaponSystems[1] = new PhxWeaponSystem(this);

        PhxAimer CurrAimer = new PhxAimer();

        int WeaponIndex = 0;
        int AimerIndex = 0;

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
            else if (properties[i] == HashUtils.GetFNV("FirePointName"))
            {
                CurrAimer.FirePoint = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                WeaponSystems[WeaponIndex].AddAimer(CurrAimer);
                CurrAimer = new PhxAimer();
            }
            else if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
            {
                int newSlot = Int32.Parse(values[i]) - 1;
                
                if (WeaponIndex != newSlot)
                {
                    WeaponSystems[WeaponIndex].AddAimer(CurrAimer);  
                    CurrAimer = new PhxAimer();                  
                }

                WeaponIndex = newSlot;
            }   
            else if (properties[i] == HashUtils.GetFNV("WeaponName"))
            {
                WeaponSystems[WeaponIndex].SetWeapon(values[i]);
            }         
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("CHUNKSECTION"))
            {
                WeaponSystems[WeaponIndex].AddAimer(CurrAimer);                    
                break;   
            }
        }

        TrackOffset = new Vector3(0f,0f,20f);
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


        if (Controller.SwitchSeat && OwnerVehicle.TrySwitchSeat(0))
        {
            Occupant = null;
            Controller.SwitchSeat = false;
            return;
        }

        if (Controller.TryEnterVehicle && OwnerVehicle.Eject(0))
        {
            Occupant = null;
            return;
        }
        


        if (Controller.ShootPrimary)
        {
            if (WeaponSystems[0] != null)
            {
                WeaponSystems[0].Fire();
            }
        }


        if (Controller.ShootSecondary)
        {
            if (WeaponSystems[1] != null)
            {
                WeaponSystems[1].Fire();
            }
        }



        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, -8f, 25f);

        bool printAimer = Controller.mouseY < 0f;

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, 0f, 0f);        

        ViewDirection = Quaternion.Euler(3f * PitchAccum, 0f, 0f) * TrackOffset;


        Vector3 TargetPos = OwnerVehicle.transform.TransformPoint(30f * ViewDirection);

        
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
        

        WeaponSystems[0].Update(TargetPos);
        WeaponSystems[1].Update(TargetPos);
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
