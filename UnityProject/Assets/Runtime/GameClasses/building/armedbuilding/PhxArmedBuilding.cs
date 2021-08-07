
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;



/*
No pooling needed for these
*/

public class PhxArmedBuilding : PhxVehicle
{
    public class ClassProperties : PhxVehicleProperties{}



    PhxVehicleTurret TurretSection;

    public override void Init()
    {
        base.Init();
        SetupEnterTrigger();

        ModelMapping.ConvexifyMeshColliders(false);
        
        var EC = C.EntityClass;
        EC.GetAllProperties(out uint[] properties, out string[] values);

        Seats = new List<PhxSeat>();

        int i = 0;
        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("BUILDINGSECTION") && 
                values[i].Equals("TURRET1", StringComparison.OrdinalIgnoreCase))
            {
                TurretSection = new PhxVehicleTurret(this, 0);
                TurretSection.InitManual(EC, i, "BUILDINGSECTION", "TURRET1");
                Seats.Add(TurretSection);
            }

            i++;
        }

        SetIgnoredCollidersOnAllWeapons();
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
        base.Tick(deltaTime);
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
