using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Linq;

public class LuaRuntime
{
    Lua L;

    public LuaRuntime()
    {
        L = new Lua();
        L.AtPanic(Panic);
        L.OpenBase();
        L.OpenMath();
        L.OpenString();
        L.OpenTable();
        RegisterLuaFunctions(typeof(GameLuaAPI));
        
        Register<Action<string[]>>(LogWarn);
    }

    public Lua GetLua()
    {
        return L;
    }

    public static Lua.ValueType ToLuaType(Type T)
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
            return default;
        }

        Lua.ValueType type = L.Type(idx);
        if (desiredType != type)
        {
            Debug.LogErrorFormat("Desired Type is '{0}', but Lua Type is '{0}'!", desiredType.ToString(), type.ToString());
            return default;
        }

        object res;
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
                return default;
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
                // TODO: proper table display (e.g. num of entries, or sth)
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

    // this is so dumb...
    bool IsTuple(Type type)
    {
        if (type.IsGenericType)
        {
            Type genType = type.GetGenericTypeDefinition();
            return genType == typeof(ValueTuple<>)
                || genType == typeof(ValueTuple<,>)
                || genType == typeof(ValueTuple<,,>)
                || genType == typeof(ValueTuple<,,,>)
                || genType == typeof(ValueTuple<,,,,>)
                || genType == typeof(ValueTuple<,,,,,>)
                || genType == typeof(ValueTuple<,,,,,,>)
                || genType == typeof(ValueTuple<,,,,,,,>)
                || genType == typeof(ValueTuple<,,,,,,,>);
        }
        return false;
    }

    bool Register<T>(T method) where T : Delegate
    {
        return Register(method.Method);
    }

    bool Register(MethodInfo mi)
    {
        if (!mi.IsStatic)
        {
            Debug.LogWarningFormat("Cannor register non-static method '{0}' as Lua function!", mi.Name);
            return false;
        }

        Lua.Function fn = (Lua l) =>
        {
            int inStack = 0;
            int outStack = 0;

            int expectedParams = l.GetTop();

            ParameterInfo[] parameters = mi.GetParameters();
            object[] invokeParams = new object[parameters.Length];

            // Special case: dynamic parameters
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object[]))
            {
                object[] dynParams = new object[expectedParams];
                for (int i = 0; i < expectedParams; ++i)
                {
                    Lua.ValueType luaType = l.Type(i + 1);

                    if (luaType == Lua.ValueType.NIL)
                    {
                        dynParams[i] = null;
                    }
                    else if (luaType == Lua.ValueType.NUMBER)
                    {
                        dynParams[i] = l.ToNumber(i + 1);
                    }
                    else if (luaType == Lua.ValueType.STRING)
                    {
                        dynParams[i] = l.ToString(i + 1);
                    }
                    else if (luaType == Lua.ValueType.BOOLEAN)
                    {
                        dynParams[i] = l.ToBoolean(i + 1);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Cannot convert lua function parameter '{0}' to C# primitive in function '{1}'", luaType.ToString(), mi.Name);
                        continue;
                    }
                }
                invokeParams[0] = dynParams;
            }
            else
            {
                if (expectedParams != parameters.Length)
                {
                    Debug.LogErrorFormat("Lua expects {0} parameters, but '{1}' got {2}", expectedParams, mi.Name, parameters.Length);
                    return 0;
                }

                // In-Parameters
                foreach (ParameterInfo pi in parameters)
                {
                    Lua.ValueType funcType = ToLuaType(pi.ParameterType);
                    if (funcType == Lua.ValueType.NONE)
                    {
                        Debug.LogErrorFormat("Unsupported parameter type '{0}' in lua function '{1}'", pi.ParameterType.Name, mi.Name);
                        continue;
                    }

                    int paramIdx = inStack;
                    int luaIdx = ++inStack;

                    Lua.ValueType luaType = l.Type(luaIdx);
                    if (luaType != funcType)
                    {
                        Debug.LogErrorFormat("Unexpected parameter type '{0}' of parameter '{1}' in function '{2}'! Lua expected '{3}'", funcType.ToString(), pi.Name, mi.Name, luaType.ToString());
                        continue;
                    }

                    if (funcType == Lua.ValueType.NIL)
                    {
                        invokeParams[paramIdx] = null;
                    }
                    else if (funcType == Lua.ValueType.NUMBER)
                    {
                        invokeParams[paramIdx] = l.ToNumber(luaIdx);
                    }
                    else if (funcType == Lua.ValueType.STRING)
                    {
                        invokeParams[paramIdx] = l.ToString(luaIdx);
                    }
                    else if (funcType == Lua.ValueType.BOOLEAN)
                    {
                        invokeParams[paramIdx] = l.ToBoolean(luaIdx);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unsupported parameter type '{0}' in lua function '{1}'", pi.ParameterType.Name, mi.Name);
                        continue;
                    }
                }
            }

            object outParam = mi.Invoke(null, invokeParams);

            // Out-Parameter
            if (mi.ReturnType != typeof(void))
            {
                if (IsTuple(mi.ReturnType))
                {
                    FieldInfo[] fields = mi.ReturnType.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (!HandleOutParam(l, field.FieldType, ref outStack, field.GetValue(outParam)))
                        {
                            Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", mi.ReturnType.Name, mi.Name);
                            return 0;
                        }
                    }
                }
                else
                {
                    if (!HandleOutParam(l, mi.ReturnType, ref outStack, outParam))
                    {
                        Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", mi.ReturnType.Name, mi.Name);
                        return 0;
                    }
                }
            }

            return outStack;
        };

        L.Register(mi.Name, fn);
        Debug.LogFormat("Registered Lua function '{0}'", mi.Name);
        return true;
    }

    static bool HandleOutParam(Lua l, Type t, ref int stackCounter, object outParam)
    {
        Lua.ValueType type = ToLuaType(t);
        if (type == Lua.ValueType.NUMBER)
        {
            l.PushNumber(Convert.ToSingle(outParam));
            stackCounter++;
            return true;
        }
        else if (type == Lua.ValueType.STRING)
        {
            l.PushString(Convert.ToString(outParam));
            stackCounter++;
            return true;
        }
        else if (type == Lua.ValueType.BOOLEAN)
        {
            l.PushBoolean(Convert.ToBoolean(outParam));
            stackCounter++;
            return true;
        }
        else if (t == typeof(byte[]))
        {
            l.PushLString((byte[])outParam);
            stackCounter++;
            return true;
        }
        return false;
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

            Register(mi);
        }
    }

    bool PrintError(Lua.ErrorCode error)
    {
        if (error != Lua.ErrorCode.NONE)
        {
            int stack = L.GetTop();
            if (stack > 0)
            {
                string luaErrMsg = L.ToString(0);
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

    static void LogWarn(object[] msg)
    {
        Debug.LogWarning(string.Join("", msg));
    }
}
