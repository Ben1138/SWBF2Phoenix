using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class LuaRuntime
{
    Lua L;

    public LuaRuntime()
    {
        L = new Lua();

        L.OpenBase();
        L.OpenMath();
        L.OpenString();
        L.OpenTable();

        RegisterLuaFunctions(typeof(LuaFunctions));
        L.AtPanic(Panic);
    }

    public Lua GetLua()
    {
        return L;
    }

    public Lua.ValueType ToLuaType(Type T)
    {
        if (T == typeof(float) ||
            T == typeof(double) ||
            //T == typeof(byte) ||
            //T == typeof(sbyte) ||
            T == typeof(short) ||
            T == typeof(ushort) ||
            T == typeof(int) ||
            T == typeof(uint) ||
            T == typeof(long) ||
            T == typeof(ulong))
        {
            return Lua.ValueType.NUMBER;
        }
        else if (T == typeof(string))
        {
            return Lua.ValueType.STRING;
        }
        else if (T == typeof(bool))
        {
            return Lua.ValueType.BOOLEAN;
        }

        return Lua.ValueType.NONE;
    }

    public T ToValue<T>(int idx)
    {
        Lua.ValueType desiredType = ToLuaType(typeof(T));
        if (desiredType == Lua.ValueType.NONE)
        {
            Debug.LogErrorFormat("Cannot convert Type '{0}' to a Lua Type!", typeof(T).Name);
            return default(T);
        }

        Lua.ValueType type = L.Type(idx);
        if (desiredType != type)
        {
            Debug.LogErrorFormat("Desired Type is '{0}', but Lua Type is '{0}'!", desiredType.ToString(), type.ToString());
            return default(T);
        }

        object res = null;
        switch (type)
        {
            case Lua.ValueType.NUMBER:
                res = L.ToNumber(idx);
                break;
            case Lua.ValueType.STRING:
                res = L.ToString(idx);
                break;
            case Lua.ValueType.BOOLEAN:
                res = L.ToBoolean(idx);
                break;
            default:
                Debug.LogErrorFormat("Cannot convert Lua Type '{0}' to C# primitive!", type.ToString());
                return default(T);
        }
        return (T)res;
    }

    public (string, string) LuaValueToStr(int idx)
    {
        Lua.ValueType type = L.Type(idx);

        string typeStr = type.ToString();
        string valueStr = "";
        switch (type)
        {
            case Lua.ValueType.NIL:
                valueStr = "NIL";
                break;
            case Lua.ValueType.NUMBER:
                valueStr = L.ToNumber(idx).ToString();
                break;
            case Lua.ValueType.STRING:
                valueStr = L.ToString(idx).ToString();
                break;
            case Lua.ValueType.BOOLEAN:
                valueStr = L.ToBoolean(idx).ToString();
                break;
            case Lua.ValueType.FUNCTION:
                valueStr = L.ToFunction(idx).ToString();
                break;
            case Lua.ValueType.LIGHTUSERDATA:
                valueStr = L.ToUserData(idx).ToString();
                break;
            case Lua.ValueType.TABLE:
                // TODO
                valueStr = "???";
                break;
            case Lua.ValueType.THREAD:
                valueStr = L.ToThread(idx).ToString();
                break;
            case Lua.ValueType.NONE:
            default:
                Debug.LogErrorFormat("Cannot convert Lua Type '{0}' to C# primitive!", type.ToString());
                valueStr = "UNKNOWN";
                break;
        }
        return (typeStr, valueStr);
    }

    public void Register<T>(string fnName, T fn) where T : Delegate
    {
        
        MethodInfo info = fn.GetMethodInfo();
        info.GetParameters();
    }

    public bool Execute(IntPtr binary, ulong buffSize, string name)
    {
        return PrintError(L.DoBuffer(binary, buffSize, name));
    }

    public bool ExecuteFile(string path)
    {
        return PrintError(L.DoFile(path));
    }

    public bool ExecuteString(string luaCode)
    {
        return PrintError(L.DoString(luaCode));
    }

    int Panic(Lua l)
    {
        Debug.LogError("LUA PANIC!");
        return 0;
    }

    void RegisterLuaFunctions(Type sourceClass)
    {
        if (!sourceClass.IsClass || !sourceClass.IsAbstract || !sourceClass.IsSealed)
        {
            Debug.LogErrorFormat("Cannot register Lua functions from non-static class '{0}'!", sourceClass.Name);
            return;
        }

        MethodInfo[] methods = sourceClass.GetMethods();
        foreach (MethodInfo mi in methods)
        {
            // Ignore CLR provided standard methods
            if (mi.Name == "Equals" ||
                mi.Name == "GetHashCode" ||
                mi.Name == "GetType" ||
                mi.Name == "ToString")
            {
                continue;
            }

            if (!mi.IsStatic)
            {
                Debug.LogWarningFormat("Skipping non-static Lua function '{0}'!", mi.Name);
                continue;
            }

            Lua.Function fn = (Lua l) =>
            {
                int inStack = 0;
                int outStack = 0;

                int expectedParams = l.GetTop();

                ParameterInfo[] parameters = mi.GetParameters();
                if (expectedParams != parameters.Length)
                {
                    Debug.LogErrorFormat("Lua expects {0} parameters, but '{1}' got {2}", expectedParams, mi.Name, parameters.Length);
                    return 0;
                }

                object[] invokeParams = new object[parameters.Length];

                // In-Parameters
                foreach (ParameterInfo pi in parameters)
                {
                    Lua.ValueType type = ToLuaType(pi.ParameterType);
                    if (type == Lua.ValueType.NUMBER)
                    {
                        invokeParams[inStack] = l.ToNumber(++inStack);
                    }
                    else if (type == Lua.ValueType.STRING)
                    {
                        invokeParams[inStack] = l.ToString(++inStack);
                    }
                    else if (type == Lua.ValueType.BOOLEAN)
                    {
                        invokeParams[inStack] = l.ToBoolean(++inStack);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unsupported parameter type '{0}' in lua function '{1}'", pi.ParameterType.Name, mi.Name);
                        continue;
                    }
                }

                object outParam = mi.Invoke(null, invokeParams);

                // Out-Parameter
                if (mi.ReturnType != typeof(void))
                {
                    Lua.ValueType type = ToLuaType(mi.ReturnType);
                    if (type == Lua.ValueType.NUMBER)
                    {
                        l.PushNumber((float)outParam);
                        outStack++;
                    }
                    else if (type == Lua.ValueType.STRING)
                    {
                        l.PushString((string)outParam);
                        outStack++;
                    }
                    else if (type == Lua.ValueType.BOOLEAN)
                    {
                        l.PushBoolean((bool)outParam);
                        outStack++;
                    }
                    //else if (type == typeof(Tuple))
                    //{
                    //    throw new Exception("Not implemented!");
                    //}
                    else
                    {
                        Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", mi.ReturnType.Name, mi.Name);
                        return 0;
                    }
                }

                return outStack;
            };

            L.Register(mi.Name, fn);
        }
    }

    bool PrintError(Lua.ErrorCode error)
    {
        if (error != Lua.ErrorCode.NONE)
        {
            int stack = L.GetTop();
            if (stack > 0)
            {
                string luaErrMsg = L.ToString(-1);
                Debug.LogErrorFormat("[LUA] {0} ERROR: {1}", error.ToString(), luaErrMsg);
            }
            else
            {
                Debug.LogErrorFormat("[LUA] {0} ERROR", error.ToString());
            }
            Debug.LogErrorFormat(error.ToString());
        }
        return error == Lua.ErrorCode.NONE;
    }
}

public static class LuaFunctions
{
    static string ScriptCB_GetPlatform()
    {
        return "PC";
    }

    static void ScriptCB_DoFile(string scriptName)
    {
        
    }

    static void AddDownloadableContent(string threeLetterName, string scriptName, int levelMemoryModifier)
    {

    }
}
