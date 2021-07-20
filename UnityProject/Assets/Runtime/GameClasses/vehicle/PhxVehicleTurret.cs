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


    public PhxVehicleTurret(uint[] properties, string[] values, 
                            ref int i, PhxVehicle parentVehicle, int sectionIndex) : 
                            base(properties, values, ref i, parentVehicle, sectionIndex)

    {
        int EndIndex = i;

        // Goofy, but didn't want to use static pattern and couldn't call base() midway through...
        // A more robust soln is coming
        while (--i > 0)
        {
            if (properties[i] == HashUtils.GetFNV("TurretNodeName"))
            {
                BaseTransform = UnityUtils.FindChildTransform(parentVehicle.transform, values[i]);
            }      
            else if (properties[i] == HashUtils.GetFNV("BUILDINGSECTION") ||
                    properties[i] == HashUtils.GetFNV("CHUNKSECTION") ||
                    properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("WALKERSECTION") ||
                    properties[i] == HashUtils.GetFNV("TURRETSECTION"))
            {
                break;
            }
        }

        // Turrets use just five poses out of nine given
        if (PilotAnimationType == PilotAnimationType.NinePose)
        {
            PilotAnimationType = PilotAnimationType.FivePose;
        }

        i = EndIndex;
    }


    public override void Update()
    {
        base.Update();

        if (Occupant == null) return;

        BaseTransform.rotation *= Quaternion.Euler(new Vector3(0f,Occupant.GetController().mouseX,0f));
    }
}
