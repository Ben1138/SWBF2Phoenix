using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class EnvironmentMonitor : EditorWindow
{
    GUIStyle EnvLVLStyle = new GUIStyle();
    GUIStyle FallbackLVLStyle = new GUIStyle();


    void OnEnable()
    {
        EnvLVLStyle.normal.textColor = Color.green;
        FallbackLVLStyle.normal.textColor = Color.yellow;
    }

    [MenuItem("Environment/Monitor")]
    public static void OpenEnvironmentMonitor()
    {
        EnvironmentMonitor window = GetWindow<EnvironmentMonitor>();
        window.Show();
    }

    void OnGUI()
    {
        RuntimeEnvironment env = GameRuntime.GetCurrentEnvironment();
        if (!Application.isPlaying || GameRuntime.Instance == null || env == null)
        {
            EditorGUILayout.LabelField("Game is not running");
            return;
        }

        Path gamePath = GameRuntime.Instance.GamePath;
        EditorGUILayout.LabelField("Environment Path", env.Path - gamePath);
        EditorGUILayout.LabelField("Fallback Path", env.FallbackPath - gamePath);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Environment State", env.State.ToString());
        EditorGUILayout.Space();


        foreach (var lvl in env.LVLs)
        {
            EditorGUILayout.LabelField(lvl.RelativePath, lvl.Level == null ? string.Format("{0:0.} %", env.GetProgress(lvl.Handle) * 100.0f) : "Loaded", lvl.bIsFallback ? FallbackLVLStyle : EnvLVLStyle);
        }
    }
}
#endif