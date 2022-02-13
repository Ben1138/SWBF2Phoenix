
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public class PhxDoor : PhxProp
{
    public class ClassProperties : PhxProp.ClassProperties 
    {
        public PhxProp<string> AnimationName = new PhxProp<string>("");
        public PhxProp<string> Animation = new PhxProp<string>("");
        public PhxMultiProp AnimationTrigger = new PhxMultiProp(typeof(string), typeof(float));

        public PhxProp<AudioClip> OpenSound = new PhxProp<AudioClip>(null);
        public PhxProp<AudioClip> CloseSound = new PhxProp<AudioClip>(null);
        public PhxProp<AudioClip> LockedSound = new PhxProp<AudioClip>(null);
    }

    PhxDoor.ClassProperties DoorClass;


    // Never heard of door locking functionality in SWBF2
    public PhxProp<bool> Islocked = new PhxProp<bool>(false);

    // Door animator; only a CraPlayer is needed for this simple case
    CraPlayer Player;

    // How many colliders (i.e. vehicles and soldiers) are in door's trigger
    int NumColliders;



    public override void Init()
    {
        base.Init();

        Transform TriggerNode = null;
        float TriggerRadius = 1f;

        DoorClass = C as PhxDoor.ClassProperties;

        foreach (object[] values in DoorClass.AnimationTrigger.Values)
        {
            if (values.Length == 0)
            {
                continue;
            }

            string nodeName = values[0] as string;
            if (!string.IsNullOrEmpty(nodeName))
            {
                TriggerNode = UnityUtils.FindChildTransform(transform, nodeName);
            }

            if (values.Length > 1)
            {
                TriggerRadius = (float) values[1];
            }
        }

        if (TriggerNode != null)
        {
            var Body = gameObject.AddComponent<Rigidbody>();
            Body.isKinematic = true;

            TriggerNode.gameObject.layer = LayerMask.NameToLayer("BuildingSoldier");
            SphereCollider TriggerCollider = TriggerNode.gameObject.AddComponent<SphereCollider>();
            TriggerCollider.radius = TriggerRadius;
            TriggerCollider.isTrigger = true;
        } 

        Player = PhxAnimationLoader.CreatePlayer(transform, DoorClass.AnimationName.Get(), DoorClass.Animation.Get(), false);
        Player.SetPlaybackSpeed(1f);
        Player.SetLooping(false);
    }

    public override void Destroy()
    {
        
    }

    void OnTriggerEnter(Collider Object)
    {
        if (Object.gameObject.layer != LayerMask.NameToLayer("SoldierAll"))
        {
            return;
        }
        else 
        {
            NumColliders++;
        }

        Player.SetPlaybackSpeed(1f); 
        if (NumColliders > 0 && !Player.IsPlaying())
        {
            Player.Play();
        }
    }


    void OnTriggerExit(Collider Object)
    {
        if (Object.gameObject.layer != LayerMask.NameToLayer("SoldierAll"))
        {
            return;
        }
        else
        {
            NumColliders--;
        }

        if (NumColliders == 0)
        {
            Player.SetPlaybackSpeed(-1f); 
            if (!Player.IsPlaying())
            {
                Player.Play();      
            }  
        }
    }
}
