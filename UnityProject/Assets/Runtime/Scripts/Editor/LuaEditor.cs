using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class LuaEditor : EditorWindow
{
    // every X seconds, save the edited lua code
    const float SAVE_INTERVALL = 2.0f;
    const string SAVE_CODE_KEY = "SWBF2RuntimeLuaEditorCode";
    float SaveCounter = 0;

    string LuaCode;
    
    [MenuItem("Runtime/Lua Editor")]
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

        Repaint();
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

        GUILayout.BeginVertical(GUILayout.Width(400));
        int stackSize = L.GetTop();
        for (int i = 1; i <= stackSize; ++i)
        {
            (string typeStr, string valStr) = runtime.LuaValueToStr(i);
            EditorGUILayout.LabelField(typeStr, valStr);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Execute"))
        {
            runtime.ExecuteString(LuaCode);
        }
    }
}
#endif