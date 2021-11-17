using UnityEngine;

public class PhxVehicleTesting : PhxUnityScript
{
    static PhxRuntimeMatch MATCH => PhxGameRuntime.GetMatch();


    private string VehicleClass = "cis_tread_hailfire";
    // "rep_hover_barcspeeder"; 
    // "rep_hover_fightertank"; 
    // "cis_tread_hailfire";
    // "cis_hover_aat"


    public override void ScriptInit()
    {
        PhxLuaAPI.ReadDataFile("ingame.lvl");
        PhxLuaAPI.ReadDataFile("side/rep.lvl");
        PhxLuaAPI.ReadDataFile("side/tur.lvl");
        PhxLuaAPI.ReadDataFile("side/cis.lvl");
    }

    public override void ScriptPostLoad()
    {
        PhxRuntimeScene scene = PhxGameRuntime.GetScene();

		scene.CreateInstance(
            scene.GetClass(VehicleClass),
            "test_vehicle",
            transform.position + 1f * Vector3.up,
            Quaternion.identity
        );        
		
		MATCH.SpawnPlayer(scene.GetClass("rep_inf_ep2_rifleman"), transform.position + new Vector3(-3f,0f,-2f), Quaternion.identity);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
          
    }
}

