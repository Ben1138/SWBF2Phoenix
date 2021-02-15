using System;
using UnityEngine;


public class GameRuntime : MonoBehaviour
{
    static Path GamePath = @"F:/SteamLibrary/steamapps/common/Star Wars Battlefront II";
    Path StdLVLPC;

    public static GameRuntime Instance { get; private set; } = null;

    RuntimeEnvironment Env;
    bool bIsRunning;


    public static LuaRuntime GetLuaRuntime()
    {
        return Instance == null ? null : Instance.Env.GetLuaRuntime();
    }

    public static RuntimeEnvironment GetCurrentEnvironment()
    {
        return Instance == null ? null : Instance.Env;
    }

    void Init()
    {
        Instance = this;
        bIsRunning = false;

        StdLVLPC = GamePath / "GameData/data/_lvl_pc";
        if (GamePath.IsFile() || 
            !GamePath.Exists() || 
            StdLVLPC.IsFile() || 
            !CheckExistence("common.lvl") ||
            !CheckExistence("core.lvl") ||
            !CheckExistence("ingame.lvl") ||
            !CheckExistence("inshell.lvl") ||
            !CheckExistence("mission.lvl") ||
            !CheckExistence("shell.lvl"))
        {
            Debug.LogErrorFormat("Invalid game path '{0}!'", GamePath);
            return;
        }

        Env = RuntimeEnvironment.Create(StdLVLPC, null);
        Env?.LoadScheduled();
    }

    bool CheckExistence(string lvlName)
    {
        Path p = StdLVLPC / lvlName;
        bool bExists = p.Exists();
        if (!bExists)
        {
            Debug.LogErrorFormat("Could not find '{0}'!", p);
        }
        return bExists;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (Env != null)
        {
            Env.Update();
            if (!bIsRunning && Env.IsLoaded)
            {
                Run();
            }
            if (Env.IsLoading)
            {
                Debug.Log(Env.GetLoadingProgress());
            }
        }
    }

    void Run()
    {
        Debug.Log("Running Game");
        bIsRunning = true;
        Env.Execute("geo1c_con");
        Env.GetLuaRuntime().CallLua("ScriptInit");
    }
}