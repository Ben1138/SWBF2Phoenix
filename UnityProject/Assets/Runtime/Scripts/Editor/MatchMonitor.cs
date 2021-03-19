using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class MatchMonitor : EditorWindow
{
    Vector2 ScrollPos;

    [MenuItem("Runtime/Match Monitor")]
    public static void OpenLuaEditor()
    {
        MatchMonitor window = GetWindow<MatchMonitor>();
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

        GameMatch gm = GameRuntime.GetMatch();
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        for (int i = 0; i < GameMatch.MAX_TEAMS; ++i)
        {
            GameMatch.Team t = gm.Teams[i];

            EditorGUILayout.LabelField("Team ID", (i+1).ToString());
            EditorGUILayout.LabelField("Name", t.Name);
            EditorGUILayout.LabelField("Aggressiveness", t.Aggressiveness.ToString());
            EditorGUILayout.LabelField("Icon", t.Icon?.ToString());
            EditorGUILayout.LabelField("Unit Count", t.UnitCount.ToString());
            EditorGUILayout.LabelField("Reinforcement Count", t.ReinforcementCount.ToString());
            EditorGUILayout.LabelField("Spawn Delay", t.SpawnDelay.ToString());
            EditorGUILayout.LabelField("Hero Class", t.HeroClass?.ToString());
            GUILayout.Label("Unit Classes:");
            foreach (GameMatch.UnitClass unitClass in t.UnitClasses)
            {
                EditorGUILayout.LabelField("    " + unitClass.Unit.Name, unitClass.Count.ToString());
            }
            GUILayout.Space(20);
        }
        EditorGUILayout.EndScrollView();
    }
}