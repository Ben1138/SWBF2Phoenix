using System;
using System.Collections.Generic;
using UnityEngine;


public struct LVLHandle
{
    uint NativeHandle;

    public LVLHandle(uint nativeHandle)
    {
        NativeHandle = nativeHandle;
    }
    public uint GetNativeHandle() => NativeHandle;

    public bool IsValid() => NativeHandle != uint.MaxValue;
}

public class RuntimeEnvironment
{
    public class LevelLoadST
    {
        public LVLHandle Handle;
        public Path RelativePath;
        public bool bIsFallback;
    }

    public class LevelST
    {
        public LibSWBF2.Wrappers.Level Level;
        public Path RelativePath;
        public bool bIsFallback;
    }

    public List<LevelST> LVLs = new List<LevelST>();
    public List<LevelLoadST> LoadingLVLs = new List<LevelLoadST>();

    public enum EnvStage
    {
        Init,           // env creation, base lvls (mission.lvl, ...) scheduled
        LoadingBase,    // loading base lvls
        ExecuteMain,    // execute lua main function -> scheduling world lvls
        LoadingWorld,   // loading world lvls
        CreateScene,    // create unity scene, convert all meshes, etc. + call optional lua post load function
        Loaded          // ready to rumble
    }

    public bool IsLoaded => Stage == EnvStage.Loaded;

    public Path Path { get; private set; }
    public Path FallbackPath { get; private set; }
    public EnvStage Stage { get; private set; }

    bool CanSchedule => Stage == EnvStage.Init || Stage == EnvStage.ExecuteMain;
    bool CanExecute => Stage == EnvStage.ExecuteMain || Stage == EnvStage.CreateScene || Stage == EnvStage.Loaded;

    LuaRuntime LuaRT;
    LibSWBF2.Wrappers.Container EnvCon;

    // points to world level inside 'LVLs'
    LibSWBF2.Wrappers.Level WorldLevel;

    string InitScriptName;
    string InitFunctionName;
    string PostLoadFunctionName;

    public static RuntimeEnvironment Create(Path envPath, Path fallbackPath = null)
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
        rt.Stage = EnvStage.Init;
        rt.WorldLevel = null;
        rt.ScheduleLVL("mission.lvl");
        //rt.ScheduleLVL("common.lvl");
        //rt.ScheduleLVL("core.lvl");
        return rt;
    }

    public LuaRuntime GetLuaRuntime()
    {
        return LuaRT;
    }

    public bool Execute(string scriptName)
    {
        Debug.Assert(CanExecute);
        LibSWBF2.Wrappers.Script s = EnvCon.FindWrapper<LibSWBF2.Wrappers.Script>(scriptName);
        if (s == null || !s.IsValid())
        {
            Debug.LogErrorFormat("Couldn't find script '{0}'!", scriptName);
            return false;
        }

        if (!s.GetData(out IntPtr luaBin, out uint size))
        {
            Debug.LogErrorFormat("Couldn't grab lua binary code from script '{0}'!", scriptName);
            return false;
        }

        return LuaRT.Execute(luaBin, size, s.Name);
    }

    public void Run(string initScript, string initFn = null, string postLoadFn = null)
    {
        Debug.Assert(Stage == EnvStage.Init);

        InitScriptName = initScript;
        InitFunctionName = initFn;
        PostLoadFunctionName = postLoadFn;

        EnvCon.LoadLevels();
        Stage = EnvStage.LoadingBase;
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

        // 2 - execute the main function
        if (!string.IsNullOrEmpty(InitFunctionName) && !LuaRT.CallLua(InitFunctionName))
        {
            Debug.LogErrorFormat("Executing lua main function '{0}' failed!", InitFunctionName);
            return;
        }

        LuaRT.CallLua(PostLoadFunctionName);

        // 3 - load via ReadDataFile scheduled lvl files
        //EnvCon.LoadLevels();
        //Stage = EnvStage.LoadingWorld;
    }

    void CreateScene()
    {
        Debug.Assert(Stage == EnvStage.CreateScene);

        // TODO: world level conversion starts here

        // 4 - execute post load function AFTER scene has been created
        if (!string.IsNullOrEmpty(PostLoadFunctionName) && !LuaRT.CallLua(PostLoadFunctionName))
        {
            Debug.LogErrorFormat("Executing lua post load function '{0}' failed!", PostLoadFunctionName);
        }

        Stage = EnvStage.Loaded;
    }

    public float GetLoadingProgress()
    {
        return EnvCon.GetOverallProgress();
    }

    public LVLHandle ScheduleLVL(Path relativeLVLPath, string[] subLVLs = null, bool bForceLocal = false)
    {
        Debug.Assert(CanSchedule);

        Path envLVLPath = Path / relativeLVLPath;
        Path fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists())
        {
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(envLVLPath, subLVLs));
            LoadingLVLs.Add(new LevelLoadST
            { 
                Handle = handle,
                RelativePath = relativeLVLPath,
                bIsFallback = false
            });
            return handle;
        }
        if (!bForceLocal && fallbackLVLPath.Exists())
        {
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(fallbackLVLPath, subLVLs));
            LoadingLVLs.Add(new LevelLoadST
            {
                Handle = handle,
                RelativePath = relativeLVLPath,
                bIsFallback = true
            });
            return handle;
        }

        Debug.LogErrorFormat("Couldn't schedule '{0}'! File not found!", relativeLVLPath);
        return new LVLHandle(uint.MaxValue);
    }

    public float GetProgress(LVLHandle handle)
    {
        return EnvCon.GetProgress(handle.GetNativeHandle());
    }

    public void Update()
    {
        for (int i = 0; i < LoadingLVLs.Count; ++i)
        {
            var lvl = EnvCon.GetLevel(LoadingLVLs[i].Handle.GetNativeHandle());
            if (lvl != null)
            {
                LVLs.Add(new LevelST
                {
                    Level = lvl,
                    RelativePath = LoadingLVLs[i].RelativePath,
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

        if (Stage == EnvStage.LoadingBase && EnvCon.IsDone())
        {
            Stage = EnvStage.ExecuteMain;
            RunMain();
        }

        if (Stage == EnvStage.LoadingWorld && EnvCon.IsDone())
        {
            Stage = EnvStage.CreateScene;
            CreateScene();
        }
    }

    RuntimeEnvironment(Path path, Path fallbackPath)
    {
        Path = path;
        FallbackPath = fallbackPath;
        Stage = EnvStage.Init;

        LuaRT = new LuaRuntime();
        EnvCon = new LibSWBF2.Wrappers.Container();
    }

    ~RuntimeEnvironment()
    {
        EnvCon.Delete();
    }
}