using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class PhxMatchMonitor : EditorWindow
{
    Vector2 ScrollPos;

    [MenuItem("Phoenix/Match Monitor")]
    public static void OpenLuaEditor()
    {
        PhxMatchMonitor window = GetWindow<PhxMatchMonitor>();
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

        PhxMatch gm = PhxGame.GetMatch();
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        for (int i = 0; i < PhxMatch.MAX_TEAMS; ++i)
        {
            PhxMatch.PhxTeam t = gm.Teams[i];

            EditorGUILayout.LabelField("Team ID", (i+1).ToString());
            EditorGUILayout.LabelField("Name", t.Name);
            EditorGUILayout.LabelField("Aggressiveness", t.Aggressiveness.ToString());
            EditorGUILayout.LabelField("Icon", t.Icon?.ToString());
            EditorGUILayout.LabelField("Unit Count", t.UnitCount.ToString());
            EditorGUILayout.LabelField("Reinforcement Count", t.ReinforcementCount.ToString());
            EditorGUILayout.LabelField("Spawn Delay", t.SpawnDelay.ToString());
            EditorGUILayout.LabelField("Hero Class", t.HeroClass?.Name);
            GUILayout.Label("Unit Classes:");
            foreach (PhxMatch.PhxUnitClass unitClass in t.UnitClasses)
            {
                EditorGUILayout.LabelField($"    {unitClass.Unit.Name}, {unitClass.CountMin}, {unitClass.CountMax}");
            }
            GUILayout.Space(20);
        }
        EditorGUILayout.EndScrollView();
    }
}