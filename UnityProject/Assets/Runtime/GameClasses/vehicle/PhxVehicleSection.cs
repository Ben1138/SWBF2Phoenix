using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;

/*
All VehicleSections have camera properties, movement ranges, and pilot configurations.
Many have Weapons and related Aimers.
They can be followed by cameras, entered, exited, hopped to/from by different pilots,
and each maintains a ref to its parent vehicle.

As of now, the Lua inaccessible properties are read directly from the EntityClass properties and values,
which are passed by the parent vehicle to the constructor.  The current reflection based assignment system
doesn't map well to implied groups eg Aimers, Springs. Lua accessible properties will use the 
reflection system and but will be limited to the parent vehicle.

Each frame the parent vehicle will call Update() on each section. 
*/

public abstract class PhxVehicleSection : IPhxTrackable
{    
    public PhxHover OwnerVehicle;

    public PhxSoldier Occupant;

    public bool CanExit = true;


    public Transform PilotPosition;
    public string PilotAnimation = "";


    public Vector3 EyePointOffset;
    public Vector3 TrackCenter;
    public Vector3 TrackOffset;
    public float TiltValue;

    public Vector2 PitchLimits;
    public Vector2 YawLimits;


    protected int Index;


    protected List<PhxAimer> Aimers;

    // View direction in local space 
    protected Vector3 ViewDirection;

    // Accumulators for view control
    protected float PitchAccum;
    protected float YawAccum;


    public abstract Vector3 GetCameraPosition();
    public abstract Quaternion GetCameraRotation();


    public bool SetOccupant(PhxSoldier s) 
    {
        if (Occupant != null) return false;
        
        Occupant = s;
        RequestSwitch = false;

        s.SetPilot(PilotPosition,"");

        return true;
    }




    public abstract void Update();


    public bool HopToNextSeat()
    {
        if (OwnerVehicle == null) {return false;}

        bool r = OwnerVehicle.TrySwitchSeat(Index);
        if (r)
        {
            Occupant.
            Occupant = null;
        }
        return r;
    }


    public void ClearOccupant()
    {
        Occupant = null;
        RequestSwitch = false;
    }



    protected bool AddAimer(PhxAimer CurrAimer)
    {
        if (CurrAimer.Node == null){ return false; }
        CurrAimer.Init();

        //Debug.LogFormat("Attempting to add Aimer: {0}", CurrAimer.Node.name);

        if (Aimers == null)
        {
            Aimers = new List<PhxAimer>();
        }

        if (Aimers.Count > 0 && Aimers[Aimers.Count - 1].HierarchyLevel > CurrAimer.HierarchyLevel)
        {
            Aimers[Aimers.Count - 1].ChildAimer = CurrAimer;
        }
        else 
        {
            Aimers.Add(CurrAimer);
        }   

        return true;     
    }


    protected int AssignCameraValues(uint[] properties, string[] values, uint )
    {
        int start = i;
        while (i < )
    }




    /*
    void UpdateInput()
    {

        PitchAccum += Controller.mouseY;
        PitchAccum = Mathf.Clamp(PitchAccum, -8f, 25f);

        YawAccum += Controller.mouseX;
        YawAccum = Mathf.Clamp(YawAccum, 0f, 0f);   
    }
    */
}
