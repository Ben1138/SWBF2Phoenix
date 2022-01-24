using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Don't permanently store references of these! 
public class PhxLuaRuntime
{
    // References a function defined in Lua (not a CFunction!)
    public class LFunction
    {
        static Lua L => PhxGame.GetLuaRuntime()?.GetLua();

        int RefIdx;

        LFunction() { }

        ~LFunction()
        {
            if (L != null)
            {
                L.Unref(Lua.LUA_REGISTRYINDEX, RefIdx);
            }
        }

        // Will fail if the top of the Lua stack is not a function!
        internal static LFunction FromStack()
        {
            if (L == null || !L.IsFunction(-1))
            {
                return null;
            }

            LFunction fn = new LFunction();

            // Ref will pop the top of the Lua stack (must be a function) and put it
            // into a lua registry, returning a reference index within that table
            fn.RefIdx = L.Ref(Lua.LUA_REGISTRYINDEX);

            // since we want to preserve the stack "as is", let's push the function back on again
            L.RawGetI(Lua.LUA_REGISTRYINDEX, fn.RefIdx);

            return fn;
        }

        public void Invoke(params object[] args)
        {
            PhxLuaRuntime rt = PhxGame.GetLuaRuntime();
            if (rt != null)
            {
                rt.CallLuaFunction(RefIdx, 0, false, false, args);
            }
        }

        public override string ToString()
        {
            return "LFunction" + RefIdx;
        }
    }

    public class Table : IEnumerable<KeyValuePair<object, object>>
    {
        public int Count => Contents.Count;

        static Lua L => PhxGame.GetLuaRuntime()?.GetLua();
        Dictionary<object, object> Contents = new Dictionary<object, object>();

        Table() { }


        internal static Table FromStack(int idx)
        {
            if (L == null || !L.IsTable(-1))
            {
                return null;
            }

            Table t = new Table();

            // push the key where to start traversal.
            // when pushing nil, we start at the beginning
            L.PushNil();

            // consider relative indices
            if (idx < 0) idx--;

            while (L.Next(idx))
            {
                // if Next() was successful, it pushed the next key value pair onto the stack
                // -1 = value
                // -2 = key

                object key = ToValue(L, -2);
                if (key == null)
                {
                    throw new Exception("Key was null during lua table traversal! This should never happen!");
                }
                t.Contents.Add(key, ToValue(L, -1));

                // pop value, keep key to continue traversal at that key
                L.Pop(1);
            }

            return t;
        }

        public object Get(params object[] path)
        {
            return Get(path, 0);
        }

        public T Get<T>(params object[] path)
        {
            object res = Get(path);
            if (res == null) return default;
            return (T)Convert.ChangeType(res, typeof(T), CultureInfo.InvariantCulture);
        }

        object Get(object[] path, int startIdx)
        {
            if (path[startIdx].GetType() == typeof(int))
            {
                // lua only knows floats as numbers
                path[startIdx] = Convert.ChangeType(path[startIdx], typeof(float), CultureInfo.InvariantCulture);
            }

            if (Contents.TryGetValue(path[startIdx], out object value))
            {
                if (value.GetType() == typeof(Table))
                {
                    return ((Table)value).Get(path, startIdx + 1);
                }
                return value;
            }
            return null;
        }

        public override string ToString()
        {
            return string.Format($"[{Contents.Count}]");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            return Contents.GetEnumerator();
        }
    }

    Lua L;

    // Store all MethodInfos for each occouring method name in order to be able to handle overloaded methods
    Dictionary<string, List<MethodInfo>> MethodOverloads = new Dictionary<string, List<MethodInfo>>();


    public PhxLuaRuntime()
    {
        L = new Lua();
        L.OnError += (string msg) => Debug.LogError("[LUA] " + msg);
        L.AtPanic(Panic);
        L.OpenBase();
        L.OpenMath();
        L.OpenString();
        L.OpenTable();
        Register<Action<object[]>>(printf);
        RegisterLuaFunctions(typeof(PhxLuaAPI));

        PhxGame inst = PhxGame.Instance;
        Debug.Assert(inst != null);
        L.PushString(inst.VersionString);
        L.SetGlobal("PHX_VER");
        L.PushNumber(inst.VersionMajor);
        L.SetGlobal("PHX_VER_MAJOR");
        L.PushNumber(inst.VersionMinor);
        L.SetGlobal("PHX_VER_MINOR");
        L.PushNumber(inst.VersionPatch);
        L.SetGlobal("PHX_VER_PATCH");
    }

    ~PhxLuaRuntime()
    {
        Close();
    }

