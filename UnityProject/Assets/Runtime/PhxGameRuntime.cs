using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;


public class PhxGameRuntime : MonoBehaviour
{
    public const int PlayerMask = 3;

    public static PhxGameRuntime Instance { get; private set; } = null;
    public PhxPath GamePath { get; private set; } = @"F:\SteamLibrary\steamapps\common\Star Wars Battlefront II";

    [Header("Settings")]
    public string Language = "english";

    [Header("References")]
    public PhxLoadscreen    InitScreenPrefab;
    public PhxLoadscreen    LoadScreenPrefab;
    public PhxMenuInterface MainMenuPrefab;
    public PhxMenuInterface PauseMenuPrefab;
    public PhxMenuInterface CharacterSelectPrefab;
    public Transform        CharSelectTransform;
    public Volume           CharSelectPPVolume;
    public Volume           PostProcessingVolume;
    public AudioMixerGroup  UIAudioMixer;
    public PhxCamera        Camera;
    public LineRenderer     AimDebug;
    public PhysicMaterial   GroundPhyMat;

    // This will only fire for maps, NOT for the main menu!
    public Action OnMatchStart;
    public Action OnRemoveMenu;

    PhxPath AddonPath => GamePath / "GameData/addon";
    PhxPath StdLVLPC;

    PhxLoadscreen    CurrentLS;
    PhxMenuInterface CurrentMenu;

    // ring buffer
    AudioSource[] UIAudio = new AudioSource[5];
    byte UIAudioHead = 0;

    PhxRuntimeEnvironment Env;
    Dictionary<string, string> RegisteredAddons = new Dictionary<string, string>();

    bool bInitMainMenu;

    // contains mapluafile strings, e.g. cor1c_con
    List<string> MapRotation = new List<string>();
    int MapRotationIdx = -1;


    public static PhxLuaRuntime GetLuaRuntime()
    {
        PhxRuntimeEnvironment env = GetEnvironment();
        return env == null ? null : env.GetLuaRuntime();
    }

    public static PhxRuntimeEnvironment GetEnvironment()
    {
        return Instance == null ? null : Instance.Env;
    }

    public static PhxCamera GetCamera()
    {
        return Instance == null ? null : Instance.Camera;
    }

    public static PhxRuntimeScene GetScene()
    {
        PhxRuntimeEnvironment env = GetEnvironment();
        return env == null ? null : env.GetScene();
    }

    public static PhxGameMatch GetMatch()
    {
        PhxRuntimeEnvironment env = GetEnvironment();
        return env == null ? null : env.GetMatch();
    }

    public static PhxTimerDB GetTimerDB()
    {
        PhxRuntimeEnvironment env = GetEnvironment();
        return env == null ? null : env.GetTimerDB();
    }

    public void AddToMapRotation(List<string> mapScripts)
    {
        MapRotation.AddRange(mapScripts);
    }

    public void AddToMapRotation(string mapScript)
    {
        MapRotation.Add(mapScript);
    }

    public void NextMap()
    {
        if (MapRotation.Count == 0)
        {
            return;
        }

        if (++MapRotationIdx >= MapRotation.Count)
        {
            MapRotationIdx = 0;
        }

        EnterMap(MapRotation[MapRotationIdx]);
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

        MapRotation.Clear();
        MapRotationIdx = -1;

        bInitMainMenu = bInit;
        ShowLoadscreen(bInit);
        RemoveMenu(false);

        Env?.ClearScene();
        Env = PhxRuntimeEnvironment.Create(StdLVLPC);

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
        RemoveMenu(false);

        PhxPath envPath = StdLVLPC;
        if (RegisteredAddons.TryGetValue(mapScript, out string addonName))
        {
            envPath = AddonPath / addonName / "data/_lvl_pc";
        }

        Env?.ClearScene();
        Env = PhxRuntimeEnvironment.Create(envPath, StdLVLPC);
        Env.ScheduleLVLRel("load/common.lvl");
        Env.OnLoadscreenLoaded += OnLoadscreenTextureLoaded;
        Env.OnLoaded += OnMapLoaded;
        Env.OnExecuteMain += OnMapExecution;
        Env.Run(mapScript, "ScriptInit", "ScriptPostLoad");
    }

