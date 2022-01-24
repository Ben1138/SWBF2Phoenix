using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;

using LibSWBF2.Wrappers;

// Has only instance properties, so no generic needed
public class PhxVehicleSpawn : PhxInstance, IPhxTickable
{
    PhxMatch Match => PhxGame.GetMatch();
    PhxScene Scene => PhxGame.GetScene();


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


    public static int VehicleCount = 0;
    public GameObject CurrentVehicle = null;

    PhxCommandpost ControlCP;
    float RespawnTimer = 0.0f;


    public override void Init()
    {
        ControlZone.OnValueChanged += OnControlZoneChanged;
        RespawnTimer = 1.0f;
    }

    public override void Destroy()
    {

    }

    void OnControlZoneChanged(string oldVal)
    {
        if (string.IsNullOrEmpty(ControlZone.Get()))
        {
            ControlCP = null;
            return;
        }

        var CommandPosts = Scene.GetCommandPosts();
        foreach (var Post in CommandPosts)
        {
            if (Post.gameObject.name.Equals(ControlZone.Get(), StringComparison.OrdinalIgnoreCase))
            {
                ControlCP = Post;
                break;
            }
        }

        RespawnTimer = 1.0f;
    }

    public enum PhxVehicleSpawnST
    {
        VehicleAwaitingUse,
        VehicleInUse,
        VehicleDecaying,
        VehicleAwaitingSpawn,

        NoVehicle
    }

    PhxVehicleSpawnST SpawnState = PhxVehicleSpawnST.VehicleAwaitingSpawn;

    

    PhxClass GetAppropriateVehicle()
    {
        int teamIdx = ControlCP != null ? ControlCP.Team : Team;
        teamIdx--;

        if (teamIdx < 0 || teamIdx > 1)
        {
            return null;
        }

        PhxMatch.PhxTeam CPTeam = Match.Teams[teamIdx];
        string ClassName;
        switch (CPTeam.Name)
        {
            case "cis":
                ClassName = teamIdx == 0 ? ClassCISATK : ClassCISDEF;
                break;
            case "rep":
                ClassName = teamIdx == 0 ? ClassRepATK : ClassRepDEF;
                break;
            case "imp":
                ClassName = teamIdx == 0 ? ClassImpATK : ClassImpDEF;
                break;
            case "all":
                ClassName = teamIdx == 0 ? ClassAllATK : ClassAllDEF;
                break;
            default:
                return null;
        }

        return Scene.GetClass(ClassName);
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


    public void Tick(float deltaTime)
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
