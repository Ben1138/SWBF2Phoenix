using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;


public class GameRuntime : MonoBehaviour
{
    public static GameRuntime Instance { get; private set; } = null;
    public RPath GamePath { get; private set; } = @"F:/SteamLibrary/steamapps/common/Star Wars Battlefront II";

    public Loadscreen InitScreenPrefab;
    public Loadscreen LoadScreenPrefab;
    public GameObject MainMenuPrefab;
    public GameObject PauseMenuPrefab;
    public Volume     PostProcessingVolume;
    public AudioMixerGroup UIAudioMixer;

    RPath AddonPath => GamePath / "GameData/addon";
    RPath StdLVLPC;

    Loadscreen CurrentLS;
    GameObject CurrentMenu;

    // ring buffer
    AudioSource[] UIAudio = new AudioSource[5];
    byte UIAudioHead = 0;

    RuntimeEnvironment Env;
    Dictionary<string, string> RegisteredAddons = new Dictionary<string, string>();

    bool bInitMainMenu;
    bool bAvailablePauseMenu;


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
        bAvailablePauseMenu = false;

        bInitMainMenu = bInit;
        ShowLoadscreen(bInit);
        RemoveMenu();

        Env?.ClearScene();
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
        bAvailablePauseMenu = false;

        ShowLoadscreen();
        RemoveMenu();

        RPath envPath = StdLVLPC;
        if (RegisteredAddons.TryGetValue(mapScript, out string addonName))
        {
            envPath = AddonPath / addonName / "data/_lvl_pc";
        }

        Env?.ClearScene();
        Env = RuntimeEnvironment.Create(envPath, StdLVLPC);
        Env.ScheduleLVLRel("load/common.lvl");
        Env.OnLoadscreenLoaded += OnLoadscreenTextureLoaded;
        Env.OnLoaded += OnMapLoaded;
        Env.OnExecuteMain += OnMapExecution;
        Env.Run(mapScript, "ScriptInit", "ScriptPostLoad");
    }

    // Counterpart of ShowMenu()
    public void RemoveMenu()
    {
        if (CurrentMenu != null)
        {
            Destroy(CurrentMenu.gameObject);
            CurrentMenu = null;
        }
    }

    public void PlayUISound(AudioClip sound, float pitch = 1.0f)
    {
        UIAudio[UIAudioHead].clip = sound;
        UIAudio[UIAudioHead].pitch = pitch;
        UIAudio[UIAudioHead].Play();

        UIAudioHead++;
        if (UIAudioHead >= UIAudio.Length)
        {
            UIAudioHead = 0;
        }
    }

    // TODO
    public Color GetTeamColor(byte teamId)
    {
        switch (teamId)
        {
            case 1:
                return Color.green;
            case 2:
                return Color.red;
            case 3:
                return Color.yellow;
        }
        return Color.white;
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

    // Counterpart is RemoveMenu()
    void ShowMenu(GameObject prefab)
    {
        if (CurrentMenu != null)
        {
            RemoveMenu();
        }
        CurrentMenu = Instantiate(prefab);
    }

    void Init()
    {
        Instance = this;

        WorldLoader.UseHDRP = true;
        ClassLoader.Instance.RegisterClassScript("commandpost", typeof(GC_commandpost));

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
            CurrentLS.SetLoadImage(TextureLoader.Instance.ImportUITexture("_LOCALIZE_english_bootlegal"));
        }
        else
        {
            CurrentLS.SetLoadImage(TextureLoader.Instance.ImportUITexture("gal_con"));
        }

        foreach (var lvl in Env.LVLs)
        {
            if (lvl.RelativePath.GetLeaf() == "addme.script")
            {
                var addme = lvl.Level.Get<LibSWBF2.Wrappers.Script>("addme");
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
        // after script execution, but BEFORE map conversion
    }

    void OnMapLoaded(object sender, EventArgs e)
    {
        RemoveLoadscreen();
        bAvailablePauseMenu = true;

        //AudioClip clip = SoundLoader.LoadSound("geo_amb_desert");
        //Debug.Log(clip.name);
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
        Debug.Assert(PauseMenuPrefab  != null);
        Debug.Assert(PostProcessingVolume != null);

        for (int i = 0; i < UIAudio.Length; ++i)
        {
            GameObject audioObj = new GameObject(string.Format("UIAudio{0}", i));
            audioObj.transform.SetParent(transform);
            UIAudio[i] = audioObj.AddComponent<AudioSource>();
            UIAudio[i].outputAudioMixerGroup = UIAudioMixer;
        }

        Init();
    }

    void Update()
    {
        Env?.Update();

        if (bAvailablePauseMenu && Input.GetButtonDown("Cancel"))
        {
            if (CurrentMenu != null)
            {
                RemoveMenu();
            }
            else
            {
                ShowMenu(PauseMenuPrefab);
            }
            PlayUISound(SoundLoader.LoadSound("ui_menuBack"), 1.2f);
        }
    }
}