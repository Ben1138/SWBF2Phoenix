using UnityEngine;

public class PhxVehicleTesting : PhxUnityScript
{
    static PhxRuntimeMatch MATCH => PhxGameRuntime.GetMatch();


    private string VehicleClass = "rep_hover_fightertank";
//"rep_hover_barcspeeder"; // "rep_hover_fightertank";


    public override void ScriptInit()
    {
        PhxLuaAPI.ReadDataFile("ingame.lvl");
        PhxLuaAPI.ReadDataFile("side/rep.lvl");
        PhxLuaAPI.ReadDataFile("side/cis.lvl");
    }

    public override void ScriptPostLoad()
    {
        Debug.Log("Loaded.");

        PhxRuntimeScene scene = PhxGameRuntime.GetScene();

		scene.CreateInstance(
            scene.GetClass(VehicleClass),
            "tst_tank",
            transform.position + 2f * Vector3.up,
            Quaternion.identity
        );        
		
		MATCH.SpawnPlayer(scene.GetClass("rep_inf_ep2_sniper"), transform.position + new Vector3(-3f,0f,0f), Quaternion.identity);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
          
    }
}

