using UnityEngine;

public class PhxVehicleTesting : PhxUnityScript
{
    static PhxMatch MATCH => PhxGame.GetMatch();


    public string VehicleClass = "rep_fly_arc170fighter_sc";
    // "rep_hover_barcspeeder"; 
    // "rep_hover_fightertank"; 
    // "cis_tread_hailfire";
    // "cis_hover_aat"


    public override void ScriptInit()
    {
        PhxLuaAPI.ReadDataFile("ingame.lvl");
        PhxLuaAPI.ReadDataFile("SIDE/rep.lvl");
        PhxLuaAPI.ReadDataFile("side/tur.lvl");
        PhxLuaAPI.ReadDataFile("side/cis.lvl");
        PhxLuaAPI.ReadDataFile("side/imp.lvl");
        PhxLuaAPI.ReadDataFile("side/vehicles.lvl");
    }

    public override void ScriptPostLoad()
    {
        PhxScene scene = PhxGame.GetScene();

		scene.CreateInstance(
            scene.GetClass(VehicleClass),
            "test_vehicle",
            transform.position,
            Quaternion.identity
        );        
		
		MATCH.SpawnPlayer(scene.GetClass("rep_inf_ep3_rifleman"), transform.position + new Vector3(-3f,0f,-2f), Quaternion.identity);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
          
    }
}

