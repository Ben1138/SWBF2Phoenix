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


    public PhxVehicleTurret(PhxVehicle V, int index) : base(V,index){}


    public override void InitManual(EntityClass EC, int StartIndex, string Header, string HeaderValue)
    {
        base.InitManual(EC, StartIndex, Header, HeaderValue);

        EC.GetAllProperties(out uint[] properties, out string[] values);

        int i = StartIndex;

        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("TurretNodeName"))
            {
                BaseTransform = UnityUtils.FindChildTransform(OwnerVehicle.transform, values[i]);
            }      
            else if (properties[i] == HashUtils.GetFNV("BUILDINGSECTION") ||
                    properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("WALKERSECTION") ||
                    properties[i] == HashUtils.GetFNV("TURRETSECTION"))
            {
                if (properties[i] == HashUtils.GetFNV(Header) &&
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

        // Turrets use just five poses out of nine given
        if (PilotAnimationType == PilotAnimationType.NinePose)
        {
            PilotAnimationType = PilotAnimationType.FivePose;
        }
    }


    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (Occupant == null) return;

        BaseTransform.rotation *= Quaternion.Euler(new Vector3(0f,Occupant.GetController().mouseX,0f));
    }
}
