using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhxEnvironmentMonitor : EditorWindow
{
    GUIStyle StdStyle = new GUIStyle();
    GUIStyle AddonStyle = new GUIStyle();


    void OnEnable()
    {
        StdStyle.normal.textColor = Color.green;
        AddonStyle.normal.textColor = Color.yellow;
    }

    [MenuItem("Phoenix/Environment Monitor")]
    public static void OpenEnvironmentMonitor()
    {
        PhxEnvironmentMonitor window = GetWindow<PhxEnvironmentMonitor>();
        window.Show();
    }

    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        PhxEnvironment env = PhxGame.GetEnvironment();
        if (!Application.isPlaying || PhxGame.Instance == null || env == null)
        {
            EditorGUILayout.LabelField("Game is not running");
            return;
        }

        PhxPath gamePath = PhxGame.Instance.GamePath;
        EditorGUILayout.LabelField("Game Path", env.GameDataPath - gamePath);
        EditorGUILayout.LabelField("Addon Path", env.AddonDataPath != null ? (env.AddonDataPath - gamePath).ToString() : "NONE");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Environment Stage", env.Stage.ToString());
        EditorGUILayout.Space();

        foreach (var lvl in env.Loaded)
        {
            EditorGUILayout.LabelField(lvl.DisplayPath, "Loaded", lvl.bIsAddon ? AddonStyle : StdStyle);
        }
        foreach (var lvl in env.Loading)
        {
            EditorGUILayout.LabelField(lvl.DisplayPath, string.Format("{0:0.} %", env.GetProgress(lvl.Handle) * 100.0f), lvl.bIsAddon ? AddonStyle : StdStyle);
        }
    }
}