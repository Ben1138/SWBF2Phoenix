using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2;
using LibSWBF2.Enums;
using LibSWBF2.Wrappers;


/*
 * Phases of PhxRuntimeEnvironment:
 * 
 * 1. Init                          Environment is created, with base LVLs scheduled and ready to load. Base LVLs are:
 *                                      - core.lvl
 *                                      - shell.lvl
 *                                      - common.lvl
 *                                      - mission.lvl
 * 2. Loading Base                  Loading scheduled base LVLs
 * 3. Executing Main                There's always a LUA script responsible for environment setup that will be executed in this phase.
 *                                  Usually, there's an optional main function within the main script that will called aswell, if specified.
 *                                  The main LUA script is expected to call ReadDataFile on multiple LVL files. These LVL files will be scheduled.
 * 4. Loading World                 All LVLs that have been scheduled during phase 3 will be loaded in this phase
 * 5. Create Scene                  In this phase, scene conversion of the imported world LVL takes place. 
 * 6. Loaded                        Final state. Everything is done.
 * 
 */


public class PhxRuntimeEnvironment
{
    public struct LVL
    {
        public SWBF2Handle Handle;
        public Level Level;
        public bool bIsSoundBank;

        // For Debug
        public PhxPath DisplayPath;
        public bool bIsAddon;
    }

    public enum EnvStage
    {
        Init,        
        LoadingBase, 
        ExecuteMain, 
        LoadingWorld,
        CreateScene, 
        Loaded       
    }


    public List<LVL> Loading = new List<LVL>();
    public List<LVL> Loaded  = new List<LVL>();

    public bool              IsLoaded => Stage == EnvStage.Loaded;
    public Action<Texture2D> OnLoadscreenLoaded;
    public Action            OnExecuteMain;
    public Action            OnLoaded;

    public PhxPath    Path          { get; private set; }
    public PhxPath    FallbackPath  { get; private set; }
    public EnvStage Stage         { get; private set; }

    bool CanSchedule => Stage == EnvStage.Init || Stage == EnvStage.ExecuteMain;
    bool CanExecute  => Stage == EnvStage.ExecuteMain || Stage == EnvStage.CreateScene || Stage == EnvStage.Loaded;

    PhxLuaRuntime  LuaRT;
    SWBF2Handle LoadscreenHandle;

    Container EnvCon;
    Level     WorldLevel;     // points to level inside 'Loaded'
    Level     LoadscreenLVL;  // points to level inside 'Loaded'

    string InitScriptName;
    string InitFunctionName;
    string PostLoadFunctionName;

    PhxRuntimeScene RTScene;
    PhxRuntimeMatch Match;
    PhxTimerDB Timers;

    List<Localization> Localizations = new List<Localization>();
    Dictionary<string, List<Localization>> LocalizationLookup = new Dictionary<string, List<Localization>>();

    // To prevent loading an lvl more than once.
    // The path here always describes the relative 2-leaf lvl path
    Dictionary<PhxPath, SWBF2Handle> PathToHandle = new Dictionary<PhxPath, SWBF2Handle>();


    PhxRuntimeEnvironment(PhxPath path, PhxPath fallbackPath)
    {
        Path = path;
        FallbackPath = fallbackPath;
        Stage = EnvStage.Init;
        WorldLevel = null;

        LuaRT = new PhxLuaRuntime();
        EnvCon = new Container();

        Loader.SetGlobalContainer(EnvCon);
    }

    ~PhxRuntimeEnvironment()
    {
        Destroy();
    }

    public void Destroy()
    {
        WorldLevel = null;
        Match.Destroy();
        Match = null;
        Timers = null;
        RTScene.Destroy();
        RTScene = null;
        OnLoadscreenLoaded = null;
        OnExecuteMain = null;
        OnLoaded = null;
        LuaRT?.Close();
        EnvCon?.Delete();
        EnvCon = null;
        PathToHandle.Clear();
        Debug.Log("PhxRuntimeEnvironment destroyed");
    }

    public PhxRuntimeScene GetScene()
    {
        return RTScene;
    }

    public PhxRuntimeMatch GetMatch()
    {
        return Match;
    }

    public PhxTimerDB GetTimerDB()
    {
        return Timers;
    }

    public static PhxRuntimeEnvironment Create(PhxPath envPath, PhxPath fallbackPath = null, bool initMatch=true)
    {
        if (!envPath.Exists())
        {
            Debug.LogErrorFormat("Given environment path '{0}' doesn't exist!", envPath);
            return null;
        }

        if (fallbackPath == null)
        {
            fallbackPath = envPath;
        }
        Debug.Assert(fallbackPath.Exists());

        PhxAnimationLoader.ClearDB();
        PhxLuaEvents.Clear();

        PhxRuntimeEnvironment rt = new PhxRuntimeEnvironment(envPath, fallbackPath);
        rt.ScheduleRel("core.lvl");
        rt.ScheduleRel("shell.lvl");
        rt.ScheduleRel("common.lvl");
        rt.ScheduleRel("mission.lvl");
        rt.ScheduleRel("sound/common.bnk");

        // TODO: Remove
        rt.ScheduleAbs(rt.FallbackPath / "ingame.lvl");

        rt.RTScene = new PhxRuntimeScene(rt, rt.EnvCon);
        rt.Match = initMatch ? new PhxRuntimeMatch() : null;
        rt.Timers = new PhxTimerDB();

        PhxAnimationLoader.Con = rt.EnvCon;

        return rt;
    }

