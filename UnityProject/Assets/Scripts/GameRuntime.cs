using System;
using System.Collections.Generic;
using UnityEngine;


public class GameRuntime : MonoBehaviour
{
    public Path GamePath { get; private set; } = @"F:/SteamLibrary/steamapps/common/Star Wars Battlefront II";
    Path AddonPath => GamePath / "GameData/addon";
    Path StdLVLPC;

    public static GameRuntime Instance { get; private set; } = null;

    RuntimeEnvironment Env;
    Dictionary<string, string> RegisteredAddons = new Dictionary<string, string>();

    public static LuaRuntime GetLuaRuntime()
    {
        RuntimeEnvironment env = GetEnvironment();
        return env == null ? null : env.GetLuaRuntime();
    }

    public static RuntimeEnvironment GetEnvironment()
    {
        return Instance == null ? null : Instance.Env;
    }

    public void RegisterAddonScript(string scriptName, string addonName)
    {
        RegisteredAddons.Add(scriptName, addonName);
    }

    void Init()
    {
        Instance = this;

        StdLVLPC = GamePath / "GameData/data/_lvl_pc";
        if (GamePath.IsFile() || 
            !GamePath.Exists() || 
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

        EnterMainMenu();
    }

    void ExploreAddons()
    {
        string[] addons = System.IO.Directory.GetDirectories(AddonPath);
        
        foreach (Path addon in addons)
        {
            Path addme = addon / "addme.script";
            if (addme.Exists() && addme.IsFile())
            {
                Env.ScheduleLVLAbs(addme);
            }
        }
    }

    void EnterMainMenu()
    {
        Debug.Assert(Env == null || Env.IsLoaded);

        Env = RuntimeEnvironment.Create(StdLVLPC);

        ExploreAddons();

        Env.OnExecuteMain += OnMainMenuExecution;
        Env.OnLoaded += OnMainMenuLoaded;
        Env.Run("missionlist");
    }

    void EnterMap(string mapScript)
    {
        Debug.Assert(Env == null || Env.IsLoaded);

        Path envPath = StdLVLPC;
        if (RegisteredAddons.TryGetValue(mapScript, out string addonName))
        {
            envPath = AddonPath / addonName / "data/_lvl_pc";
        }

        Env = RuntimeEnvironment.Create(envPath, StdLVLPC);
        Env.OnLoaded += OnMapLoaded;
        Env.Run(mapScript, "ScriptInit", "ScriptPostLoad");
    }

    void OnMainMenuExecution(object sender, EventArgs e)
    {
        foreach (var lvl in Env.LVLs)
        {
            if (lvl.RelativePath.GetLeaf() == "addme.script")
            {
                var addme = lvl.Level.GetScript("addme");
                if (addme == null)
                {
                    Debug.LogWarningFormat("Seems like '{0}' has no 'addme' script chunk!", lvl.RelativePath);
                    continue;
                }
                Env.Execute(addme);
            }
        }
    }

    void OnMainMenuLoaded(object sender, EventArgs e)
    {
        EnterMap("geo1c_con");
    }

    void OnMapLoaded(object sender, EventArgs e)
    {

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
        Env?.Update();
    }
}