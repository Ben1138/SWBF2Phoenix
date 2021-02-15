using System;
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
    enum EnvState
    {
        Init, Loading, Loaded, Running
    }

    public bool IsLoading => State == EnvState.Loading;
    public bool IsLoaded => State == EnvState.Loaded;
    public bool IsRunning => State == EnvState.Running;

    Path Path;
    Path FallbackPath;
    EnvState State;
    LuaRuntime LuaRT;

    //uint MissionHandle;
    //uint IngameHandle;
    //uint CommonHandle;

    LibSWBF2.Wrappers.Container EnvCon;
    LibSWBF2.Wrappers.Level MissionLVL;
    LibSWBF2.Wrappers.Level IngameLVL;


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
        //rt.ScheduleLVL("ingame.lvl");
        //rt.ScheduleLVL("common.lvl");
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
        Path envLVLPath = Path / relativeLVLPath;
        Path fallbackLVLPath = FallbackPath / relativeLVLPath;
        if (envLVLPath.Exists())
        {
            return new LVLHandle(EnvCon.AddLevel(envLVLPath, subLVLs));
        }
        if (fallbackLVLPath.Exists())
        {
            return new LVLHandle(EnvCon.AddLevel(fallbackLVLPath, subLVLs));
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