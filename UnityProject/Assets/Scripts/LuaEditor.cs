using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LuaEditor : EditorWindow
{
    string LuaCode;
    
    [MenuItem("Lua/Editor")]
    public static void OpenLuaEditor()
    {
        LuaEditor window = GetWindow<LuaEditor>();
        window.Show();
    }

    

    void OnGUI()
    {
        LuaRuntime runtime = GameRuntime.GetLuaRuntime();
        if (!Application.isPlaying || runtime == null)
        {
            EditorGUILayout.LabelField("LUA is not running");
            return;
        }
        Lua L = runtime.GetLua();

        GUILayout.BeginHorizontal();
        LuaCode = GUILayout.TextArea(LuaCode, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        string luaStackStr = "";
        int stackSize = L.GetTop();
        for (int i = 0; i < stackSize; ++i)
        {
            (string typeStr, string valStr) = runtime.LuaValueToStr(i);
            luaStackStr += typeStr + '\t' + valStr + '\n';
        }
        GUILayout.TextArea(luaStackStr, GUILayout.Width(200), GUILayout.ExpandHeight(true));
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Execute"))
        {
            runtime.ExecuteString(LuaCode);
        }
    }
}
