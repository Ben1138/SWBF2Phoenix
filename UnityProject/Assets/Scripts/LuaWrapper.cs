using System;
using System.Runtime.InteropServices;

using lua_State_ptr = System.IntPtr;
using lua_Debug_ptr = System.IntPtr;
using lua_Number = System.Single;
using char_ptr = System.IntPtr;
using void_ptr = System.IntPtr;
using size_t = System.UInt64;
using size_t_ptr = System.IntPtr;

public static class LuaWrapper
{
    const string LIB_NAME = "lua50-swbf2-x64.dll";

    public delegate int lua_CFunction(lua_State_ptr L);
    public delegate char_ptr lua_Chunkreader(lua_State_ptr L, void_ptr ud, size_t_ptr sz);
    public delegate int lua_Chunkwriter(lua_State_ptr L, void_ptr p, size_t sz, void_ptr ud);
    public delegate void lua_Hook(lua_State_ptr L, lua_Debug_ptr ar);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_State_ptr lua_open();
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_close(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_State_ptr lua_newthread(lua_State_ptr L);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_CFunction lua_atpanic(lua_State_ptr L, lua_CFunction panicf);


    /*
    ** basic stack manipulation
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_gettop(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_settop(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushvalue(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_remove(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_insert(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_replace(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_checkstack(lua_State_ptr L, int sz);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_xmove(lua_State_ptr from, lua_State_ptr to, int n);


    /*
    ** access functions (stack -> C)
    */

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_isnumber(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_isstring(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_iscfunction(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_isuserdata(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_type(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_typename (lua_State_ptr L, int tp);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_equal(lua_State_ptr L, int idx1, int idx2);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_rawequal(lua_State_ptr L, int idx1, int idx2);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_lessthan(lua_State_ptr L, int idx1, int idx2);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_Number lua_tonumber(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_toboolean(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_tostring (lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern size_t lua_strlen(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_CFunction lua_tocfunction(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void_ptr lua_touserdata(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_State_ptr lua_tothread(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void_ptr lua_topointer (lua_State_ptr L, int idx);


    /*
    ** push functions (C -> stack)
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushnil(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushnumber(lua_State_ptr L, lua_Number n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushlstring(lua_State_ptr L, char_ptr s, size_t l);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushstring(lua_State_ptr L, char_ptr s);
    //[DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	//public static extern char_ptr lua_pushvfstring (lua_State_ptr L, char_ptr fmt, va_list argp);
    //[DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	//public static extern char_ptr lua_pushfstring (lua_State_ptr L, char_ptr fmt, ...);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushcclosure(lua_State_ptr L, lua_CFunction fn, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushboolean(lua_State_ptr L, int b);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_pushlightuserdata(lua_State_ptr L, void_ptr p);


    /*
    ** get functions (Lua -> stack)
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_gettable(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_rawget(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_rawgeti(lua_State_ptr L, int idx, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_newtable(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void_ptr lua_newuserdata(lua_State_ptr L, size_t sz);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_getmetatable(lua_State_ptr L, int objindex);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_getfenv(lua_State_ptr L, int idx);


    /*
    ** set functions (stack -> Lua)
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_settable(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_rawset(lua_State_ptr L, int idx);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_rawseti(lua_State_ptr L, int idx, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_setmetatable(lua_State_ptr L, int objindex);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_setfenv(lua_State_ptr L, int idx);


    /*
    ** `load' and `call' functions (load and run Lua code)
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_call(lua_State_ptr L, int nargs, int nresults);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_pcall(lua_State_ptr L, int nargs, int nresults, int errfunc);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_cpcall(lua_State_ptr L, lua_CFunction func, void_ptr ud);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_load(lua_State_ptr L, lua_Chunkreader reader, void_ptr dt,
                        char_ptr chunkname);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_dump(lua_State_ptr L, lua_Chunkwriter writer, void_ptr data);


    /*
    ** coroutine functions
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_yield(lua_State_ptr L, int nresults);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_resume(lua_State_ptr L, int narg);

    /*
    ** garbage-collection functions
    */
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_getgcthreshold(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_getgccount(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_setgcthreshold(lua_State_ptr L, int newthreshold);

    /*
    ** miscellaneous functions
    */

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_version();

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_error(lua_State_ptr L);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_next(lua_State_ptr L, int idx);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern void lua_concat(lua_State_ptr L, int n);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_pushupvalues(lua_State_ptr L);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_getstack(lua_State_ptr L, int level, lua_Debug_ptr ar);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_getinfo(lua_State_ptr L, char_ptr what, lua_Debug_ptr ar);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_getlocal (lua_State_ptr L, lua_Debug_ptr ar, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_setlocal (lua_State_ptr L, lua_Debug_ptr ar, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_getupvalue (lua_State_ptr L, int funcindex, int n);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern char_ptr lua_setupvalue (lua_State_ptr L, int funcindex, int n);

    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_sethook(lua_State_ptr L, lua_Hook func, int mask, int count);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern lua_Hook lua_gethook(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_gethookmask(lua_State_ptr L);
    [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
	public static extern int lua_gethookcount(lua_State_ptr L);
}
