
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
        public PhxProp<string> AnimationTrigger = new PhxProp<string>(null);
        public PhxProp<string> AttachTrigger = new PhxProp<string>(null);
    }

    PhxAnimatedProp.ClassProperties APropClass;

    CraAnimator Animator;

    Dictionary<string, int> AnimStates = new Dictionary<string, int>();

    bool IsIdling;


    public override void Init()
    {
        base.Init();

        APropClass = C as PhxAnimatedProp.ClassProperties;


        Animator = CraAnimator.CreateNew(1, APropClass.Animation.Values.Count);

        // Add each Animation as a separate state
        foreach (object[] values in APropClass.Animation.Values)
        {
            string CurrAnimName = (values[0] as string);

            CraPlayer CurrPlayer = PhxAnimationLoader.CreatePlayer(transform, APropClass.AnimationName.Get(), CurrAnimName, false);
            if (CurrPlayer.IsValid())
            {
                if (CurrAnimName.Equals("idle", StringComparison.OrdinalIgnoreCase))
                {
                    CurrPlayer.SetLooping(true);
                }

                AnimStates[CurrAnimName] = Animator.AddState(0, CurrPlayer);
            }
            else 
            {
                Debug.LogErrorFormat("Animatedprop failed to add anim state: {0}", CurrAnimName);
            }
        }

        if (APropClass.AnimationTrigger.Get() != null)
        {
            var Body = gameObject.AddComponent<Rigidbody>();
            Body.isKinematic = true;

            string[] Parts = APropClass.AnimationTrigger.Get().Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);

            if (Parts.Length >= 3)
            {
                string AnimName = Parts[0];
                Transform TriggerNode = UnityUtils.FindChildTransform(transform, Parts[1]);
                float TriggerRadius = PhxUtils.FloatFromString(Parts[2]);

                if (TriggerNode != null && AnimStates.ContainsKey(AnimName))
                {
                    TriggerNode.gameObject.layer = LayerMask.NameToLayer("BuildingSoldier");
                    PhxTrigger Trigger = TriggerNode.gameObject.AddComponent<PhxTrigger>();
                    Trigger.Init(TriggerRadius, () => PlayTriggerAnim(AnimName), null);
                }          
            }
        }

        if (AnimStates.ContainsKey("idle"))
        {
            IsIdling = true;
            Animator.SetState(0, AnimStates["idle"]);            
        }
    }


    public void PlayTriggerAnim(string ColliderName)
    {
        if (IsIdling)
        {
            IsIdling = false;
            Animator.SetState(0, AnimStates[ColliderName]);
        }
    }


    public override void Tick(float deltaTime)
    {
        if (!IsIdling)
        {
            if (!Animator.GetCurrentState(0).IsPlaying())
            {
                Animator.SetState(0, AnimStates["idle"]);
                IsIdling = true;
            }
        }
    } 
}
