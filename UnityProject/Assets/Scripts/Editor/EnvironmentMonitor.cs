using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnvironmentMonitor : EditorWindow
{
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

        foreach (var lvl in env.LoadedLVLs)
        {
            EditorGUILayout.LabelField(lvl.Name, "[ Loaded ]");
        }

        foreach (LVLHandle handle in env.LoadingLVLs)
        {
            EditorGUILayout.LabelField(handle.GetRelativePath(), string.Format("{0:0.} %", env.GetProgress(handle) * 100.0f));
        }
    }
}
