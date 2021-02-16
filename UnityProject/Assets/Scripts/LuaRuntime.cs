using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Linq;

// Don't permanently store references of these! 
public class LuaRuntime
{
    Lua L;

    // Store all MethodInfos for each occouring method name in order to be able to handle overloaded methods
    Dictionary<string, List<MethodInfo>> MethodOverloads = new Dictionary<string, List<MethodInfo>>();

    public LuaRuntime()
    {
        L = new Lua();
        L.AtPanic(Panic);
        L.OpenBase();
        L.OpenMath();
        L.OpenString();
        L.OpenTable();
        RegisterLuaFunctions(typeof(GameLuaAPI));
        
        Register<Action<object[]>>(LogWarn);
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
        else if (T == typeof(Lua.Function))
        {
            return Lua.ValueType.FUNCTION;
        }
        else if (T == typeof(object))
        {
            return Lua.ValueType.NIL;
        }

        return Lua.ValueType.NONE;
    }

    public object ToValue(int idx)
    {
        return ToValue(L, idx);
    }

    public static object ToValue(Lua l, int idx)
    {
        Lua.ValueType type = l.Type(idx);
        object res;
        switch (type)
        {
            case Lua.ValueType.NIL:
                res = null;
                break;
            case Lua.ValueType.NUMBER:
                res = l.ToNumber(idx);
                break;
            case Lua.ValueType.STRING:
                res = l.ToString(idx);
                break;
            case Lua.ValueType.BOOLEAN:
                res = l.ToBoolean(idx);
                break;
            case Lua.ValueType.FUNCTION:
                res = l.ToFunction(idx);
                break;
            default:
                Debug.LogErrorFormat("Cannot convert Lua Type '{0}' to C# equivalent!", type.ToString());
                return null;
        }
        return res;
    }

    public int PushValue<T>(T value)
    {
        return PushValue(L, value);
    }

    public static int PushValue<T>(Lua l, T value)
    {
        return PushValue(l, value, typeof(T));
    }

    public static int PushValue(Lua l, object value, Type T)
    {
        Lua.ValueType type = ToLuaType(T);
        if (type == Lua.ValueType.NUMBER)
        {
            l.PushNumber(Convert.ToSingle(value));
            return 1;
        }
        else if (type == Lua.ValueType.STRING)
        {
            l.PushString(Convert.ToString(value));
            return 1;
        }
        else if (type == Lua.ValueType.BOOLEAN)
        {
            l.PushBoolean(Convert.ToBoolean(value));
            return 1;
        }
        else if (type == Lua.ValueType.FUNCTION)
        {
            l.PushFunction((Lua.Function)value);
            return 1;
        }
        else if (T == typeof(byte[]))
        {
            l.PushLString((byte[])value);
            return 1;
        }

        Debug.LogErrorFormat("Cannot not push C# value of type '{0}' to lua!", T.Name);
        return 0;
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
        if (Check(L.DoBuffer(binary, buffSize, name)))
        {
            Debug.LogFormat("'{0}' executed.", name);
            return true;
        }
        return false;
    }

    public bool ExecuteFile(string path)
    {
        return Check(L.DoFile(path));
    }

    public bool ExecuteString(string luaCode)
    {
        return Check(L.DoString(luaCode));
    }

