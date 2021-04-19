using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public static class PhxAnimationBanks
{
    public static readonly Dictionary<string, Dictionary<string, string>> Banks = new Dictionary<string, Dictionary<string, string>>()
    {
        { 
            "human", new Dictionary<string, string>() 
            { 
                { "StandIdle", "human_rifle_stand_idle_emote_full" } 
            } 
        }
    };
}


public class PhxHumanAnimator : CraAnimator
{
    [Range(-1f, 1f)]
    public float Forward;
    public bool Sprinting;

    int StandSprintFull;
    int StandRunForward;
    int StandWalkForward;
    int StandIdle;
    int StandRunBackward;
    int StandReload;

    float LastForward;


    public void Init()
    {
        StandSprintFull  = AddState(0, PhxAnimationLoader.CreateState(transform, "human_1", "human_rifle_sprint_full", true));
        StandRunForward  = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_runforward", true));
        StandWalkForward = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_walkforward", true));
        StandIdle        = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_idle_emote_full", true));
        StandRunBackward = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_runbackward", true));
        StandReload      = AddState(1, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_reload_full", false, "bone_a_spine"));

        ConnectStates(0, StandIdle, StandRunForward);
        ConnectStates(0, StandIdle, StandWalkForward);
        ConnectStates(0, StandIdle, StandRunBackward);
        //ConnectStates(StandIdle, StandReload);

        ConnectStates(0, StandWalkForward, StandRunForward);

        SetState(0, StandIdle);
        SetState(1, CraSettings.STATE_NONE);

        OnStateFinished += StateFinished;
    }

    public void PlayIntroAnim()
    {
        SetState(1, StandReload);
    }

    void StateFinished(int level)
    {
        if (level == 1 && Layers[1].CurrentStateIdx != CraSettings.STATE_NONE)
        {
            SetState(1, CraSettings.STATE_NONE);
        }
    }

    // Update is called once per frame
    void Update()
    {
        BlendTo(0, CraSettings.STATE_NONE, 0f);
        if (Sprinting)
        {
            SetState(0, StandSprintFull);
        }
        else if (Forward > 0 && Forward <= 0.5f)
        {
            SetState(0, StandWalkForward);
            SetPlaybackSpeed(0, StandWalkForward, Forward / 0.5f);
        }
        else if (Forward > 0.5f)
        {
            SetState(0, StandWalkForward);
            BlendTo(0, StandRunForward, (Forward - 0.5f) / 0.5f);
            //SetPlaybackSpeed(0, StandRunForward, Forward / 1f);
        }
        else if (Forward < 0f)
        {
            SetState(0, StandRunBackward);
            SetPlaybackSpeed(0, StandRunBackward, -Forward / 1f);
        }
        else
        {
            SetState(0, StandIdle);
        }

        Tick(Time.deltaTime);
        LastForward = Forward;
    }
}

//[CustomEditor(typeof(PhxHumanAnimator))]
//[CanEditMultipleObjects]
//public class CraAnimatorEditor : Editor
//{
//    int ChosenLayer = 0;

//    void OnEnable()
//    {

//    }

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        PhxHumanAnimator anim = (PhxHumanAnimator)target;

//        ChosenLayer = EditorGUILayout.IntField(ChosenLayer);
//        if (ChosenLayer >= 0 && ChosenLayer < anim.Layers.Length)
//        {
//            CraLayer layer = anim.Layers[ChosenLayer];
//            layer.CurrentStateIdx = EditorGUILayout.IntField(layer.CurrentStateIdx);
//            layer.BlendStateIdx = EditorGUILayout.IntField(layer.BlendStateIdx);
//            layer.BlendValue = EditorGUILayout.FloatField(layer.BlendValue);
//        }
//    }
//}