    public PhxMenuInterface ShowMenu(PhxMenuInterface prefab)
    {
        if (CurrentMenu != null)
        {
            RemoveMenu(false);
        }
        CurrentMenu = Instantiate(prefab.gameObject).GetComponent<PhxMenuInterface>();
        return CurrentMenu;
    }

    public void RemoveMenu()
    {
        RemoveMenu(true);
    }

    void RemoveMenu(bool bInvokeEvent)
    {
        if (CurrentMenu != null)
        {
            CurrentMenu.Clear();
            Destroy(CurrentMenu.gameObject);
            CurrentMenu = null;
        }
        if (bInvokeEvent)
        {
            OnRemoveMenu?.Invoke();
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
    public Color GetTeamColor(int teamId)
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

    void OnLoadscreenTextureLoaded(Texture2D loadscreenTexture)
    {
        CurrentLS.SetLoadImage(loadscreenTexture);
    }

    void RemoveLoadscreen()
    {
        Debug.Assert(CurrentLS != null);
        CurrentLS.FadeOut();
        CurrentLS = null;
    }

    void Init()
    {
        Instance = this;
        WorldLoader.UseHDRP = true;

        StdLVLPC = GamePath / "GameData/data/_lvl_pc";
        if (GamePath.IsFile()              || 
            !GamePath.Exists()             || 
            !CheckExistence("common.lvl")  ||
            !CheckExistence("core.lvl")    ||
            !CheckExistence("ingame.lvl")  ||
            !CheckExistence("inshell.lvl") ||
            !CheckExistence("mission.lvl") ||
            !CheckExistence("shell.lvl"))
        {
            Debug.LogErrorFormat("Invalid game path '{0}!'", GamePath);
            return;
        }

        //EnterMainMenu(true);
        EnterMap("geo1c_con");
    }

    void ExploreAddons()
    {
        string[] addons = System.IO.Directory.GetDirectories(AddonPath);
        
        foreach (PhxPath addon in addons)
        {
            PhxPath addme = addon / "addme.script";
            if (addme.Exists() && addme.IsFile())
            {
                Env.ScheduleLVLAbs(addme);
            }
        }
    }

    void OnMainMenuExecution()
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

    void OnMainMenuLoaded()
    {
        RemoveLoadscreen();
        //EnterMap("geo1c_con");
        ShowMenu(MainMenuPrefab);
    }

    void OnMapExecution()
    {
        // after script execution, but BEFORE map conversion
    }

    void OnMapLoaded()
    {
        RemoveLoadscreen();
        OnMatchStart?.Invoke();
    }

    bool CheckExistence(string lvlName)
    {
        PhxPath p = StdLVLPC / lvlName;
        bool bExists = p.Exists();
        if (!bExists)
        {
            Debug.LogErrorFormat("Could not find '{0}'!", p);
        }
        return bExists;
    }

    void Start()
    {
        Debug.Assert(InitScreenPrefab     != null);
        Debug.Assert(LoadScreenPrefab     != null);
        Debug.Assert(MainMenuPrefab       != null);
        Debug.Assert(PauseMenuPrefab      != null);
        Debug.Assert(CharSelectTransform  != null);
        Debug.Assert(CharSelectPPVolume   != null);
        Debug.Assert(PostProcessingVolume != null);
        Debug.Assert(UIAudioMixer         != null);
        Debug.Assert(Camera               != null);
        Debug.Assert(GroundPhyMat         != null);

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
    }

    void FixedUpdate()
    {
        Env?.FixedUpdate(Time.fixedDeltaTime);
    }
}