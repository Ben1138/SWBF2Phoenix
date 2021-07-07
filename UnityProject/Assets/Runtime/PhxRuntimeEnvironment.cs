using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2;
using LibSWBF2.Enums;
using LibSWBF2.Wrappers;


/*
 * Phases of PhxRuntimeEnvironment:
 * 
 * 1. Init                          Environment is created, with base LVLs scheduled and ready to load
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
    public class LoadST
    {
        public SWBF2Handle Handle;
        public PhxPath PathPartial;
        public bool bIsFallback;
        public bool bIsSoundBank;
    }
    public class LevelST
    {
        public LibSWBF2.Wrappers.Level Level;
        public PhxPath RelativePath;
        public bool bIsFallback;
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


    public List<LevelST> LVLs = new List<LevelST>();
    public List<LoadST>  LoadingLVLs = new List<LoadST>();

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
    Level     WorldLevel;     // points to level inside 'LVLs'
    Level     LoadscreenLVL;  // points to level inside 'LVLs'

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
        rt.ScheduleLVLRel("core.lvl");
        rt.ScheduleLVLRel("shell.lvl");
        rt.ScheduleLVLRel("common.lvl");
        rt.ScheduleLVLRel("mission.lvl");
        rt.ScheduleSoundBankRel("sound/common.bnk");

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

        /*
        if (script.Name == "missionlist")
        {
            return LuaRT.ExecuteFile("/Volumes/boot/bf2modding/assets/Shell/scripts/missionlist.lua");
        }
        */

        return LuaRT.Execute(luaBin, size, script.Name);
    }

    public void Run(string initScript, string initFn = null, string postLoadFn = null)
    {
        Debug.Assert(Stage == EnvStage.Init);
        Loader.ResetAllLoaders();

        InitScriptName = initScript;
        InitFunctionName = initFn;
        PostLoadFunctionName = postLoadFn;

        LoadscreenHandle = ScheduleLVLRel(GetLoadscreenPath());
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

    public SWBF2Handle ScheduleLVLAbs(PhxPath absoluteLVLPath, string[] subLVLs = null, bool bForceLocal = false)
    {
        Debug.Assert(CanSchedule);

        PhxPath leafPath = absoluteLVLPath.GetLeafs(2);
        if (PathToHandle.TryGetValue(leafPath, out SWBF2Handle handle))
        {
            return handle;
        }

        if (absoluteLVLPath.Exists() && absoluteLVLPath.IsFile())
        {
            handle = EnvCon.AddLevel(absoluteLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = leafPath,
                bIsFallback = false,
                bIsSoundBank = false
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", absoluteLVLPath);
        return new SWBF2Handle(ushort.MaxValue);
    }

    // relativeLVLPath: relative to Environment!
    public SWBF2Handle ScheduleLVLRel(PhxPath relativeLVLPath, string[] subLVLs = null, bool bForceLocal = false)
    {
        Debug.Assert(CanSchedule);

        PhxPath leafPath = relativeLVLPath.GetLeafs(2);
        if (PathToHandle.TryGetValue(leafPath, out SWBF2Handle handle))
        {
            return handle;
        }

        PhxPath envLVLPath = Path / relativeLVLPath;
        PhxPath fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists() && envLVLPath.IsFile())
        {
            handle = EnvCon.AddLevel(envLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            { 
                Handle = handle,
                PathPartial = leafPath,
                bIsFallback = false,
                bIsSoundBank = false
            });
            return handle;
        }
        if (fallbackLVLPath.Exists() && fallbackLVLPath.IsFile())
        {
            handle = EnvCon.AddLevel(fallbackLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = leafPath,
                bIsFallback = true,
                bIsSoundBank = false
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relativeLVLPath);
        return new SWBF2Handle(ushort.MaxValue);
    }

    // relativeLVLPath: relative to Environment!
    public SWBF2Handle ScheduleSoundBankRel(PhxPath relativeLVLPath)
    {
        Debug.Assert(CanSchedule);

        PhxPath leafPath = relativeLVLPath.GetLeafs(2);
        if (PathToHandle.TryGetValue(leafPath, out SWBF2Handle handle))
        {
            return handle;
        }

        PhxPath envLVLPath = Path / relativeLVLPath;
        PhxPath fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists() && envLVLPath.IsFile())
        {
            handle = EnvCon.AddSoundBank(envLVLPath);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = leafPath,
                bIsFallback = false,
                bIsSoundBank = true
            });
            return handle;
        }
        if (fallbackLVLPath.Exists() && fallbackLVLPath.IsFile())
        {
            handle = EnvCon.AddSoundBank(fallbackLVLPath);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = leafPath,
                bIsFallback = true,
                bIsSoundBank = true
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relativeLVLPath);
        return new SWBF2Handle(ushort.MaxValue);
    }

    public float GetProgress(SWBF2Handle handle)
    {
        return EnvCon.GetProgress(handle);
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < LoadingLVLs.Count; ++i)
        {
            if (LoadingLVLs[i].bIsSoundBank)
            {
                if (EnvCon.GetStatus(LoadingLVLs[i].Handle) == ELoadStatus.Loaded)
                {
                    LoadingLVLs.RemoveAt(i);
                }
                else if (EnvCon.GetStatus(LoadingLVLs[i].Handle) == ELoadStatus.Failed)
                {
                    Debug.LogErrorFormat("Loading '{0}' failed!", LoadingLVLs[i].PathPartial);
                    LoadingLVLs.RemoveAt(i);
                }
            }
            else
            {
                // will return a Level instance ONLY IF loaded
                var lvl = EnvCon.GetLevel(LoadingLVLs[i].Handle);
                if (lvl != null)
                {
                    LVLs.Add(new LevelST
                    {
                        Level = lvl,
                        RelativePath = LoadingLVLs[i].PathPartial,
                        bIsFallback = LoadingLVLs[i].bIsFallback
                    });
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
                    Localizations.AddRange(lvl.Get<LibSWBF2.Wrappers.Localization>());

                    LoadingLVLs.RemoveAt(i);
                    break; // do not further iterate altered list
                }
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

            if (EnvCon.IsDone() && LoadingLVLs.Count == 0)
            {
                Stage = EnvStage.ExecuteMain;
                RunMain();
            }
        }

        if (Stage == EnvStage.LoadingWorld && EnvCon.IsDone() && LoadingLVLs.Count == 0)
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