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
#if UNITY_EDITOR
    public class LevelST
    {
        public LibSWBF2.Wrappers.Level Level;
        public LVLHandle Handle;
        public Path RelativePath;
        public bool bIsFallback;
    }

    public List<LevelST> LVLs = new List<LevelST>();
#endif

    public enum EnvState
    {
        Init, Loading, Loaded
    }

    public bool IsLoading => State == EnvState.Loading;
    public bool IsLoaded => State == EnvState.Loaded;

    public Path Path { get; private set; }
    public Path FallbackPath { get; private set; }
    public EnvState State { get; private set; }

    LuaRuntime LuaRT;

    //uint MissionHandle;
    //uint CommonHandle;

    LibSWBF2.Wrappers.Container EnvCon;


    public static RuntimeEnvironment Create(Path envPath, Path fallbackPath)
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
        rt.ScheduleLVL("mission.lvl");
        rt.ScheduleLVL("common.lvl");
        return rt;
    }

    public LuaRuntime GetLuaRuntime()
    {
        return LuaRT;
    }

    public bool Execute(string scriptName)
    {
        Debug.Assert(State == EnvState.Loaded);
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

    public bool Run(string initScript, string initFn)
    {
        Debug.Assert(State == EnvState.Loaded);

        // 1 - execute the main script
        bool res = Execute(initScript);

        // 2 - execute the main function
        if (res)
        {
            res &= LuaRT.CallLua(initFn);
        }

        // 3 - load via ReadDataFile scheduled lvl files
        if (res)
        {
            //LoadScheduled();
        }

        return res;
    }

    public void LoadScheduled()
    {
        Debug.Assert(State != EnvState.Loading);

        EnvCon.LoadLevels();
        State = EnvState.Loading;
    }

    public float GetLoadingProgress()
    {
        return EnvCon.GetOverallProgress();
    }

    public LVLHandle ScheduleLVL(Path relativeLVLPath, string[] subLVLs = null)
    {
        Debug.Assert(State != EnvState.Loading);

        Path envLVLPath = Path / relativeLVLPath;
        Path fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists())
        {
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(envLVLPath, subLVLs));
#if UNITY_EDITOR
            LVLs.Add(new LevelST
            { 
                Level = null, 
                Handle = handle,
                RelativePath = relativeLVLPath,
                bIsFallback = false
            });
#endif
            return handle;
        }
        if (fallbackLVLPath.Exists())
        {
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(fallbackLVLPath, subLVLs));
#if UNITY_EDITOR
            LVLs.Add(new LevelST
            {
                Level = null,
                Handle = handle,
                RelativePath = relativeLVLPath,
                bIsFallback = true
            });
#endif
            return handle;
        }
        return new LVLHandle(uint.MaxValue);
    }

    public float GetProgress(LVLHandle handle)
    {
        return EnvCon.GetProgress(handle.GetNativeHandle());
    }

    public void Update()
    {
        if (State == EnvState.Loading && EnvCon.IsDone())
        {
            State = EnvState.Loaded;
        }

#if UNITY_EDITOR
        for (int i = 0; i < LVLs.Count; ++i)
        {
            if (LVLs[i].Level == null)
            {
                LVLs[i].Level = EnvCon.GetLevel(LVLs[i].Handle.GetNativeHandle());
            }
        }
#endif
    }

    RuntimeEnvironment(Path path, Path fallbackPath)
    {
        Path = path;
        FallbackPath = fallbackPath;
        State = EnvState.Init;

        LuaRT = new LuaRuntime();
        EnvCon = new LibSWBF2.Wrappers.Container();
    }

    ~RuntimeEnvironment()
    {
        EnvCon.Delete();
    }
}