    public bool CallLua(string fnName)
    {
        bool res = false;
        L.GetGlobal(fnName);
        if (!L.IsFunction(-1))
        {
            Debug.LogErrorFormat("Could not find global function '{0}()'!", fnName);
            return res;
        }
        res = Check(L.PCall(0, 0, 0));
        if (!res)
        {
            L.Pop(2);
        }
        else
        {
            L.Pop(1);
        }
        return res;
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

    static T Cast<T>(object obj)
    {
        return (T)obj;
    }

    bool Register<T>(T method) where T : Delegate
    {
        return Register(method.Method);
    }

    bool Register(MethodInfo mi)
    {
        string methodName = mi.Name;

        if (!mi.IsStatic)
        {
            Debug.LogWarningFormat("Cannor register non-static method '{0}' as Lua function!", methodName);
            return false;
        }

        List<MethodInfo> overloads = null;
        if (!MethodOverloads.TryGetValue(methodName, out overloads))
        {
            overloads = new List<MethodInfo>();
            MethodOverloads.Add(methodName, overloads);
        }
        overloads.Add(mi);

        if (overloads.Count > 1)
        {
            // do not register a method name to Lua more than once!
            return true;
        }

        Lua.Function fn = (Lua l) =>
        {
            int inStackCount = 0;
            int outStackCount = 0;

            int expectedParams = l.GetTop();
            Lua.ValueType[] expectedParamTypes = new Lua.ValueType[expectedParams];
            for (int i = 0; i < expectedParams; ++i)
            {
                expectedParamTypes[i] = l.Type(i + 1);
            }

            // find correct overloaded method (if any)
            if (!MethodOverloads.TryGetValue(methodName, out List<MethodInfo> availableOverloads))
            {
                Debug.LogErrorFormat("There is no method registered to '{0}'!? This should never happen!", methodName);
                return 0;
            }

            MethodInfo method = null;
            foreach (MethodInfo i in availableOverloads)
            {
                if (i.GetParameters().Length == expectedParams)
                {

                    method = i;
                    break;
                }
            }
            if (method == null)
            {
                foreach (MethodInfo i in availableOverloads)
                {
                    ParameterInfo[] p = i.GetParameters();
                    if (p.Length == 1 && p[0].ParameterType == typeof(object[]))
                    {
                        method = i;
                        break;
                    }
                }
            }
            if (method == null)
            {
                Debug.LogErrorFormat("There is no overloaded method '{0}' registered with {1} parameters (what lua expected)!", methodName, expectedParams);
                return 0;
            }

            ParameterInfo[] parameters = method.GetParameters();
            object[] invokeParams = new object[parameters.Length];

            // Special case: dynamic parameters
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object[]))
            {
                object[] dynParams = new object[expectedParams];
                for (int i = 0; i < expectedParams; ++i)
                {
                    dynParams[i] = ToValue(i + 1);
                }
                invokeParams[0] = dynParams;
            }
            else
            {
                if (expectedParams != parameters.Length)
                {
                    Debug.LogErrorFormat("Lua expects {0} parameters, but '{1}' got {2}", expectedParams, methodName, parameters.Length);
                    return 0;
                }

                // In-Parameters
                foreach (ParameterInfo pi in parameters)
                {
                    Lua.ValueType funcType = ToLuaType(pi.ParameterType);
                    if (funcType == Lua.ValueType.NONE)
                    {
                        Debug.LogErrorFormat("Unsupported C# parameter type '{0}' in function '{1}'", pi.ParameterType.Name, methodName);
                        continue;
                    }

                    int paramIdx = inStackCount;
                    int luaIdx = ++inStackCount;

                    Lua.ValueType luaType = expectedParamTypes[paramIdx];
                    invokeParams[paramIdx] = ToValue(luaIdx);

                    if (luaType == Lua.ValueType.NUMBER)
                    {
                        // also support other number parameters, such as int, long etc
                        invokeParams[paramIdx] = Convert.ChangeType(invokeParams[paramIdx], pi.ParameterType);
                    }
                }
            }

            object outParam = method.Invoke(null, invokeParams);

            // Out-Parameter
            if (method.ReturnType != typeof(void))
            {
                // Lua supports multiple return values. In C# we use tuples to mirror that
                if (IsTuple(method.ReturnType))
                {
                    FieldInfo[] fields = method.ReturnType.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (!HandleOutParam(l, ref outStackCount, field.FieldType, field.GetValue(outParam)))
                        {
                            Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", method.ReturnType.Name, methodName);
                            return 0;
                        }
                    }
                }
                else
                {
                    if (!HandleOutParam(l, ref outStackCount, method.ReturnType, outParam))
                    {
                        Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", method.ReturnType.Name, methodName);
                        return 0;
                    }
                }
            }

            return outStackCount;
        };

        L.Register(methodName, fn);
        //Debug.LogFormat("Registered Lua function '{0}'", methodName);
        return true;
    }

    static bool HandleOutParam(Lua l, ref int stackCounter, Type outParamType, object outParam)
    {
        int prev = stackCounter;
        stackCounter += PushValue(l, outParam, outParamType);
        return stackCounter != prev;
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

    bool Check(Lua.ErrorCode error)
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
        }
        return error == Lua.ErrorCode.NONE;
    }

    static void LogWarn(object[] msg)
    {
        Debug.LogWarning(string.Join("", msg));
    }
}
