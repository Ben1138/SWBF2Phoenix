using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhxSceneMonitor : EditorWindow
{
    Vector2 Scroll;

    [MenuItem("Phoenix/Scene Monitor")]
    public static void OpenSceneMonitor()
    {
        PhxSceneMonitor window = GetWindow<PhxSceneMonitor>();
        window.Show();
    }

    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        PhxScene scene = PhxGame.GetScene();
        if (!Application.isPlaying || PhxGame.Instance == null || scene == null)
        {
            EditorGUILayout.LabelField("Game is not running");
            return;
        }

        Scroll = EditorGUILayout.BeginScrollView(Scroll);
        EditorGUILayout.LabelField("World Instances", scene.GetInstanceCount().ToString());
        EditorGUILayout.LabelField("Tickables", scene.GetTickableCount().ToString());
        EditorGUILayout.LabelField("Tickables Physics", scene.GetTickablePhysicsCount().ToString());
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Projectiles", $"{scene.GetActiveProjectileCount()}/{scene.GetTotalProjectileCount()}");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Command posts:");
        foreach (var cp in scene.GetCommandPosts())
        {
            EditorGUILayout.LabelField($"Name: {cp.name}", $"Team: {cp.Team.Get()}");
        }
        EditorGUILayout.EndScrollView();
    }
}