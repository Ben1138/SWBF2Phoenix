using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LuaEditor : EditorWindow
{
    // every X seconds, save the edited lua code
    const float SAVE_INTERVALL = 2.0f;
    const string SAVE_CODE_KEY = "SWBF2RuntimeLuaEditorCode";
    float SaveCounter = 0;

    string LuaCode;
    
    [MenuItem("Lua/Editor")]
    public static void OpenLuaEditor()
    {
        LuaEditor window = GetWindow<LuaEditor>();
        window.Show();
    }

    void Awake()
    {
        LuaCode = PlayerPrefs.GetString(SAVE_CODE_KEY);
    }

    void Update()
    {
        SaveCounter += Time.deltaTime;
        if (SaveCounter >= SAVE_INTERVALL)
        {
            PlayerPrefs.SetString(SAVE_CODE_KEY, LuaCode);
            SaveCounter = 0.0f;
        }
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
            luaStackStr += string.Format("[{0}] {1}\t{2}\n", i, typeStr, valStr);
        }
        GUILayout.TextArea(luaStackStr, GUILayout.Width(400), GUILayout.ExpandHeight(true));
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Execute"))
        {
            runtime.ExecuteString(LuaCode);
        }
    }
}
