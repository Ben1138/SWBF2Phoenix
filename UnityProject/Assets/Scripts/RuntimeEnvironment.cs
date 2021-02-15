using System;
using System.Collections.Generic;
using UnityEngine;


public struct LVLHandle
{
    uint NativeHandle;
    Path RelativePath;

    public LVLHandle(uint nativeHandle, Path relPath)
    {
        NativeHandle = nativeHandle;
        RelativePath = relPath;
    }

    public string GetRelativePath() => RelativePath;
    public uint GetNativeHandle() => NativeHandle;

    public bool IsValid() => NativeHandle != uint.MaxValue;
}

public class RuntimeEnvironment
{
    public enum EnvState
    {
        Init, Loading, Loaded
    }

    public bool IsLoading => State == EnvState.Loading;
    public bool IsLoaded => State == EnvState.Loaded;

    // for monitoring only
    public List<LibSWBF2.Wrappers.Level> LoadedLVLs = new List<LibSWBF2.Wrappers.Level>();
    public List<LVLHandle> LoadingLVLs = new List<LVLHandle>();

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
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(envLVLPath, subLVLs), relativeLVLPath);
            LoadingLVLs.Add(handle);
            return handle;
        }
        if (fallbackLVLPath.Exists())
        {
            LVLHandle handle = new LVLHandle(EnvCon.AddLevel(fallbackLVLPath, subLVLs), relativeLVLPath);
            LoadingLVLs.Add(handle);
            return handle;
        }
        return new LVLHandle(uint.MaxValue, "");
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

        for (int i = 0; i < LoadingLVLs.Count; ++i)
        {
            var lvl = EnvCon.GetLevel(LoadingLVLs[i].GetNativeHandle());
            if (lvl != null)
            {
                LoadingLVLs.RemoveAt(i);
                LoadedLVLs.Add(lvl);
                break; // do not further iterate altered list
            }
        }
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