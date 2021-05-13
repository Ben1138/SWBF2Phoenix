using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhxEnvironmentMonitor : EditorWindow
{
    GUIStyle EnvLVLStyle = new GUIStyle();
    GUIStyle FallbackLVLStyle = new GUIStyle();


    void OnEnable()
    {
        EnvLVLStyle.normal.textColor = Color.green;
        FallbackLVLStyle.normal.textColor = Color.yellow;
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
        PhxRuntimeEnvironment env = PhxGameRuntime.GetEnvironment();
        if (!Application.isPlaying || PhxGameRuntime.Instance == null || env == null)
        {
            EditorGUILayout.LabelField("Game is not running");
            return;
        }

        PhxPath gamePath = PhxGameRuntime.Instance.GamePath;
        EditorGUILayout.LabelField("Environment Path", env.Path - gamePath);
        EditorGUILayout.LabelField("Fallback Path", env.FallbackPath - gamePath);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Environment Stage", env.Stage.ToString());
        EditorGUILayout.Space();

        foreach (var lvl in env.LVLs)
        {
            EditorGUILayout.LabelField(lvl.RelativePath, "Loaded", lvl.bIsFallback ? FallbackLVLStyle : EnvLVLStyle);
        }
        foreach (var lvl in env.LoadingLVLs)
        {
            EditorGUILayout.LabelField(lvl.PathPartial, string.Format("{0:0.} %", env.GetProgress(lvl.Handle) * 100.0f), lvl.bIsFallback ? FallbackLVLStyle : EnvLVLStyle);
        }
        EditorGUILayout.Space();

        string memStr = "";
        if (CraClip.GlobalBakeMemoryConsumption >= 1000)
        {
            memStr = (CraClip.GlobalBakeMemoryConsumption / 1000f).ToString() + " KB";
        }
        else if (CraClip.GlobalBakeMemoryConsumption >= 1000000)
        {
            memStr = (CraClip.GlobalBakeMemoryConsumption / 1000000f).ToString() + " MB";
        }
        else
        {
            memStr = CraClip.GlobalBakeMemoryConsumption.ToString() + " Bytes";
        }

        EditorGUILayout.LabelField("Animation Memory", memStr);
    }
}