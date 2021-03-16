using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public TextListBox MapList;

    LuaRuntime RT { get { return GameRuntime.GetLuaRuntime(); } }

    public void StartGeonosis()
    {
        GameRuntime.Instance.EnterMap("geo1c_con");
    }

    public void StartMustafar()
    {
        GameRuntime.Instance.EnterMap("mus1c_con");
    }

    public void StartCorouscant()
    {
        GameRuntime.Instance.EnterMap("cor1c_con");
    }



    void Start()
    {
        Debug.Assert(MapList != null);

        bool bForMP = false;
        RT.CallLuaFunction("missionlist_ExpandMaplist", 0, bForMP);
        LuaRuntime.Table spMissions = RT.GetTable("missionselect_listbox_contents");

        foreach (KeyValuePair<object, object> entry in spMissions)
        {
            LuaRuntime.Table map = entry.Value as LuaRuntime.Table;
            string mapluafile = map.Get<string>("mapluafile");

            object[] res = RT.CallLuaFunction("missionlist_GetLocalizedMapName", 2, false, true, mapluafile);

            string mapName = res[0] as string;
            bool bIsModLevel = map.Get<bool>("isModLevel");

            MapList.AddItem(mapName, bIsModLevel);
        }

        //string mapluafile = spMissions.Get(1, "mapluafile") as string;
        //LuaRuntime.Table modes = RT.CallLuaFunction("missionlist_ExpandModelist", 1, mapluafile)[0] as LuaRuntime.Table;

        //Debug.Log(spMissions.Get(1, "mapluafile"));
    }
}