    public void Close()
    {
        if (L != null)
        {
            L.Close();
            L = null;
            MethodOverloads.Clear();
        }
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
        else if (T == typeof(int?))
        {
            return Lua.ValueType.LIGHTUSERDATA;
        }
        else if (T == typeof(string))
        {
            return Lua.ValueType.STRING;
        }
        else if (T == typeof(bool))
        {
            return Lua.ValueType.BOOLEAN;
        }
        else if (T == typeof(LFunction))
        {
            return Lua.ValueType.FUNCTION;
        }
        else if (T == typeof(object))
        {
            return Lua.ValueType.NIL;
        }

        return Lua.ValueType.NONE;
    }

    public object ToValue(int idx, bool bIsUnicode = false)
    {
        return ToValue(L, idx, bIsUnicode);
    }

    public static object ToValue(Lua l, int idx, bool bIsUnicode=false)
    {
        Lua.ValueType type = l.Type(idx);
        int top = l.GetTop();
        object res;
        switch (type)
        {
            case Lua.ValueType.NIL:
                res = null;
                break;
            case Lua.ValueType.NUMBER:
                res = l.ToNumber(idx);
                break;
            case Lua.ValueType.LIGHTUSERDATA:
                res = (int?)l.ToUserData(idx);
                break;
            case Lua.ValueType.STRING:
                res = bIsUnicode ? l.ToStringUnicode(idx) : l.ToString(idx);
                break;
            case Lua.ValueType.BOOLEAN:
                res = l.ToBoolean(idx);
                break;
            case Lua.ValueType.FUNCTION:
                if (idx == -1 || idx == l.GetTop())
                {
                    res = LFunction.FromStack();
                }
                else
                {
                    res = "LFunction";
                }
                break;
            case Lua.ValueType.TABLE:
                res = Table.FromStack(idx);
                break;
            default:
                Debug.LogErrorFormat("Cannot convert Lua Type '{0}' to C# equivalent!", type.ToString());
                return null;
        }
        
        // Debug only!
        int top2 = l.GetTop();
        Lua.ValueType type2 = l.Type(idx);
        if (top != top2 || type != type2)
        {
            Debug.LogError("Somewhere, something went terribly wrong...");
        }

        return res;
    }

    public int PushValue<T>(T value)
    {
        return PushValue(L, value);
    }

    public int PushValues(object[] values, bool bIsUnicode = false)
    {
        int numPushed = 0;
        for (int i = 0; i < values.Length; ++i)
        {
            numPushed += PushValue(L, values[i], values[i].GetType(), bIsUnicode);
        }
        return numPushed;
    }

    public static int PushValue<T>(Lua l, T value, bool bIsUnicode = false)
    {
        return PushValue(l, value, typeof(T), bIsUnicode);
    }

    public static int PushValue(Lua l, object value, Type T, bool bIsUnicode=false)
    {
        if (value == null)
        {
            l.PushNil();
            return 1;
        }

        Lua.ValueType type = ToLuaType(T);
        if (type == Lua.ValueType.NUMBER)
        {
            l.PushNumber(Convert.ToSingle(value));
            return 1;
        }
        else if (type == Lua.ValueType.LIGHTUSERDATA)
        {
            l.PushLightUserData((IntPtr)Convert.ToInt32(value));
            return 1;
        }
        else if (type == Lua.ValueType.STRING)
        {
            if (bIsUnicode)
            {
                l.PushStringUnicode(Convert.ToString(value));
            }
            else
            {
                l.PushString(Convert.ToString(value));
            }
            return 1;
        }
        else if (type == Lua.ValueType.BOOLEAN)
        {
            l.PushBoolean(Convert.ToBoolean(value));
            return 1;
        }
        else if (type == Lua.ValueType.FUNCTION)
        {
            l.PushFunction((Lua.CFunction)value);
            return 1;
        }

        Debug.LogErrorFormat("Cannot not push C# value of type '{0}' to lua!", T.Name);
        return 0;
    }

    public Table GetTable(string name)
    {
        return GetTable(L, name);
    }

