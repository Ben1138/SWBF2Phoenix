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


    public PhxProp<string> ControlZone = new PhxProp<string>("");

    PhxCommandpost CommandPost;


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

        if (ControlZone.Get() == null) return;

        var CommandPosts = Scene.GetCommandPosts();
        foreach (var Post in CommandPosts)
        {
            if (Post.gameObject.name.Equals(ControlZone.Get(), StringComparison.OrdinalIgnoreCase))
            {
                CommandPost = Post;
                Team = Post.Team;
                break;
            }
        }
    }

    

    PhxClass GetAppropriateVehicle()
    {
        int index = Team.Get() - 1;
        if (index < 0 || index > 1)
        {
            return null;
        }

        PhxRuntimeMatch.PhxTeam CPTeam = Match.Teams[index];
        string ClassName;
        switch (CPTeam.Name)
        {
            case "cis":
                ClassName = Team == 1 ? ClassCISATK : ClassCISDEF;
                break;
            case "rep":
                ClassName = Team == 1 ? ClassRepATK : ClassRepDEF;
                break;
            case "imp":
                ClassName = Team == 1 ? ClassImpATK : ClassImpDEF;
                break;
            case "all":
                ClassName = Team == 1 ? ClassAllATK : ClassAllDEF;
                break;
            default:
                return null;
                break;
        }

        return Scene.GetClass(ClassName);
    }




    public override void Init()
    {
        RespawnTimer = 1.0f;
    }



    private void SpawnVehicle()
    {
        PhxClass VehicleClass = GetAppropriateVehicle();

        if (VehicleClass != null)
        {
            Scene.CreateInstance(
                        VehicleClass,
                        "vehicle_" + VehicleCount++.ToString(),
                        transform.position,
                        transform.rotation
            );
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

    public override void BindEvents(){}
    public override void Tick(float deltaTime)
    {
        //Profiler.BeginSample("Tick Vehiclespawn");
        //UpdateState(deltaTime);
        //Profiler.EndSample();
    }

    public override void TickPhysics(float deltaTime)
    {
        //Profiler.BeginSample("Tick Hover Physics");
        //UpdatePhysics(deltaTime);
        //Profiler.EndSample();
    }
}
