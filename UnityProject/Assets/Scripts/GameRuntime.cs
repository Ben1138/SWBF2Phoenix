using UnityEngine;

public class GameRuntime : MonoBehaviour
{
    public static GameRuntime Instance { get; private set; } = null;

    LuaRuntime Lua;


    public static LuaRuntime GetLuaRuntime()
    {
        return Instance == null ? null : Instance.Lua;
    }

    void Start()
    {
        Lua = new LuaRuntime();
        Instance = this;
        //Lua.ExecuteFile(Application.dataPath + "/geo1c_con.script");
    }

    void Update()
    {
        
    }
}
