using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;

using LibSWBF2.Wrappers;

// Has only instance properties, so no generic needed
public class PhxVehicleSpawn : PhxInstance
{
    PhxRuntimeMatch Match => PhxGameRuntime.GetMatch();
    PhxRuntimeScene Scene => PhxGameRuntime.GetScene();


    public PhxProp<float> ExpireTimeEnemy = new PhxProp<float>(1.0f);
    public PhxProp<float> ExpireTimeField = new PhxProp<float>(1.0f);
    public PhxProp<float> DecayTime = new PhxProp<float>(10.0f);
    public PhxProp<float> SpawnTime = new PhxProp<float>(10.0f);

    public PhxProp<string> ClassAllATK = new PhxProp<string>("");
    public PhxProp<string> ClassCISATK = new PhxProp<string>("");
    public PhxProp<string> ClassImpATK = new PhxProp<string>("");
    public PhxProp<string> ClassRepATK = new PhxProp<string>("");
    public PhxProp<string> ClassLocATK = new PhxProp<string>("");
    public PhxProp<string> ClassHisATK = new PhxProp<string>("");
    public PhxProp<string> ClassAllDEF = new PhxProp<string>("");
    public PhxProp<string> ClassCISDEF = new PhxProp<string>("");
    public PhxProp<string> ClassImpDEF = new PhxProp<string>("");
    public PhxProp<string> ClassRepDEF = new PhxProp<string>("");
    public PhxProp<string> ClassLocDEF = new PhxProp<string>("");
    public PhxProp<string> ClassHisDEF = new PhxProp<string>("");
    public PhxProp<string> ClassLocals = new PhxProp<string>("");


    public PhxProp<PhxRegion> ControlZone = new PhxProp<PhxRegion>(null);


    public override void InitInstance(ISWBFProperties instOrClass, PhxClass classProperties)
    {
        Type type = GetType();
        MemberInfo[] members = type.GetMembers();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(IPhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                IPhxPropRef refValue = (IPhxPropRef)type.GetField(member.Name).GetValue(this);
                P.Register(member.Name, refValue);
            }
        }

        // make sure the instance is listening on property change events
        // before assigning the actual instance property value
        // BindEvents();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(IPhxPropRef).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                IPhxPropRef refValue = (IPhxPropRef)type.GetField(member.Name).GetValue(this);
                PhxPropertyDB.AssignProp(instOrClass, member.Name, refValue);
            }
        }
    }



    public static int VehicleCount = 0;


    public GameObject CurrentVehicle = null;


    private float DecayTimer;
    private float RespawnTimer = 0.0f;


    public enum PhxVehicleSpawnST
    {
        VehicleAwaitingUse,
        VehicleInUse,
        VehicleDecaying,
        VehicleAwaitingSpawn,

        NoVehicle
    }

    private PhxVehicleSpawnST SpawnState = PhxVehicleSpawnST.VehicleAwaitingSpawn;


    void Start()
    {
        Init();
    }

    


    public void Init()
    {
        RespawnTimer = 1.0f;

        //DecayTimer = 
    }



    private void SpawnVehicle()
    {
        string ClassName = ClassRepATK != "" ? ClassRepATK : ClassRepDEF;

        if (ClassName == "") return;

        PhxClass VehicleClass = Scene.GetClass(ClassName);

        if (VehicleClass != null)
        {
            // Will update to account for team allegiance etc...
            Scene.CreateInstance(
                        VehicleClass,
                        "vehicle_" + VehicleCount++.ToString(),
                        transform.position + new Vector3(0.0f, 1.0f, 0.0f),
                        transform.rotation
            );
        }  
        else 
        {
            Debug.LogError("Scene couldn't find PhxClass for vehicle type: " + ClassName);
        }  
    }



    void ApplyTeam(int oldTeam)
    {
        //if (Team == oldTeam)
        //{
        //    // nothing to do
        //    return;
        //}
    }


    void Update()
    {
        // ALL STATE CHANGE HANDLED HERE   
        if (SpawnState == PhxVehicleSpawnST.VehicleAwaitingSpawn)
        {
            if (RespawnTimer <= 0.0f)
            {
                SpawnVehicle();
                RespawnTimer = SpawnTime;

                SpawnState = PhxVehicleSpawnST.VehicleAwaitingUse;
            }
            else 
            {
                RespawnTimer -= Time.deltaTime;
            }
        }
        else if (SpawnState == PhxVehicleSpawnST.VehicleDecaying)
        {
            if (CurrentVehicle != null)
            {
                //decay health
            }
        }
        else 
        {
            // 
        }
    }
}
