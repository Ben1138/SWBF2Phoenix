using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using lua_State_ptr = System.IntPtr;
using lua_Debug_ptr = System.IntPtr;
using char_ptr = System.IntPtr;
using void_ptr = System.IntPtr;
using size_t = System.UInt64;
using luaL_reg_ptr = System.IntPtr;
using luaL_Buffer_ptr = System.IntPtr;

public class Lua
{
	/* 
	** ===============================================================
	** lua.h
	** ===============================================================
	*/

	public delegate int Function(Lua L);
	//public delegate byte[] Chunkreader(Lua L, void_ptr ud, out size_t sz);
	//public delegate int Chunkwriter(Lua L, void_ptr p, size_t sz, void_ptr ud);
	public delegate void Hook(Lua L, out Debug ar);

	public enum ValueType : int
	{
		NONE = -1,
		NIL = 0,
		BOOLEAN = 1,
		LIGHTUSERDATA = 2,
		NUMBER = 3,
		STRING = 4,
		TABLE = 5,
		FUNCTION = 6,
		USERDATA = 7,
		THREAD = 8
	}

	public enum ErrorCode : int
	{
		NONE = 0,
		RUN = 1,
		FILE = 2,
		SYNTAX = 3,
		MEM = 4,
		ERR = 5
	}

	public enum EventCode : int
	{
		CALL = 0,
		RET = 1,
		LINE = 2,
		COUNT = 3,
		TAILRET = 4
	}

	[System.Flags]
	public enum HookMask : int
	{
		DISABLED = 0,
		CALL = 1 << EventCode.CALL,
		RET = 1 << EventCode.RET,
		LINE = 1 << EventCode.LINE,
		COUNT = 1 << EventCode.COUNT,
	}

