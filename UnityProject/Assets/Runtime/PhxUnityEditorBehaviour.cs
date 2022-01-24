#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class PhxUnityEditorBehaviour
{
    static PhxUnityEditorBehaviour()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        Debug.Log("Attached to play mode state change callback");
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting play mode, attempt to destroy PhxGame...");
            PhxGame.Instance?.Destroy();
        }
    }
}
#endif