
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxArmedBuilding : PhxVehicle<PhxArmedBuilding.ClassProperties>, IPhxTrackable
{
    static PhxGameRuntime GAME => PhxGameRuntime.Instance;
    static PhxRuntimeMatch MTC => PhxGameRuntime.GetMatch();
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();
    static PhxCamera CAM => PhxGameRuntime.GetCamera();


    public class ClassProperties : PhxVehicleProperties{}


    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


    PhxPawnController Controller = null;


    PhxInstance Aim;


    public bool TrySwitchSeat(int index)
    {
        return false;
    }



    public bool TryEnterVehicle(PhxSoldier soldier, 
                                out string NinePoseAnim, 
                                out Transform PilotPosition)
    {
        NinePoseAnim = "";
        PilotPosition = null;

        // Find first available seat

        if (TurretSection.IsOccupied())
        {
            return false;
        }
        else 
        {
            TurretSection.SetOccupant(soldier);
            PhxGameRuntime.GetCamera().FollowTrackable(TurretSection);
            return true;
        }
    }


    public bool Eject(int i)
    {
        if (i >= Sections.Count)
        {
            return false;
        }

        if (Sections[i] != null || Sections[i].Occupant != null)
        {
            Sections[i].Occupant.SetFree(transform.position + Vector3.up * 2.0f);
            PhxGameRuntime.GetCamera().Follow(Sections[i].Occupant);
            Sections[i].Occupant = null;

            return true;
        }
        else 
        {
            return false;
        }
    }


    PhxVehicleTurret TurretSection;
    List<PhxVehicleSection> Sections;

    public override void Init()
    {
        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        Sections = new List<PhxVehicleSection>();

        int i = 0;
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("BUILDINGSECTION") && values[i] == "TURRET1")
            {
                TurretSection = new PhxVehicleTurret(properties, values, ref i, transform, 0);
                Sections.Add(TurretSection);
            }

            i++;
        }
    }


    void UpdateState(float deltaTime)
    {
        TurretSection.Update();
    }

    void UpdatePhysics(float deltaTime){}


    public Vector3 GetCameraPosition()
    {
        return TurretSection.GetCameraPosition();
    }

    public Quaternion GetCameraRotation()
    {
        return TurretSection.GetCameraRotation();
    }


    public override bool IncrementSlice(out float progress)
    {
        progress = SliceProgress;
        return false;
    }


    public override void Tick(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick ArmedBuilding");
        UpdateState(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public override void TickPhysics(float deltaTime)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Tick ArmedBuilding Physics");
        UpdatePhysics(deltaTime);
        UnityEngine.Profiling.Profiler.EndSample();
    }



    public override void BindEvents(){}
    public override void Fixate(){}
    public override IPhxWeapon GetPrimaryWeapon(){ return null; }
    public void AddAmmo(float amount){}
    public override void PlayIntroAnim(){}
    public PhxInstance GetAim(){ return Aim; }
    void StateFinished(int layer){}
    public void AddHealth(float amount){}
}
