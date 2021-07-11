
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxArmedBuilding : PhxVehicle
{
    public class ClassProperties : PhxVehicleProperties{}

    public PhxProp<float> CurHealth = new PhxProp<float>(100.0f);


    PhxVehicleTurret TurretSection;

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
                TurretSection = new PhxVehicleTurret(properties, values, ref i, this, 0);
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
