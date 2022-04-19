using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;



public class PhxVehicleTurret : PhxSeat
{
    /*
	string TurretYawSound = "";
    float TurretYawSoundPitch = 0.7f;
    string TurretPitchSound = "";
    float TurretPitchSoundPitch = 0.7f;
    string TurretAmbientSound = "";
    string TurretActivateSound = "vehicle_equip";
    string TurretDeactivateSound = "vehicle_equip";
    string TurretStartSound = "";
    string TurretStopSound = "";
    */

    bool CanRotate = true;


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
                BaseTransform = UnityUtils.FindChildTransform(Owner.GetRootTransform(), values[i]);
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

        CanRotate = (Owner as MonoBehaviour).gameObject.transform != BaseTransform;
    }


    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (Occupant == null || !CanRotate) return;

        var axes = PlayerInput.GetVehicleAxesDelta(); // TODO: Get rid of this!
        BaseTransform.rotation *= Quaternion.Euler(new Vector3(0f, axes.View.X.Value, 0f));
    }
}
