using System;
using System.Collections.Generic;
using UnityEngine;


public class GameRuntime : MonoBehaviour
{
    public static GameRuntime Instance { get; private set; } = null;
    public RPath GamePath { get; private set; } = @"F:/SteamLibrary/steamapps/common/Star Wars Battlefront II";

    public Loadscreen InitScreenPrefab;
    public Loadscreen LoadScreenPrefab;
    public GameObject MainMenuPrefab;

    RPath AddonPath => GamePath / "GameData/addon";
    RPath StdLVLPC;

    Loadscreen CurrentLS;
    GameObject CurrentMenu;

    RuntimeEnvironment Env;
    Dictionary<string, string> RegisteredAddons = new Dictionary<string, string>();

    bool bInitMainMenu;


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
        if (RegisteredAddons.TryGetValue(scriptName, out string addNm))
        {
            Debug.LogWarningFormat("Addon script '{0}' already registered to '{1}'!", scriptName, addNm);
            return;
        }
        RegisteredAddons.Add(scriptName, addonName);
    }

    public void EnterMainMenu(bool bInit = false)
    {
        Debug.Assert(Env == null || Env.IsLoaded);

        bInitMainMenu = bInit;
        ShowLoadscreen(bInit);
        RemoveMenu();

        Env = RuntimeEnvironment.Create(StdLVLPC);

        if (!bInit)
        {
            Env.ScheduleLVLRel("load/gal_con.lvl");
        }

        RegisteredAddons.Clear();
        ExploreAddons();

        Env.OnExecuteMain += OnMainMenuExecution;
        Env.OnLoaded += OnMainMenuLoaded;
        Env.Run("missionlist");
    }

    public void EnterMap(string mapScript)
    {
        Debug.Assert(Env == null || Env.IsLoaded);

        ShowLoadscreen();
        RemoveMenu();

        RPath envPath = StdLVLPC;
        if (RegisteredAddons.TryGetValue(mapScript, out string addonName))
        {
            envPath = AddonPath / addonName / "data/_lvl_pc";
        }

        Env = RuntimeEnvironment.Create(envPath, StdLVLPC);
        Env.ScheduleLVLRel("load/common.lvl");
        Env.OnLoadscreenLoaded += OnLoadscreenTextureLoaded;
        Env.OnLoaded += OnMapLoaded;
        Env.OnExecuteMain += OnMapExecution;
        Env.Run(mapScript, "ScriptInit", "ScriptPostLoad");
    }

    void ShowLoadscreen(bool bInitScreen = false)
    {
        Debug.Assert(CurrentLS == null);
        CurrentLS = Instantiate(bInitScreen ? InitScreenPrefab : LoadScreenPrefab);
    }

    void OnLoadscreenTextureLoaded(object sender, LoadscreenLoadedEventsArgs args)
    {
        CurrentLS.SetLoadImage(args.LoadscreenTexture);
    }

    void RemoveLoadscreen()
    {
        Debug.Assert(CurrentLS != null);
        CurrentLS.FadeOut();
        CurrentLS = null;
    }

    void ShowMenu(GameObject prefab)
    {
        if (CurrentMenu != null)
        {
            RemoveMenu();
        }
        CurrentMenu = Instantiate(prefab);
    }

    void RemoveMenu()
    {
        if (CurrentMenu != null)
        {
            Destroy(CurrentMenu.gameObject);
            CurrentMenu = null;
        }
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

        EnterMainMenu(true);
    }

    void ExploreAddons()
    {
        string[] addons = System.IO.Directory.GetDirectories(AddonPath);
        
        foreach (RPath addon in addons)
        {
            RPath addme = addon / "addme.script";
            if (addme.Exists() && addme.IsFile())
            {
                Env.ScheduleLVLAbs(addme);
            }
        }
    }

    void OnMainMenuExecution(object sender, EventArgs e)
    {
        Debug.Assert(CurrentLS != null);

        if (bInitMainMenu)
        {
            CurrentLS.SetLoadImage(TextureLoader.Instance.ImportTexture("_LOCALIZE_english_bootlegal"));
        }
        else
        {
            CurrentLS.SetLoadImage(TextureLoader.Instance.ImportTexture("gal_con"));
        }

        foreach (var lvl in Env.LVLs)
        {
            if (lvl.RelativePath.GetLeaf() == "addme.script")
            {
                var addme = lvl.Level.GetWrapper<LibSWBF2.Wrappers.Script>("addme");
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
        RemoveLoadscreen();
        //EnterMap("geo1c_con");
        ShowMenu(MainMenuPrefab);
    }

    void OnMapExecution(object sender, EventArgs e)
    {
        
    }

    void OnMapLoaded(object sender, EventArgs e)
    {
        RemoveLoadscreen();
    }

    bool CheckExistence(string lvlName)
    {
        RPath p = StdLVLPC / lvlName;
        bool bExists = p.Exists();
        if (!bExists)
        {
            Debug.LogErrorFormat("Could not find '{0}'!", p);
        }
        return bExists;
    }

    void Start()
    {
        Debug.Assert(InitScreenPrefab != null);
        Debug.Assert(LoadScreenPrefab != null);
        Debug.Assert(MainMenuPrefab   != null);

        Init();
    }

    void Update()
    {
        Env?.Update();
    }
}