	const int LUA_IDSIZE = 60;
	const int LUA_REGISTRYINDEX = -10000;
	const int LUA_GLOBALSINDEX = -10001;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Debug
	{
		int Event;
		string Name;
		string Namewhat;
		string What;
		string Source;
		int CurrentLine;
		int Nups;
		int LineDefined;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = LUA_IDSIZE)]
		byte[] Short_src;

		int I_ci;
	}

	readonly lua_State_ptr L;
	static Dictionary<lua_State_ptr, Lua> LuaInstances = new Dictionary<lua_State_ptr, Lua>();
	static List<LuaWrapper.lua_CFunction> LuaFunctions = new List<LuaWrapper.lua_CFunction>();

	public Lua()
	{
		L = LuaWrapper.lua_open();
		LuaInstances.Add(L, this);
	}

	~Lua()
	{
		LuaWrapper.lua_close(L);
		LuaInstances.Remove(L);
	}

	LuaWrapper.lua_CFunction CB_Function(Function fn)
	{
		LuaWrapper.lua_CFunction cb = (lua_State_ptr L) =>
		{
			return fn(LuaInstances[L]);
		};
		LuaFunctions.Add(cb);
		return cb;
	}

	Function CB_Function(LuaWrapper.lua_CFunction fn)
	{
		return (Lua L) =>
		{
			return fn(L.L);
		};
	}

	//LuaWrapper.lua_Chunkreader CB_Chunkreader(Chunkreader fn)
	//{
	//	return (lua_State_ptr L, void_ptr ud, out size_t sz) =>
	//	{
	//		byte[] chunk = fn(LuaInstances[L], ud, out sz);
	//		return fn(LuaInstances[L], ud, out sz);
	//	};
	//}

	LuaWrapper.lua_Hook CB_Hook(Hook fn)
	{
		return (lua_State_ptr L, lua_Debug_ptr ar) => 
		{
			Debug d = Marshal.PtrToStructure<Debug>(ar);
			fn(LuaInstances[L], out d); 
		};
	}

	//Hook CB_Hook(LuaWrapper.lua_Hook fn)
	//{
	//	return (Lua L, lua_Debug_ptr ar) =>
	//	{
	//		fn(L.L, ar);
	//	};
	//}

	public void AtPanic(Function panicf)
	{
		LuaWrapper.lua_atpanic(L, CB_Function(panicf));
	}

	/*
    ** basic stack manipulation
    */

	public int GetTop()
	{
		return LuaWrapper.lua_gettop(L);
	}
	public void SetTop(int idx)
	{
		LuaWrapper.lua_settop(L, idx);
	}
	public void PushValue(int idx)
	{
		LuaWrapper.lua_pushvalue(L, idx);
	}
	public void Remove(int idx)
	{
		LuaWrapper.lua_remove(L, idx);
	}
	public void Insert(int idx)
	{
		LuaWrapper.lua_insert(L, idx);
	}
	public void Replace(int idx)
	{
		LuaWrapper.lua_replace(L, idx);
	}
	public bool CheckStack(int sz)
	{
		return LuaWrapper.lua_checkstack(L, sz) != 0;
	}

	public void XMove(Lua other, int n)
	{
		LuaWrapper.lua_xmove(L, other.L, n);
	}

	/*
    ** access functions (stack -> C)
    */


	public bool IsNumber(int idx)
	{
		return LuaWrapper.lua_isnumber(L, idx) != 0;
	}
	public bool IsString(int idx)
	{
		return LuaWrapper.lua_isstring(L, idx) != 0;
	}
	public bool IsCFunction(int idx)
	{
		return LuaWrapper.lua_iscfunction(L, idx) != 0;
	}
	public bool IsUserData(int idx)
	{
		return LuaWrapper.lua_isuserdata(L, idx) != 0;
	}
	public ValueType Type(int idx)
	{
		return (ValueType)LuaWrapper.lua_type(L, idx);
	}
	public string TypeName(int tp)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_typename(L, tp));
	}

	public bool Equal(int idx1, int idx2)
	{
		return LuaWrapper.lua_equal(L, idx1, idx2) != 0;
	}
	public bool RawEqual(int idx1, int idx2)
	{
		return LuaWrapper.lua_rawequal(L, idx1, idx2) != 0;
	}
	public bool LessThan(int idx1, int idx2)
	{
		return LuaWrapper.lua_lessthan(L, idx1, idx2) != 0;
	}

	public float ToNumber(int idx)
	{
		return LuaWrapper.lua_tonumber(L, idx);
	}
	public bool ToBoolean(int idx)
	{
		return LuaWrapper.lua_toboolean(L, idx) != 0;
	}
	public string ToString(int idx)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_tostring(L, idx));
	}
	public ulong StrLen(int idx)
	{
		return LuaWrapper.lua_strlen(L, idx);
	}
	public Function ToFunction(int idx)
	{
		return CB_Function(LuaWrapper.lua_tocfunction(L, idx));
	}
	public void_ptr ToUserData(int idx)
	{
		return LuaWrapper.lua_touserdata(L, idx);
	}
	public lua_State_ptr ToThread(int idx)
	{
		return LuaWrapper.lua_tothread(L, idx);
	}
	public void_ptr ToPointer(int idx)
	{
		return LuaWrapper.lua_topointer(L, idx);
	}

	/*
    ** push functions (C -> stack)
    */

	public void PushNil()
	{
		LuaWrapper.lua_pushnil(L);
	}
	public void PushNumber(float n)
	{
		LuaWrapper.lua_pushnumber(L, n);
	}
	public void PushLString(byte[] str)
	{
		IntPtr p = Marshal.AllocHGlobal(str.Length);
		LuaWrapper.lua_pushlstring(L, p, (ulong)str.Length);
		Marshal.FreeHGlobal(p);
	}
	public void PushString(string s)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(s);
		LuaWrapper.lua_pushstring(L, str);
		Marshal.FreeHGlobal(str);
	}
	public void PushCClosure(Function fn, int n)
	{
		LuaWrapper.lua_pushcclosure(L, CB_Function(fn), n);
	}
	public void PushBoolean(bool b)
	{
		LuaWrapper.lua_pushboolean(L, b ? 1 : 0);
	}
	//public void pushlightuserdata(byte[] data)
	//{
	//	void_ptr p = Marshal.AllocHGlobal(data.Length);
	//	Marshal.Copy(p, 0, )
	//	LuaWrapper.lua_pushlightuserdata(L, p);
	//}

	/*
    ** get functions (Lua -> stack)
    */

	public void GetTable(int idx)
	{
		LuaWrapper.lua_gettable(L, idx);
	}
	public void RawGet(int idx)
	{
		LuaWrapper.lua_rawget(L, idx);
	}
	public void RawGetI(int idx, int n)
	{
		LuaWrapper.lua_rawgeti(L, idx, n);
	}
	public void NewTable()
	{
		LuaWrapper.lua_newtable(L);
	}
	public void_ptr NewUserData(size_t sz)
	{
		return LuaWrapper.lua_newuserdata(L, sz);
	}
	public bool GetMetaTable(int objindex)
	{
		return LuaWrapper.lua_getmetatable(L, objindex) != 0;
	}
	public void GetFenv(int idx)
	{
		LuaWrapper.lua_getfenv(L, idx);
	}

	/*
    ** set functions (stack -> Lua)
    */

	public void SetTable(int idx)
	{
		LuaWrapper.lua_settable(L, idx);
	}
	public void RawSet(int idx)
	{
		LuaWrapper.lua_rawset(L, idx);
	}
	public void RawSetI(int idx, int n)
	{
		LuaWrapper.lua_rawseti(L, idx, n);
	}
	public bool SetMetaTable(int objindex)
	{
		return LuaWrapper.lua_setmetatable(L, objindex) != 0;
	}
	public bool SetFenv(int idx)
	{
		return LuaWrapper.lua_setfenv(L, idx) != 0;
	}

	/*
    ** `load' and `call' functions (load and run Lua code)
    */

	public void Call(int nargs, int nresults)
	{
		LuaWrapper.lua_call(L, nargs, nresults);
	}
	public ErrorCode PCall(int nargs, int nresults, int errfunc)
	{
		return (ErrorCode)LuaWrapper.lua_pcall(L, nargs, nresults, errfunc);
	}
	public ErrorCode CPCall(Function func, void_ptr ud)
	{
		return (ErrorCode)LuaWrapper.lua_cpcall(L, CB_Function(func), ud);
	}
	//public ErrorCode Load(Chunkreader reader, void_ptr dt, string chunkname)
	//{
	//	char_ptr str = Marshal.StringToHGlobalAnsi(chunkname);
	//	int res = LuaWrapper.lua_load(L, reader, dt, str);
	//	Marshal.FreeHGlobal(str);
	//	return (ErrorCode)res;
	//}

	//public int Dump(Chunkwriter writer, void_ptr data)
	//{
	//	return LuaWrapper.lua_dump(L, writer, data);
	//}

	/*
    ** coroutine functions
    */

	public ErrorCode Yield(int nresults)
	{
		return (ErrorCode)LuaWrapper.lua_yield(L, nresults);
	}
	public ErrorCode Resume(int narg)
	{
		return (ErrorCode)LuaWrapper.lua_resume(L, narg);
	}
	/*
    ** garbage-collection functions
    */

	public int GetGCThreshold()
	{
		return LuaWrapper.lua_getgcthreshold(L);
	}
	public int GetGCCount()
	{
		return LuaWrapper.lua_getgccount(L);
	}
	public void SetGCThreshold(int newthreshold)
	{
		LuaWrapper.lua_setgcthreshold(L, newthreshold);
	}
	/*
    ** miscellaneous functions
    */


	public static string Version()
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_version());
	}

	public int Error()
	{
		return LuaWrapper.lua_error(L);
	}

	public bool Next(int idx)
	{
		return LuaWrapper.lua_next(L, idx) != 0;
	}

	public void Concat(int n)
	{
		LuaWrapper.lua_concat(L, n);
	}

	public int PushUpValues()
	{
		return LuaWrapper.lua_pushupvalues(L);
	}

	public bool GetStack(int level, lua_Debug_ptr ar)
	{
		return LuaWrapper.lua_getstack(L, level, ar) != 0;
	}
	public bool GetInfo(string what, lua_Debug_ptr ar)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(what);
		int res = LuaWrapper.lua_getinfo(L, str, ar);
		Marshal.FreeHGlobal(str);
		return res != 0;
	}
	public string GetLocal(lua_Debug_ptr ar, int n)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_getlocal(L, ar, n));
	}
	public string SetLocal(lua_Debug_ptr ar, int n)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_setlocal(L, ar, n));
	}
	public string GetUpValue(int funcindex, int n)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_getupvalue(L, funcindex, n));
	}
	public string SetUpValue(int funcindex, int n)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.lua_setupvalue(L, funcindex, n));
	}

	public int SetHook(Hook func, HookMask mask, int count)
	{
		return LuaWrapper.lua_sethook(L, CB_Hook(func), (int)mask, count);
	}
	//public Hook GetHook()
	//{
	//	return CB_Hook(LuaWrapper.lua_gethook(L));
	//}
	public HookMask GetHookMask()
	{
		return (HookMask)LuaWrapper.lua_gethookmask(L);
	}
	public int GetHookCount()
	{
		return LuaWrapper.lua_gethookcount(L);
	}

	/* 
	** ===============================================================
	** some useful macros
	** ===============================================================
	*/

	public void Pop(int n) => SetTop(-(n) - 1);
	public void Register(string n, Function f)
	{
		PushString(n);
		PushFunction(f);
		SetTable(LUA_GLOBALSINDEX);
	}
	public void PushFunction(Function f) => PushCClosure(f, 0);
	public bool IsFunction(int n) => Type(n) == ValueType.FUNCTION;
	public bool IsTable(int n) => Type(n) == ValueType.TABLE;
	public bool IsLightUserData(int n) => Type(n) == ValueType.LIGHTUSERDATA;
	public bool IsNil(int n) => Type(n) == ValueType.NIL;
	public bool IsBoolean(int n) => Type(n) == ValueType.BOOLEAN;
	public bool IsNone(int n) => Type(n) == ValueType.NONE;
	public bool IsNoneOrNil(int n) => Type(n) <= 0;


	/* 
	** ===============================================================
	** lualib.h
	** ===============================================================
	*/

	public int OpenBase()
	{
		return LuaWrapper.luaopen_base(L);
	}
	public int OpenTable()
	{
		return LuaWrapper.luaopen_table(L);
	}
	public int OpenIO()
	{
		return LuaWrapper.luaopen_io(L);
	}
	public int OpenString()
	{
		return LuaWrapper.luaopen_string(L);
	}
	public int OpenMath()
	{
		return LuaWrapper.luaopen_math(L);
	}
	public int OpenDebug()
	{
		return LuaWrapper.luaopen_debug(L);
	}
	public int OpenLoadLib()
	{
		return LuaWrapper.luaopen_loadlib(L);
	}


	/* 
	** ===============================================================
	** lauxlib.h
	** ===============================================================
	*/

	public void OpenLib(string libname, luaL_reg_ptr l, int nup)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(libname);
		LuaWrapper.luaL_openlib(L, str, l, nup);
		Marshal.FreeHGlobal(str);
	}
	public int GetMetaField(int obj, string e)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(e);
		int res = LuaWrapper.luaL_getmetafield(L, obj, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public int CallMeta(int obj, string e)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(e);
		int res = LuaWrapper.luaL_callmeta(L, obj, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public int TypError(int narg, string tname)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(tname);
		int res = LuaWrapper.luaL_typerror(L, narg, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public int ArgError(int numarg, string extramsg)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(extramsg);
		int res = LuaWrapper.luaL_argerror(L, numarg, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public string CheckLString(int numArg, out size_t l)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.luaL_checklstring(L, numArg, out l));
	}
	public string OptLString(int numArg, string def, out size_t l)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(def);
		char_ptr res = LuaWrapper.luaL_optlstring(L, numArg, str, out l);
		Marshal.FreeHGlobal(str);
		return Marshal.PtrToStringAnsi(res);
	}
	public float CheckNumber(int numArg)
	{
		return LuaWrapper.luaL_checknumber(L, numArg);
	}
	public float OptNumber(int nArg, float def)
	{
		return LuaWrapper.luaL_optnumber(L, nArg, def);
	}

	public void CheckStack(int sz, string msg)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(msg);
		LuaWrapper.luaL_checkstack(L, sz, str);
		Marshal.FreeHGlobal(str);
	}
	public void CheckType(int narg, int t)
	{
		LuaWrapper.luaL_checktype(L, narg, t);
	}
	public void CheckAny(int narg)
	{
		LuaWrapper.luaL_checkany(L, narg);
	}

	public int NewMetaTable(string tname)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(tname);
		int res = LuaWrapper.luaL_newmetatable(L, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public void GetMetaTable(string tname)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(tname);
		LuaWrapper.luaL_getmetatable(L, str);
		Marshal.FreeHGlobal(str);
	}
	public void_ptr CheckUData(int ud, string tname)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(tname);
		void_ptr res = LuaWrapper.luaL_checkudata(L, ud, str);
		Marshal.FreeHGlobal(str);
		return res;
	}

	public void Where(int lvl)
	{
		LuaWrapper.luaL_where(L, lvl);
	}

	public int Ref(int t)
	{
		return LuaWrapper.luaL_ref(L, t);
	}
	public void Unref(int t, int reference)
	{
		LuaWrapper.luaL_unref(L, t, reference);
	}

	public int GetN(int t)
	{
		return LuaWrapper.luaL_getn(L, t);
	}
	public void SetN(int t, int n)
	{
		LuaWrapper.luaL_setn(L, t, n);
	}

	public int LoadFile(string filename)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(filename);
		int res = LuaWrapper.luaL_loadfile(L, str);
		Marshal.FreeHGlobal(str);
		return res;
	}
	public int LoadBuffer(void_ptr buff, size_t sz, string name)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(name);
		int res = LuaWrapper.luaL_loadbuffer(L, buff, sz, str);
		Marshal.FreeHGlobal(str);
		return res;
	}

	public void BuffInit(luaL_Buffer_ptr B)
	{
		LuaWrapper.luaL_buffinit(L, B);
	}
	public static string PrepBuffer(luaL_Buffer_ptr B)
	{
		return Marshal.PtrToStringAnsi(LuaWrapper.luaL_prepbuffer(B));
	}
	//public static void luaL_addlstring(luaL_Buffer_ptr B, char_ptr s, size_t l)
	//{
	//	LuaWrapper.luaL_addlstring(B, s, l);
	//}
	public static void AddString(luaL_Buffer_ptr B, string s)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(s);
		LuaWrapper.luaL_addstring(B, str);
		Marshal.FreeHGlobal(str);
	}
	public static void AddValue(luaL_Buffer_ptr B)
	{
		LuaWrapper.luaL_addvalue(B);
	}
	public static void PushResult(luaL_Buffer_ptr B)
	{
		LuaWrapper.luaL_pushresult(B);
	}

	public ErrorCode DoFile(string filename)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(filename);
		int res = LuaWrapper.lua_dofile(L, str);
		Marshal.FreeHGlobal(str);
		return (ErrorCode)res;
	}
	public ErrorCode DoString(string str)
	{
		char_ptr s = Marshal.StringToHGlobalAnsi(str);
		int res = LuaWrapper.lua_dostring(L, s);
		Marshal.FreeHGlobal(s);
		return (ErrorCode)res;
	}
	public ErrorCode DoBuffer(IntPtr buff, ulong sz, string n)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(n);
		int res = LuaWrapper.lua_dobuffer(L, buff, sz, str);
		Marshal.FreeHGlobal(str);
		return (ErrorCode)res;
	}

	/*
	** ===============================================================
	** some useful macros
	** ===============================================================
	*/

	public void ArgCheck(bool cond, int numarg, string extramsg) 
	{
		if (!cond)
		{
			ArgError(numarg, extramsg);
		}
	}
	public string CheckString(int n) => CheckLString(n, out ulong _);
	public string OptString(int n, string d) => OptLString(n, d, out _);
	public int CheckInt(int n) => (int)CheckNumber(n);
	public long CheckLong(int n) => (long)CheckNumber(n);
	public int OptInt(int n, float d) => (int)OptNumber(n, d);
	public long OptLong(int n, float d) => (long)OptNumber(n, d);
}