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
    List<Vector3> StartPoints;
    List<string>    AnimationNames;

    public PhxAnimationGroup()
    {
        Animators = new List<Animation>();
        AnimationNames = new List<string>();
        StartPoints = new List<Vector3>();
    }

    public bool AddInstanceAnimationPair(Animation anim, string AnimationName)
    {
        StartPoints.Add(anim.gameObject.transform.position);
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
            Animators[i].gameObject.transform.parent.position = StartPoints[i];
            Animators[i].gameObject.transform.localPosition = Vector3.zero;
            
            Animators[i].enabled = true;
            Animators[i].Play(AnimationNames[i]);

            AnimationState aState = Animators[i][AnimationNames[i]];
        }
    }

    public void Rewind()
    {
        for (int i = 0; i < Animators.Count; i++)
        {
            Animators[i].Rewind(AnimationNames[i]);
        }   
    }

    public void SetStartPoint()
    {
        for (int i = 0; i < Animators.Count; i++)
        {
            Transform tx = Animators[i].gameObject.transform;
            StartPoints[i] = tx.position;
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


    public void InitializeWorldAnimations(World[] worlds)
    {
        Dictionary<string, WorldAnimation> WorldAnims = new Dictionary<string, WorldAnimation>();
        foreach (World world in worlds)
        {
            foreach (WorldAnimation worldAnim in world.GetAnimations())
            {
                WorldAnims[worldAnim.Name] = worldAnim;
            }
        }

        foreach (World world in worlds)
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

                    if (instance == null || !WorldAnims.TryGetValue(pair.Item1, out WorldAnimation wldAnim))
                    {
                        continue;
                    }

                    instance.isStatic = false;

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


                    AnimationCurve[] rKeys = AnimationLoader.Instance.GetWorldAnimationRotationCurves(wldAnim, instance.transform);
                    AnimationCurve[] pKeys = AnimationLoader.Instance.GetWorldAnimationPositionCurves(wldAnim, instance.transform);

                    AnimationClip clip = new AnimationClip();
                    clip.legacy = true;
                    clip.name = wldAnim.Name;

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

                    clip.wrapMode = wldAnim.IsLooping ? WrapMode.Loop : WrapMode.ClampForever;


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
