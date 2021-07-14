
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxHoverMainSection : PhxVehicleSection
{
    public PhxHoverMainSection(uint[] properties, string[] values, ref int i, PhxHover hv, bool print = false)
    {
        Index = 0;

        OwnerVehicle = hv;
        BaseTransform = hv.gameObject.transform;

        WeaponSystems = new List<PhxWeaponSystem>();
        WeaponSystems.Add(new PhxWeaponSystem(this));
        

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
                    WeaponSystems.Add(new PhxWeaponSystem(this));

                    // New WeapSec indicates that the last defined aimer should be added to
                    // the previous WeapSec
                    WeaponSystems[WeaponIndex].AddAimer(CurrAimer);  

                    // With a new WeaponSection comes a new default aimer? 
                    CurrAimer = new PhxAimer();                  
                }

                WeaponIndex = newSlot - 1;
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
    } 


    // Returns a Vector3 where x = strafe input, y = drive input, and z = turn input.
    public Vector3 GetDriverInput()
    {
        if (Occupant == null)
        {
            return Vector3.zero;
        }
        else 
        {
            PhxPawnController Controller = Occupant.GetController();
            return new Vector3(Controller.MoveDirection.x, Controller.MoveDirection.y, Controller.mouseX);
        }
    }
}
