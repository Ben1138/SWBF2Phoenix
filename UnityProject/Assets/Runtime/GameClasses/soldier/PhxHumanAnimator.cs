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

//public enum PhxAnimState
//{
//    Walk,
//    Sprint,
//    Jump,
//    Fall,
//    Land,
//    TurnLeft,
//    TurnRight
//}

public class PhxHumanAnimator : CraAnimator
{
    //[Range(-1f, 1f)]
    //public float Forward;
    //public PhxAnimState State;

    public int StandSprint { get; private set; }
    public int StandRun { get; private set; }
    public int StandWalk { get; private set; }
    public int StandIdle { get; private set; }
    public int StandBackward { get; private set; }
    public int StandReload { get; private set; }

    public int StandAlertIdle { get; private set; }
    public int StandAlertWalk { get; private set; }
    public int StandAlertRun { get; private set; }
    public int StandAlertBackward { get; private set; }
    public int Jump { get; private set; }
    public int Fall { get; private set; }
    public int LandSoft { get; private set; }
    public int LandHard { get; private set; }
    public int TurnLeft { get; private set; }
    public int TurnRight { get; private set; }


    public void Init()
    {
        StandIdle          = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_idle_emote_full", true));
        StandWalk          = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_walkforward", true));
        StandRun           = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_runforward", true));
        StandSprint        = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_sprint_full", true));
        StandBackward      = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_runbackward", true));
        StandReload        = AddState(1, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_reload_full", false, "bone_a_spine"));
        StandAlertIdle     = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_standalert_idle_emote_full", true));
        StandAlertWalk     = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_standalert_walkforward", true));
        StandAlertRun      = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_standalert_runforward", true));
        StandAlertBackward = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_standalert_runbackward", true));
        Jump               = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_jump", false));
        Fall               = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_fall", true));
        LandSoft           = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_landsoft", false));
        LandHard           = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_1", "human_rifle_landhard", false));
        TurnLeft           = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_turnleft", false));
        TurnRight          = AddState(0, PhxAnimationLoader.CreatePlayer(transform, "human_0", "human_rifle_stand_turnright", false));

        SetState(0, StandIdle);
        SetState(1, CraSettings.STATE_NONE);
    }

    public void PlayIntroAnim()
    {
        SetState(1, StandReload);
    }

    void Update()
    {
        Profiler.BeginSample("PhxHumanAnimator Tick");
        Tick(Time.deltaTime);
        Profiler.EndSample();
    }
}