    public static Table GetTable(Lua l, string name)
    {
        l.GetGlobal(name);
        if (l.IsTable(-1))
        {
            return Table.FromStack(-1);
        }
        return null;
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

    // Returns null on failure. If nResults was zero, the result will be an empty array
    public object[] CallLuaFunction(string fnName, int nResults, params object[] args)
    {
        return CallLuaFunction(fnName, nResults, false, false, args);
    }

    // Returns null on failure. If nResults was zero, the result will be an empty array
    public object[] CallLuaFunction(string fnName, int nResults, bool bUnicodeParams, bool bUnicodeReturn, params object[] args)
    {
        L.GetGlobal(fnName);
        return CallLuaFunctionOnStack(nResults, bUnicodeParams, bUnicodeReturn, args);
    }

    // Returns null on failure. If nResults was zero, the result will be an empty array
    object[] CallLuaFunction(int fnRefIdx, int nResults, bool bUnicodeParams, bool bUnicodeReturn, object[] args)
    {
        // get function from reference index and push it on the stack
        L.RawGetI(Lua.LUA_REGISTRYINDEX, fnRefIdx);
        return CallLuaFunctionOnStack(nResults, bUnicodeParams, bUnicodeReturn, args);
    }

    object[] CallLuaFunctionOnStack(int nResults, bool bUnicodeParams, bool bUnicodeReturn, object[] args)
    {
        object[] res = new object[nResults];

        if (!L.IsFunction(-1))
        {
            Debug.LogErrorFormat("Given LFunction does not point to a lua function but '{0}'!", L.Type(-1).ToString());
            return null;
        }
        PushValues(args, bUnicodeParams);
        if (!Check(L.PCall(args.Length, nResults, 0)))
        {
            // pop error message
            L.Pop(1);
            return null;
        }

        for (int i = 0; i < nResults; ++i)
        {
            res[nResults - 1 - i] = ToValue(-1, bUnicodeReturn);
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
            Debug.LogWarningFormat("Cannot register non-static method '{0}' as Lua function!", methodName);
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

        Lua.CFunction fn = (Lua l) =>
        {
            int outStackCount = 0;

            int expectedParams = l.GetTop();
            Lua.ValueType[] expectedParamTypes = new Lua.ValueType[expectedParams];
            for (int i = 0; i < expectedParams; ++i)
            {
                expectedParamTypes[i] = l.Type(i + 1);
            }

            // find correct overloaded method (if any)
            // right now, overloads are merely identified by parameter count, NOT parameter types!
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
                bool bIsUnicode = parameters[0].GetCustomAttribute<PhxLuaAPI.Unicode>() != null;

                object[] dynParams = new object[expectedParams];
                for (int i = expectedParams - 1; i >= 0; --i)
                {
                    dynParams[i] = ToValue(-1, bIsUnicode);
                    l.Pop(1);
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
                for (int i = parameters.Length - 1; i >= 0; --i)
                {
                    ParameterInfo pi = parameters[i];

                    if (ToLuaType(pi.ParameterType) == Lua.ValueType.NONE)
                    {
                        Debug.LogErrorFormat("Unsupported C# parameter type '{0}' in function '{1}'", pi.ParameterType.Name, methodName);
                        continue;
                    }

                    bool bIsUnicode = pi.GetCustomAttribute<PhxLuaAPI.Unicode>() != null;
                    invokeParams[i] = ToValue(-1, bIsUnicode);
                    l.Pop(1);

                    Lua.ValueType luaType = expectedParamTypes[i];
                    if (luaType == Lua.ValueType.NUMBER)
                    {
                        // special case: nullables (used as pointers in Lua)
                        if (pi.ParameterType == typeof(int?))
                        {
                            invokeParams[i] = new int?((int)Convert.ChangeType(invokeParams[i], typeof(int), CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            // also support other number parameters, such as int, long etc
                            invokeParams[i] = Convert.ChangeType(invokeParams[i], pi.ParameterType, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            
            object outParam = method.Invoke(null, invokeParams);

            // Out-Parameter
            if (method.ReturnType != typeof(void))
            {
                bool bIsUnicode = method.ReturnTypeCustomAttributes.IsDefined(typeof(PhxLuaAPI.Unicode), false);

                // Lua supports multiple return values. In C# we use tuples to mirror that
                if (IsTuple(method.ReturnType))
                {
                    FieldInfo[] fields = method.ReturnType.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (!HandleOutParam(l, ref outStackCount, field.FieldType, field.GetValue(outParam), bIsUnicode))
                        {
                            Debug.LogErrorFormat("Unsupported return type '{0}' in lua function '{1}'", method.ReturnType.Name, methodName);
                            return 0;
                        }
                    }
                }
                else
                {
                    if (!HandleOutParam(l, ref outStackCount, method.ReturnType, outParam, bIsUnicode))
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

    static bool HandleOutParam(Lua l, ref int stackCounter, Type outParamType, object outParam, bool bIsUnicode)
    {
        int prev = stackCounter;
        stackCounter += PushValue(l, outParam, outParamType, bIsUnicode);
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

    static void printf(object[] msg)
    {
        if (msg[0] is string)
        {
            object[] fmt = new object[msg.Length - 1];
            Array.Copy(msg, 1, fmt, 0, fmt.Length);
            Debug.Log(PhxHelpers.Format(msg[0] as string, fmt));
        }
        else
        {
            Debug.Log(string.Join(" ", msg));
        }
    }
}
