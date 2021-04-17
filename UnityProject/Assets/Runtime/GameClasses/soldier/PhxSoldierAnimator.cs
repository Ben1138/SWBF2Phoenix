using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxSoldierAnimator : CraAnimator
{
    static CraMask UpperBody = new CraMask(true, "");
    static CraMask LowerBody = new CraMask(true, "");

    int Idle;
    int Reload;


    public void Init()
    {
        Idle = AddState(CreateState("human_0", "human_rifle_stand_idle_emote_full", true));
        Reload = AddState(CreateState("human_0", "human_rifle_stand_reload_full", false));
        ConnectStates(Idle, Reload);
        SetState(Idle);

        OnStateFinished += () =>
        {
            if (CurrentStateIdx != Idle)
            {
                SetState(Idle);
            }
        };
    }

    public void PlayIntroAnim()
    {
        SetState(Reload);
    }

    CraState CreateState(string animBank, string animName, bool loop, string maskBone=null)
    {
        CraPlayer anim = new CraPlayer();
        anim.SetClip(PhxAnimationLoader.Import(animBank, animName));

        if (string.IsNullOrEmpty(maskBone))
        {
            anim.Assign(transform);
        }
        else
        {
            anim.Assign(transform, new CraMask(true, maskBone));
        }

        anim.Looping = loop;
        return new CraState(anim);
    }

    // Update is called once per frame
    void Update()
    {
        Tick(Time.deltaTime);
    }
}
