using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;



public class PhxVehicleTurret : PhxVehicleSection
{    
	string TurretYawSound = "";
    float TurretYawSoundPitch = 0.7f;
    string TurretPitchSound = "";
    float TurretPitchSoundPitch = 0.7f;
    string TurretAmbientSound = "";
    string TurretActivateSound = "vehicle_equip";
    string TurretDeactivateSound = "vehicle_equip";
    string TurretStartSound = "";
    string TurretStopSound = "";


    public PhxVehicleTurret(uint[] properties, string[] values, ref int i, Transform parentVehicle, int sectionIndex)
    {
        Index = sectionIndex;

        PhxWeaponSystem Weapon = new PhxWeaponSystem(this);

        WeaponSystems = new List<PhxWeaponSystem>();
        WeaponSystems.Add(Weapon);

        //BaseTransform = TurretNode;

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
                BaseTransform = UnityUtils.FindChildTransform(parentVehicle, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("YawLimits"))
            {
                YawLimits = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("PitchLimits"))
            {
                PitchLimits = PhxUtils.Vec2FromString(values[i]);
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
            else if (properties[i] == HashUtils.GetFNV("TiltValue"))
            {
                TiltValue = float.Parse(values[i]);
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
            else if (properties[i] == HashUtils.GetFNV("NextAimer"))
            {
            	Weapon.AddAimer(CurrAimer);
            	CurrAimer = new PhxAimer();
            }  
            else if (properties[i] == HashUtils.GetFNV("WeaponName"))
            {
                Weapon.SetWeapon(values[i]);
            }         
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("CHUNKSECTION"))
            {
                Weapon.AddAimer(CurrAimer);
                break;
            }
        }
    }


    public override void Update()
    {
        if (Occupant == null) return;

        base.Update();
        BaseTransform.rotation = Quaternion.Euler(new Vector3(0f,YawAccum,0f));
    }
}
