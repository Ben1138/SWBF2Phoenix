using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxSoldierAnimator : CraAnimator
{
    CraPlayer StandRunForward = new CraPlayer();
    CraPlayer StandWalkForward = new CraPlayer();
    CraPlayer StandIdle = new CraPlayer();
    CraPlayer StandWalkBackward = new CraPlayer();

    bool IsInit = false;


    public void Init()
    {
        StandIdle.SetClip(PhxAnimationLoader.Import("human_0", "human_rifle_stand_idle_emote_full"));
        StandIdle.Assign(transform);
        StandIdle.Looping = true;
        StandIdle.Play();
        IsInit = true;
    }

    public void PlayIntroAnim()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInit) return;
        StandIdle.Update(Time.deltaTime);
    }
}
