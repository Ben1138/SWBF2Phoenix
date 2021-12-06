
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxArmedBuilding : PhxVehicle
{
    public class ClassProperties : PhxVehicleProperties{}



    PhxVehicleTurret TurretSection;

    public override void Init()
    {
        base.Init();
        SetupEnterTrigger();
        
        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        Sections = new List<PhxVehicleSection>();

        int i = 0;
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("BUILDINGSECTION") && 
                values[i].Equals("TURRET1", StringComparison.OrdinalIgnoreCase))
            {
                TurretSection = new PhxVehicleTurret(this, 0);
                TurretSection.InitManual(EC, i, "BUILDINGSECTION", "TURRET1");
                Sections.Add(TurretSection);
            }

            i++;
        }
    }

    public override void Destroy()
    {
        
    }

    void UpdateState(float deltaTime)
    {
        if (TurretSection != null)
        {
            TurretSection.Update();        
        }
    }

    void UpdatePhysics(float deltaTime){}


    public override Vector3 GetCameraPosition()
    {
        return TurretSection.GetCameraPosition();
    }

    public override Quaternion GetCameraRotation()
    {
        return TurretSection.GetCameraRotation();
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
}
