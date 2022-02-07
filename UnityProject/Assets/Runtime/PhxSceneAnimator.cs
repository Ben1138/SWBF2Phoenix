using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



public class PhxAnimationGroup
{
    List<Animation> Animators;
    List<string>    AnimationNames;

    public PhxAnimationGroup()
    {
        Animators = new List<Animation>();
        AnimationNames = new List<string>();
    }

    public bool AddInstanceAnimationPair(Animation anim, string AnimationName)
    {
        Animators.Add(anim);
        AnimationNames.Add(AnimationName);

        return true;
    }


    public void Pause()
    {
        foreach (Animation anim in Animators)
        {
            anim.enabled = false;
        }
    }

    public void Play()
    {
        for (int i = 0; i < Animators.Count; i++)
        {
            Animators[i].enabled = true;
            Animators[i].Play(AnimationNames[i]);
        }
    }

    public void Rewind()
    {
        for (int i = 0; i < Animators.Count; i++)
        {
            Animators[i].Rewind(AnimationNames[i]);
        }   
    }
}









public class PhxSceneAnimator
{
    Dictionary<string, AnimationClip> AnimDB;
    Dictionary<string, PhxAnimationGroup> AnimGroupDB;
   
    public PhxSceneAnimator()
    {
        AnimDB = new Dictionary<string, AnimationClip>();
        AnimGroupDB = new Dictionary<string, PhxAnimationGroup>();
    }


    public void InitializeAnimations(World[] worlds)
    {
        foreach (World wld in worlds)
        {
            InitializeAnimations(wld);
        }
    }

    public void InitializeAnimations(World world)
    {
        WorldAnimation[] worldAnims = world.GetAnimations();

        foreach (WorldAnimation anim in worldAnims)
        {
            if (AnimDB.ContainsKey(anim.Name))
            {
                continue;
            }

            AnimationCurve[] rKeys = AnimationLoader.Instance.GetWorldAnimationRotationCurves(anim);
            AnimationCurve[] pKeys = AnimationLoader.Instance.GetWorldAnimationPositionCurves(anim);

            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            clip.name = anim.Name;

            if (rKeys != null)
            {
                clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rKeys[0]);
                clip.SetCurve("", typeof(Transform), "localEulerAngles.y", rKeys[1]);
                clip.SetCurve("", typeof(Transform), "localEulerAngles.z", rKeys[2]);
            }

            if (pKeys != null)
            {
                clip.SetCurve("", typeof(Transform), "localPosition.x", pKeys[0]);
                clip.SetCurve("", typeof(Transform), "localPosition.y", pKeys[1]);
                clip.SetCurve("", typeof(Transform), "localPosition.z", pKeys[2]);  
            } 

            clip.wrapMode = anim.IsLooping ? WrapMode.Loop : WrapMode.ClampForever;

            AnimDB[anim.Name] = clip;
        }
    }
        


    public void InitializeAnimationGroups(World[] wlds)
    {
        foreach (World wld in wlds)
        {
            InitializeAnimationGroups(wld);
        }
    }

    public void InitializeAnimationGroups(World world)
    {
        foreach (WorldAnimationGroup animGroup in world.GetAnimationGroups())
        {
            if (AnimGroupDB.ContainsKey(animGroup.Name)) continue;

            PhxAnimationGroup newAnimGroup = new PhxAnimationGroup();
            AnimGroupDB[animGroup.Name.ToLower()] = newAnimGroup;

            List<Tuple<string,string>> AnimInstPairs = animGroup.GetAnimationInstancePairs();
            foreach (var pair in AnimInstPairs)
            {
                GameObject instance = GameObject.Find(pair.Item2);
                AnimationClip clip = AnimDB.ContainsKey(pair.Item1) ? AnimDB[pair.Item1] : null;

                if (instance == null || clip == null)
                {
                    continue;
                }

                instance.isStatic = false;


                // string path = AnimationUtility.CalculateTransformPath(instance, WorldRoot.transform);
                // Debug.LogFormat("  Instance: {0} will be animated by {1}", instanceObj.name, pair.Item1);

                Animation anim = instance.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = instance.AddComponent<Animation>();
                }

                if (instance.transform.parent.gameObject.name != instance.name + "_animroot")
                {
                    GameObject dummmyPrnt = new GameObject(instance.name + "_animroot");
                    dummmyPrnt.transform.position = instance.transform.position;
                    dummmyPrnt.transform.rotation = instance.transform.rotation;
                    dummmyPrnt.transform.SetParent(instance.transform.parent, true);

                    instance.transform.SetParent(dummmyPrnt.transform, true);                         
                }

                anim.AddClip(clip, clip.name);

                if (animGroup.PlaysAtStart)
                {
                    anim.clip = clip;
                    anim.playAutomatically = true;
                    anim.Play();
                }

                newAnimGroup.AddInstanceAnimationPair(anim, clip.name);
            }
        }
    }


    public bool PlayAnimation(string AnimationGroupName)
    {
        if (AnimGroupDB.TryGetValue(AnimationGroupName, out PhxAnimationGroup grp))
        {
            grp.Play();
            return true;
        }
        return false;
    }

    public bool RewindAnimation(string AnimationGroupName)
    {
        if (AnimGroupDB.TryGetValue(AnimationGroupName, out PhxAnimationGroup grp))
        {
            grp.Play();
            return true;
        }

        return false;
    }

    public bool PauseAnimation(string AnimationGroupName)
    {
        if (AnimGroupDB.TryGetValue(AnimationGroupName, out PhxAnimationGroup grp))
        {
            grp.Pause();
            return true;
        }

        return false;
    }
}
