
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Enums;
using System.Runtime.ExceptionServices;


public class PhxAnimatedProp : PhxProp
{
    public class ClassProperties : PhxProp.ClassProperties 
    {
        public PhxProp<string> AnimationName = new PhxProp<string>(null);
        public PhxMultiProp Animation = new PhxMultiProp(typeof(string));

        // These can be quite complex, best to parse manually
        public PhxProp<string> AttachTrigger = new PhxProp<string>(null);
        public PhxProp<string> AnimationTrigger = new PhxProp<string>(null);
    }

    PhxAnimatedProp.ClassProperties APropClass;

    CraPlayer Player;


    public override void Init()
    {
        base.Init();

        APropClass = C as PhxAnimatedProp.ClassProperties;


        bool HasIdle = false;
        foreach (object[] values in APropClass.Animation.Values)
        {
            HasIdle = (values[0] as string).Equals("idle", StringComparison.OrdinalIgnoreCase);
            if (HasIdle) break;
        }

        if (HasIdle && APropClass.AnimationName.Get() != null)
        {
            Player = PhxAnimationLoader.CreatePlayer(transform, APropClass.AnimationName.Get(), "idle", false);
            Player.SetPlaybackSpeed(1f);
            Player.SetLooping(true);  
            Player.Play();            
        }
    }
}
