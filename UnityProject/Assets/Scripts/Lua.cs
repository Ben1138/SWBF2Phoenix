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

	const int LUA_REGISTRYINDEX = -10000;
	const int LUA_GLOBALSINDEX = -10001;
	readonly lua_State_ptr L;

	public Lua()
	{
		L = LuaWrapper.lua_open();
	}

	~Lua()
	{
		LuaWrapper.lua_close(L);
	}


	public LuaWrapper.lua_CFunction atpanic(LuaWrapper.lua_CFunction panicf)
	{
		return LuaWrapper.lua_atpanic(L, panicf);
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
	public LuaWrapper.lua_CFunction ToCFunction(int idx)
	{
		return LuaWrapper.lua_tocfunction(L, idx);
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

	public void PushNil(lua_State_ptr L)
	{
		LuaWrapper.lua_pushnil(L);
	}
	public void PushNumber(float n)
	{
		LuaWrapper.lua_pushnumber(L, n);
	}
	//public void PushLString(string s, ulong l)
	//{
	//	char_ptr str = Marshal.StringToHGlobalAnsi(s);
	//	LuaWrapper.lua_pushlstring(L, str, l);
	//	Marshal.FreeHGlobal(str);
	//}    
	public void PushString(string s)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(s);
		LuaWrapper.lua_pushstring(L, str);
		Marshal.FreeHGlobal(str);
	}
	public void PushCClosure(LuaWrapper.lua_CFunction fn, int n)
	{
		LuaWrapper.lua_pushcclosure(L, fn, n);
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
	public ErrorCode CPCall(LuaWrapper.lua_CFunction func, void_ptr ud)
	{
		return (ErrorCode)LuaWrapper.lua_cpcall(L, func, ud);
	}
	public ErrorCode Load(LuaWrapper.lua_Chunkreader reader, void_ptr dt, string chunkname)
	{
		char_ptr str = Marshal.StringToHGlobalAnsi(chunkname);
		int res = LuaWrapper.lua_load(L, reader, dt, str);
		Marshal.FreeHGlobal(str);
		return (ErrorCode)res;
	}

	public int Dump(LuaWrapper.lua_Chunkwriter writer, void_ptr data)
	{
		return LuaWrapper.lua_dump(L, writer, data);
	}

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

	public int SetHook(LuaWrapper.lua_Hook func, HookMask mask, int count)
	{
		return LuaWrapper.lua_sethook(L, func, (int)mask, count);
	}
	public LuaWrapper.lua_Hook GetHook()
	{
		return LuaWrapper.lua_gethook(L);
	}
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
	public void Register(string n, LuaWrapper.lua_CFunction f)
	{
		PushString(n);
		PushCFunction(f);
		SetTable(LUA_GLOBALSINDEX);
	}
	public void PushCFunction(LuaWrapper.lua_CFunction f) => PushCClosure(f, 0);
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
	
}