using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class PhxTimerMonitor : EditorWindow
{
    Vector2 ScrollPos;

    [MenuItem("Runtime/Timer Monitor")]
    public static void OpenLuaEditor()
    {
        PhxTimerMonitor window = GetWindow<PhxTimerMonitor>();
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
        PhxLuaRuntime rt = PhxGameRuntime.GetLuaRuntime();
        if (!Application.isPlaying || rt == null)
        {
            EditorGUILayout.LabelField("LUA is not running");
            return;
        }

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        PhxTimerDB tdb = PhxGameRuntime.GetTimerDB();
        for (int i = 0; i < tdb.InUseIndices.Count; ++i)
        {
            int idx = tdb.InUseIndices[i];
            tdb.GetTimer(idx, out PhxTimerDB.PhxTimer timer);

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