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

    int StandRunSprint;
    int StandWalkRun;
    int StandIdleWalk;
    int StandIdleBackward;
    int StandReload;


    public void Init()
    {
        StandRunSprint    = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_runforward",      "human_1", "human_rifle_sprint_full",       true));
        StandWalkRun      = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_walkforward",     "human_0", "human_rifle_stand_runforward",  true));
        StandIdleWalk     = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_idle_emote_full", "human_0", "human_rifle_stand_walkforward", true));
        StandIdleBackward = AddState(0, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_idle_emote_full", "human_0", "human_rifle_stand_runbackward", true));
        StandReload       = AddState(1, PhxAnimationLoader.CreateState(transform, "human_0", "human_rifle_stand_reload_full",      false,    "bone_a_spine"));

        SetState(0, StandIdleWalk);
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

    void Update()
    {
        //if (Sprinting)
        //{
        //    SetState(0, StandRunSprint);
        //}
        //else if (Forward > 0 && Forward <= 0.5f)
        //{
        //    SetState(0, StandIdleWalk);
        //    SetPlaybackSpeed(0, StandIdleWalk, Forward / 0.5f);
        //}
        //else if (Forward > 0.5f)
        //{
        //    SetState(0, StandIdleWalk);
        //    //SetPlaybackSpeed(0, StandRunForward, Forward / 1f);
        //}
        //else if (Forward < 0f)
        //{
        //    SetState(0, StandIdleBackward);
        //    SetPlaybackSpeed(0, StandIdleBackward, -Forward / 1f);
        //}
        //else
        //{
        //    SetState(0, StandIdleWalk);
        //}

        Tick(Time.deltaTime);
    }
}