    public PhxLuaRuntime GetLuaRuntime()
    {
        return LuaRT;
    }

    public T Find<T>(string name) where T : NativeWrapper, new()
    {
        return EnvCon.Get<T>(name);
    }

    public bool Execute(string scriptName)
    {
        Debug.Assert(CanExecute);
        Script script = EnvCon.Get<Script>(scriptName);
        if (script == null || !script.IsValid())
        {
            Debug.LogErrorFormat("Couldn't find script '{0}'!", scriptName);
            return false;
        }
        return Execute(script);
    }

    public bool Execute(Script script)
    {
        Debug.Assert(CanExecute);
        if (script == null || !script.IsValid())
        {
            Debug.LogErrorFormat("Given script '{0}' is NULl or invalid!", script.Name);
            return false;
        }

        if (!script.GetData(out IntPtr luaBin, out uint size))
        {
            Debug.LogErrorFormat("Couldn't grab lua binary code from script '{0}'!", script.Name);
            return false;
        }

        return LuaRT.Execute(luaBin, size, script.Name);
    }

    public void Run(string initScript, string initFn = null, string postLoadFn = null)
    {
        Debug.Assert(Stage == EnvStage.Init);
        Loader.ResetAllLoaders();

        InitScriptName = initScript;
        InitFunctionName = initFn;
        PostLoadFunctionName = postLoadFn;

        LoadscreenHandle = ScheduleRel(GetLoadscreenPath());
        EnvCon.LoadLevels();

        Stage = EnvStage.LoadingBase;
    }

    public float GetLoadingProgress()
    {
        float stageContribution = 1.0f / 2.0f;
        float stageProgress = Stage >= EnvStage.LoadingWorld ? stageContribution : 0.0f;

        return stageProgress + stageContribution * EnvCon.GetOverallProgress();
    }

    public Level GetWorldLevel()
    {
        return WorldLevel;
    }

    public SWBF2Handle ScheduleAbs(PhxPath absPath, string[] subLVLs = null)
    {
        if (!Schedule(absPath, out SWBF2Handle handle, subLVLs))
        {
            Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", absPath);
        }
        return handle;
    }

