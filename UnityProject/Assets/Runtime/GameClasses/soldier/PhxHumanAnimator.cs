using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

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

public enum PhxAnimState
{
    Walking,
    Sprinting,
    Jumping,
    Falling,
    Landing
}

public class PhxHumanAnimator : CraAnimator
{
    [Range(-1f, 1f)]
    public float Forward;
    public PhxAnimState State;

    int StandSprint;
    int StandRun;
    int StandWalk;
    int StandIdle;
    int StandBackward;
    int StandReload;
    int Jump;
    int Land;


    public void Init()
    {
        StandSprint   = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_sprint_full", true));
        StandRun      = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_runforward", true));
        StandWalk     = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_walkforward", true));
        StandIdle     = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_idle_emote_full", true));
        StandBackward = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_runbackward", true));
        StandReload   = AddState(1, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_reload_full", false, "bone_a_spine"));
        Jump          = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_jump", true));
        Land          = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_landsoft", true));

        SetState(0, StandIdle);
        SetState(1, CraSettings.STATE_NONE);

        OnStateFinished += StateFinished;
    }

    public void PlayIntroAnim()
    {
        SetState(1, StandReload);
    }

    void StateFinished(int layer)
    {
        if (layer == 1)
        {
            SetState(1, CraSettings.STATE_NONE);
        }
    }

    void Update()
    {
        Profiler.BeginSample("PhxHumanAnimator State Update");
        if (State == PhxAnimState.Sprinting)
        {
            SetState(0, StandSprint);
        }
        else if (State == PhxAnimState.Jumping)
        {
            SetState(0, Jump);
        }
        else if (State == PhxAnimState.Landing)
        {
            SetState(0, Land);
        }
        else if (Forward > 0.2f && Forward <= 0.75f)
        {
            SetState(0, StandWalk);
            SetPlaybackSpeed(0, StandWalk, Forward / 0.75f);
        }
        else if (Forward > 0.75f)
        {
            SetState(0, StandRun);
            SetPlaybackSpeed(0, StandRun, Forward / 1f);
        }
        else if (Forward < -0.2f)
        {
            SetState(0, StandBackward);
            SetPlaybackSpeed(0, StandBackward, -Forward / 1f);
        }
        else
        {
            SetState(0, StandIdle);
        }
        Profiler.EndSample();

        Profiler.BeginSample("PhxHumanAnimator Tick");
        Tick(Time.deltaTime);
        Profiler.EndSample();
    }
}