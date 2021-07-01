/*

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;



public class PhxVehicleSection : MonoBehaviour
{
    
    PhxInstance OwnerVehicle = null;

    public bool IsBody = false;


    PhxSoldier Occupant = null;

    public bool CanExit = true;


    Transform PilotPosition = null;

    string NinePoseAnim = null;

    // Second one will be null in the case of turrets
    IPhxWeapon[] Weapons = new IPhxWeapon[2];

    Vector3 CameraLocation;
    Vector3 CameraFocalPoint;


    public bool HasOccupant() 
    {
        return Occupant == null; 
    }

    public bool SetOccupant(PhxSoldier s) 
    {
        if (Occupant != null) return false;
        
        Occupant = s;


        s.GetController().ViewDirection = Vector3.forward;


        return true;
    }


    public void SetProperties(Dictionary<string, IPhxPropRef> section, PhxInstance Owner)
    {
        //OwnerVehicle = Owner;

        if (section.TryGetValue("PilotPosition", out IPhxPropRef ppVal))
        {
            PhxProp<string> hpName = (PhxProp<string>)ppVal;
            PilotPosition = UnityUtils.FindChildTransform(Owner.transform, hpName);
        }

        if (PilotPosition != null)
        {
            if (section.TryGetValue("Pilot9Pose", out IPhxPropRef ninePoseVal))
            {
                NinePoseAnim = (PhxProp<string>) ninePoseVal;
            }
        }

        if (section.TryGetValue("EyePointOffset", out IPhxPropRef coVal))
        {
            CameraLocation = (PhxProp<Vector3>) coVal;
        }

        if (section.TryGetValue("TrackOffset", out IPhxPropRef cfpVal))
        {
            CameraFocalPoint = (PhxProp<Vector3>) cfpVal;
        }
    }






    void Update()
    {
        if (Occupant == null) return;

        var Controller = Occupant.GetController();

        if (Controller.TryEnterVehicle)
        {
            if (CanExit)
            {
                //OwnerVehicle.EjectOccupant(Occupant);
                Occupant = null;
            }
        }

        if (Controller.ShootPrimary)
        {
            // fire
        }


        if (Controller.ShootSecondary)
        {
            // fire
        }






        if (true)//IsPrimary)
        {
            // Handle vehicle controls
        }




    }
    
}
*/












