using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2;
using LibSWBF2.Enums;


/*
 * Phases of RuntimeEnvironment:
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


public class LoadscreenLoadedEventsArgs : EventArgs
{
    public LoadscreenLoadedEventsArgs(Texture2D tex)
    {
        LoadscreenTexture = tex;
    }

    public Texture2D LoadscreenTexture;
}

public class RuntimeEnvironment
{
    public class LoadST
    {
        public SWBF2Handle Handle;
        public RPath PathPartial;
        public bool bIsFallback;
        public bool bIsSoundBank;
    }
    public class LevelST
    {
        public LibSWBF2.Wrappers.Level Level;
        public RPath RelativePath;
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

    public bool                                     IsLoaded => Stage == EnvStage.Loaded;
    public EventHandler<LoadscreenLoadedEventsArgs> OnLoadscreenLoaded;
    public EventHandler<EventArgs>                  OnExecuteMain;
    public EventHandler<EventArgs>                  OnLoaded;

    public RPath Path           { get; private set; }
    public RPath FallbackPath   { get; private set; }
    public EnvStage Stage       { get; private set; }

    bool CanSchedule => Stage == EnvStage.Init || Stage == EnvStage.ExecuteMain;
    bool CanExecute  => Stage == EnvStage.ExecuteMain || Stage == EnvStage.CreateScene || Stage == EnvStage.Loaded;

    LuaRuntime  LuaRT;
    SWBF2Handle LoadscreenHandle;

    LibSWBF2.Wrappers.Container EnvCon;
    LibSWBF2.Wrappers.Level     WorldLevel;     // points to world level inside 'LVLs'
    LibSWBF2.Wrappers.Level     LoadscreenLVL;  // points to level inside 'LVLs'

    string InitScriptName;
    string InitFunctionName;
    string PostLoadFunctionName;

    List<GameObject> SceneRoots = new List<GameObject>();
    Dictionary<string, ISWBFGameClass> WorldInstances = new Dictionary<string, ISWBFGameClass>(StringComparer.InvariantCultureIgnoreCase);


    RuntimeEnvironment(RPath path, RPath fallbackPath)
    {
        Path = path;
        FallbackPath = fallbackPath;
        Stage = EnvStage.Init;
        WorldLevel = null;

        LuaRT = new LuaRuntime();
        EnvCon = new LibSWBF2.Wrappers.Container();

        Loader.SetGlobalContainer(EnvCon);
    }

    ~RuntimeEnvironment()
    {
        EnvCon.Delete();
    }


    public static RuntimeEnvironment Create(RPath envPath, RPath fallbackPath = null)
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

        RuntimeEnvironment rt = new RuntimeEnvironment(envPath, fallbackPath);
        rt.ScheduleLVLRel("core.lvl");
        rt.ScheduleLVLRel("shell.lvl");
        rt.ScheduleLVLRel("common.lvl");
        rt.ScheduleLVLRel("mission.lvl");
        rt.ScheduleSoundBankRel("sound/common.bnk");
        return rt;
    }

    public LuaRuntime GetLuaRuntime()
    {
        return LuaRT;
    }

    public T Find<T>(string name) where T : LibSWBF2.Wrappers.NativeWrapper, new()
    {
        return EnvCon.Get<T>(name);
    }

    public bool Execute(string scriptName)
    {
        Debug.Assert(CanExecute);
        LibSWBF2.Wrappers.Script script = EnvCon.Get<LibSWBF2.Wrappers.Script>(scriptName);
        if (script == null || !script.IsValid())
        {
            Debug.LogErrorFormat("Couldn't find script '{0}'!", scriptName);
            return false;
        }
        return Execute(script);
    }

    public bool Execute(LibSWBF2.Wrappers.Script script)
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
        Debug.Assert(SceneRoots.Count == 0);
        Loader.ResetAllLoaders();

        InitScriptName = initScript;
        InitFunctionName = initFn;
        PostLoadFunctionName = postLoadFn;

        LoadscreenHandle = ScheduleLVLRel(GetLoadscreenPath());
        EnvCon.LoadLevels();

        Stage = EnvStage.LoadingBase;
    }

    public void ClearScene()
    {
        for (int i = 0; i < SceneRoots.Count; ++i)
        {
            UnityEngine.Object.Destroy(SceneRoots[i]);
        }
        WorldInstances.Clear();
    }

    public float GetLoadingProgress()
    {
        float stageContribution = 1.0f / 2.0f;
        float stageProgress = Stage >= EnvStage.LoadingWorld ? stageContribution : 0.0f;

        return stageProgress + stageContribution * EnvCon.GetOverallProgress();
    }

    public SWBF2Handle ScheduleLVLAbs(RPath absoluteLVLPath, string[] subLVLs = null, bool bForceLocal = false)
    {
        Debug.Assert(CanSchedule);

        if (absoluteLVLPath.Exists() && absoluteLVLPath.IsFile())
        {
            SWBF2Handle handle = EnvCon.AddLevel(absoluteLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = absoluteLVLPath.GetLeafs(2),
                bIsFallback = false,
                bIsSoundBank = false
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", absoluteLVLPath);
        return new SWBF2Handle(uint.MaxValue);
    }

    // relativeLVLPath: relative to Environment!
    public SWBF2Handle ScheduleLVLRel(RPath relativeLVLPath, string[] subLVLs = null, bool bForceLocal = false)
    {
        Debug.Assert(CanSchedule);

        RPath envLVLPath = Path / relativeLVLPath;
        RPath fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists() && envLVLPath.IsFile())
        {
            SWBF2Handle handle = EnvCon.AddLevel(envLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            { 
                Handle = handle,
                PathPartial = relativeLVLPath.GetLeafs(2),
                bIsFallback = false,
                bIsSoundBank = false
            });
            return handle;
        }
        if (!bForceLocal && fallbackLVLPath.Exists() && fallbackLVLPath.IsFile())
        {
            SWBF2Handle handle = EnvCon.AddLevel(fallbackLVLPath, subLVLs);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = relativeLVLPath.GetLeafs(2),
                bIsFallback = true,
                bIsSoundBank = false
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relativeLVLPath);
        return new SWBF2Handle(uint.MaxValue);
    }

    // relativeLVLPath: relative to Environment!
    public SWBF2Handle ScheduleSoundBankRel(RPath relativeLVLPath)
    {
        Debug.Assert(CanSchedule);

        RPath envLVLPath = Path / relativeLVLPath;
        RPath fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists() && envLVLPath.IsFile())
        {
            SWBF2Handle handle = EnvCon.AddSoundBank(envLVLPath);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = relativeLVLPath.GetLeafs(2),
                bIsFallback = false,
                bIsSoundBank = true
            });
            return handle;
        }
        if (fallbackLVLPath.Exists() && fallbackLVLPath.IsFile())
        {
            SWBF2Handle handle = EnvCon.AddSoundBank(fallbackLVLPath);
            LoadingLVLs.Add(new LoadST
            {
                Handle = handle,
                PathPartial = relativeLVLPath.GetLeafs(2),
                bIsFallback = true,
                bIsSoundBank = true
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relativeLVLPath);
        return new SWBF2Handle(uint.MaxValue);
    }

    public float GetProgress(SWBF2Handle handle)
    {
        return EnvCon.GetProgress(handle);
    }

    public void Update()
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
                    OnLoadscreenLoaded?.Invoke(this, new LoadscreenLoadedEventsArgs(TextureLoader.Instance.ImportUITexture(textures[texIdx].Name)));
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
            Stage = EnvStage.CreateScene;
            CreateScene();
        }
    }

    public void SetProperty(string instName, string propName, object propValue)
    {
        if (WorldInstances.TryGetValue(instName, out ISWBFGameClass classScript))
        {
            classScript.SetProperty(propName, propValue);
        }
    }

    RPath GetLoadscreenPath()
    {
        // First, try grab loadscreen for standard maps
        RPath loadscreenLVL = new RPath("load") / (InitScriptName.Substring(0, 4) + ".lvl");
        if ((Path / loadscreenLVL).Exists() || (FallbackPath / loadscreenLVL).Exists())
        {
            return loadscreenLVL;
        }
        return "load/common.lvl";
    }

    void RunMain()
    {
        Debug.Assert(Stage == EnvStage.ExecuteMain);

        // 1 - execute the main script
        if (!Execute(InitScriptName))
        {
            Debug.LogErrorFormat("Executing lua main script '{0}' failed!", InitScriptName);
            return;
        }
        OnExecuteMain?.Invoke(this, null);

        // 2 - execute the main function -> will call ReadDataFile multiple times
        if (!string.IsNullOrEmpty(InitFunctionName) && !LuaRT.CallLua(InitFunctionName))
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
            bool hasTerrain = false;
            WorldLoader.Instance.TerrainAsMesh = true;

            List<(GameObject, ISWBFClass)> instances = new List<(GameObject, ISWBFClass)>();
            foreach (var world in WorldLevel.Get<LibSWBF2.Wrappers.World>())
            {
                WorldLoader.Instance.ImportTerrain = !hasTerrain;
                SceneRoots.Add(WorldLoader.Instance.ImportWorld(world, out hasTerrain, out instances));

                foreach ((GameObject, ISWBFGameClass) inst in instances)
                {
                    WorldInstances.Add(inst.Item1.name, inst.Item2);
                }
            }
        }

        // 4 - execute post load function AFTER scene has been created
        if (!string.IsNullOrEmpty(PostLoadFunctionName) && !LuaRT.CallLua(PostLoadFunctionName))
        {
            Debug.LogErrorFormat("Executing lua post load function '{0}' failed!", PostLoadFunctionName);
        }

        Stage = EnvStage.Loaded;
        OnLoaded?.Invoke(this, null);
    }
}