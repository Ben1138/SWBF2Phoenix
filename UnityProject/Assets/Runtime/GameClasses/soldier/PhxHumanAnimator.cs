using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        //ConnectStates(StandRunForward, StandWalkForward);

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
            SetState(0, StandRunForward);
            SetPlaybackSpeed(0, StandRunForward, Forward / 1f);
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
