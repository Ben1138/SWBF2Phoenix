using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class TimerMonitor : EditorWindow
{
    Vector2 ScrollPos;

    [MenuItem("Runtime/Timer Monitor")]
    public static void OpenLuaEditor()
    {
        TimerMonitor window = GetWindow<TimerMonitor>();
        window.Show();
    }

    void Awake()
    {
        
    }

    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        LuaRuntime rt = GameRuntime.GetLuaRuntime();
        if (!Application.isPlaying || rt == null)
        {
            EditorGUILayout.LabelField("LUA is not running");
            return;
        }

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        TimerDB tdb = GameRuntime.GetTimerDB();
        for (int i = 0; i < tdb.InUseIndices.Count; ++i)
        {
            int idx = tdb.InUseIndices[i];
            tdb.GetTimer(idx, out TimerDB.Timer timer);

            EditorGUILayout.LabelField("Name", timer.Name);
            EditorGUILayout.LabelField("Elapsed", (Mathf.Round(timer.Elapsed * 100f) / 100f).ToString());
            EditorGUILayout.LabelField("Target", timer.Target.ToString());
            EditorGUILayout.LabelField("Rate", timer.Rate.ToString());
            EditorGUILayout.LabelField("IsRunning", timer.IsRunning.ToString());
            GUILayout.Space(20);
        }
        EditorGUILayout.EndScrollView();
    }
}