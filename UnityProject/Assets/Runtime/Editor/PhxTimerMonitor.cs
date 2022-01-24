using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class PhxTimerMonitor : EditorWindow
{
    Vector2 ScrollPos;

    [MenuItem("Phoenix/Timer Monitor")]
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
        PhxLuaRuntime rt = PhxGame.GetLuaRuntime();
        if (!Application.isPlaying || rt == null)
        {
            EditorGUILayout.LabelField("LUA is not running");
            return;
        }

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        PhxTimerDB tdb = PhxGame.GetTimerDB();
        for (int i = 0; i < tdb.InUseIndices.Count; ++i)
        {
            int idx = tdb.InUseIndices[i];
            tdb.GetTimer(idx, out PhxTimerDB.PhxTimer timer);

            EditorGUILayout.LabelField("Name", timer.Name);
            EditorGUILayout.LabelField("Time", (Mathf.Round(timer.Time * 100f) / 100f).ToString());
            EditorGUILayout.LabelField("Rate", timer.Rate.ToString());
            EditorGUILayout.LabelField("IsRunning", timer.IsRunning.ToString());
            GUILayout.Space(20);
        }
        EditorGUILayout.EndScrollView();
    }
}