    public SWBF2Handle ScheduleRel(PhxPath relPath, string[] subLVLs = null, bool bNoFallback = false)
    {
        SWBF2Handle handle;
        if (Schedule(Path / relPath, out handle, subLVLs) || bNoFallback)
        {
            if (!handle.IsValid())
            {
                Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relPath);
            }
            return handle;
        }
        else if (!Schedule(FallbackPath / relPath, out handle, subLVLs))
        {
            Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relPath);
        }
        return handle;
    }

    public float GetProgress(SWBF2Handle handle)
    {
        return EnvCon.GetProgress(handle);
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < Loading.Count; ++i)
        {
            ELoadStatus status = EnvCon.GetStatus(Loading[i].Handle);
            if (status == ELoadStatus.Loaded)
            {
                LVL scheduled = Loading[i];
                if (!scheduled.bIsSoundBank)
                {
                    var lvl = EnvCon.GetLevel(Loading[i].Handle);
                    Debug.Assert(lvl != null);
                    scheduled.Level = lvl;

                    if (lvl.IsWorldLevel)
                    {
                        if (WorldLevel != null)
                        {
                            Debug.LogErrorFormat("Encounterred another world lvl '{0}' in environment! Previously found world lvl: '{1}'", lvl.Name, WorldLevel.Name);
                        }
                        else
                        {
                            WorldLevel = lvl;
                        }
                    }

                    // grab lvl localizations, if any
                    Localizations.AddRange(lvl.Get<Localization>());
                }

                Loaded.Add(scheduled);
                Loading.RemoveAt(i);
                break; // do not further iterate altered list
            }
            else if (status == ELoadStatus.Failed)
            {
                Debug.LogErrorFormat("Loading '{0}' failed!", Loading[i].DisplayPath);

                Loading.RemoveAt(i);
                break; // do not further iterate altered list
            }    
        }

        if (Stage == EnvStage.LoadingBase)
        {
            if (LoadscreenLVL == null)
            {
                LoadscreenLVL = EnvCon.GetLevel(LoadscreenHandle);
                if (LoadscreenLVL != null)
                {
                    var textures = LoadscreenLVL.Get<LibSWBF2.Wrappers.Texture>();
                    int texIdx = UnityEngine.Random.Range(0, textures.Length - 1);
                    OnLoadscreenLoaded?.Invoke(TextureLoader.Instance.ImportUITexture(textures[texIdx].Name));
                }
            }

            if (EnvCon.IsDone() && Loading.Count == 0)
            {
                Stage = EnvStage.ExecuteMain;
                RunMain();
            }
        }

        if (Stage == EnvStage.LoadingWorld && EnvCon.IsDone() && Loading.Count == 0)
        {
            for (int i = 0; i < Localizations.Count; ++i)
            {
                string locName = Localizations[i].Name;
                if (LocalizationLookup.TryGetValue(locName, out var loc))
                {
                    loc.Add(Localizations[i]);
                }
                else
                {
                    LocalizationLookup.Add(locName, new List<LibSWBF2.Wrappers.Localization> { Localizations[i] });
                }
            }

            // apply queued Lua calls like "AddUnitClass"
            Match?.ApplySchedule();

            Stage = EnvStage.CreateScene;
            CreateScene();
        }

        Timers?.Tick(deltaTime);
        Match?.Tick(deltaTime);
        RTScene.Tick(deltaTime);
    }

    public void TickPhysics(float deltaTime)
    {
        RTScene.TickPhysics(deltaTime);
    }

    public string GetLocalized(string localizedPath, bool bReturnNullIfNotFound=false)
    {
        if (GetLocalized(PhxGameRuntime.Instance.Language, localizedPath, out string localizedUnicode))
        {
            return localizedUnicode;
        }
        if (GetLocalized("english", localizedPath, out localizedUnicode))
        {
            return localizedUnicode;
        }
        return bReturnNullIfNotFound ? null : localizedPath;
    }

    bool GetLocalized(string language, string localizedPath, out string localizedUnicode)
    {
        localizedUnicode = localizedPath;

        List<LibSWBF2.Wrappers.Localization> locs;
        if (!LocalizationLookup.TryGetValue(language, out locs))
        {
            return false;
        }

        for (int i = 0; i < locs.Count; ++i)
        {
            if (locs[i].GetLocalizedWideString(localizedPath, out localizedUnicode))
            {
                return true;
            }
        }
        return false;
    }

    public string GetLocalizedMapName(string mapluafile)
    {
        object[] res = LuaRT.CallLuaFunction("missionlist_GetLocalizedMapName", 2, false, true, mapluafile);
        string mapName = res[0] as string;
        return mapName;
    }

    bool Schedule(PhxPath absPath, out SWBF2Handle handle, string[] subLVLs = null)
    {
        Debug.Assert(CanSchedule);

        if (PathToHandle.TryGetValue(absPath, out handle))
        {
            return true;
        }

        if (absPath.Exists() && absPath.IsFile())
        {
            bool bSoundBank = absPath.HasExtension(".bnk");
            if (bSoundBank)
            {
                handle = EnvCon.AddSoundBank(absPath);
            }
            else
            {
                handle = EnvCon.AddLevel(absPath, subLVLs);
            }
            Loading.Add(new LVL
            {
                Handle = handle,
                bIsSoundBank = absPath.HasExtension(".bnk"),

                DisplayPath = absPath.GetLeaf(2),
                bIsAddon = absPath.Contains("/addon/")
            });

            PathToHandle.Add(absPath, handle);
            return true;
        }

        handle = new SWBF2Handle(ushort.MaxValue);
        return false;
    }

    PhxPath GetLoadscreenPath()
    {
        // First, try grab loadscreen for standard maps
        if (!string.IsNullOrEmpty(InitScriptName))
        {
            PhxPath loadscreenLVL = new PhxPath("load") / (InitScriptName.Substring(0, 4) + ".lvl");
            if ((Path / loadscreenLVL).Exists() || (FallbackPath / loadscreenLVL).Exists())
            {
                return loadscreenLVL;
            } 
        }
        return "load/common.lvl";
    }

    void RunMain()
    {
        Debug.Assert(Stage == EnvStage.ExecuteMain);

        // 1 - execute the main script
        if (!string.IsNullOrEmpty(InitScriptName) && !Execute(InitScriptName))
        {
            Debug.LogErrorFormat("Executing lua main script '{0}' failed!", InitScriptName);
            return;
        }
        OnExecuteMain?.Invoke();

        // 2 - execute the main function -> will call ReadDataFile multiple times
        if (!string.IsNullOrEmpty(InitFunctionName) && LuaRT.CallLuaFunction(InitFunctionName, 0) == null)
        {
            Debug.LogErrorFormat("Executing lua main function '{0}' failed!", InitFunctionName);
            return;
        }

        // 3 - load the (via ReadDataFile) scheduled lvl files
        EnvCon.LoadLevels();
        Stage = EnvStage.LoadingWorld;
    }

    void CreateScene()
    {
        Debug.Assert(Stage == EnvStage.CreateScene);

        // WorldLevel will be null for Main Menu
        if (WorldLevel != null)
        {
            RTScene.Import(WorldLevel.Get<LibSWBF2.Wrappers.World>());
        }

        // 4 - execute post load function AFTER scene has been created
        if (!string.IsNullOrEmpty(PostLoadFunctionName) && LuaRT.CallLuaFunction(PostLoadFunctionName, 0) == null)
        {
            Debug.LogErrorFormat("Executing lua post load function '{0}' failed!", PostLoadFunctionName);
        }

        Stage = EnvStage.Loaded;
        OnLoaded?.Invoke();
    }
}