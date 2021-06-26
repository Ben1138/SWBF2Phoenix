using System.Collections.Generic;
using UnityEngine;

using LibSWBF2.Utils;



public class PhxEffect
{
    GameObject EffectObject;
    List<ParticleSystem> Systems;

    public readonly string EffectName = "";

    public bool IsFree = true;

    public bool IsPlaying = false;


    public PhxEffect(GameObject fx)
    {
        if (fx != null)
        {
            Systems = new List<ParticleSystem>();
            foreach (ParticleSystem ps in fx.GetComponentsInChildren<ParticleSystem>())
            {
                Systems.Add(ps);
            }

            EffectObject = fx;

            EffectName = fx.name;
        }
    }

    public PhxEffect(PhxEffect EffectToCopy)
    {
        Systems = new List<ParticleSystem>();

        if (EffectToCopy.EffectObject != null)
        {
            EffectObject = GameObject.Instantiate(EffectToCopy.EffectObject);
            
            foreach (ParticleSystem ps in EffectObject.GetComponentsInChildren<ParticleSystem>())
            {
                Systems.Add(ps);
            } 

            EffectName = EffectToCopy.EffectObject.name; 
        }
    }

    public void SetParent(Transform Parent)
    {
        if (EffectObject != null)
            EffectObject.transform.parent = Parent;
    }

    public void SetLocalTransform(Vector3 Position, Quaternion Rotation)
    {
        if (EffectObject != null)
        {
            EffectObject.transform.localPosition = Position;
            EffectObject.transform.localRotation = Rotation;  
        }          
    }

    public void Release()
    {

    }


    public void SetLooping(bool Loop = true)
    {
        foreach (ParticleSystem ps in Systems)
        {
            var mainMod = ps.main;
            mainMod.loop = Loop;
        }
    }



    public void Play()
    {
        if (EffectObject == null)
        {
            Debug.LogWarningFormat("Effect object for {0} is null!", EffectName);
            return;
        }

        EffectObject?.SetActive(true);
        foreach (ParticleSystem ps in Systems)
        {
            ps.Play();
        }

        IsPlaying = true;
    }

    public void Stop()
    {
        if (EffectObject == null)
        {
            Debug.LogWarningFormat("Effect object for {0} is null!", EffectName);
            return;
        }

        foreach (ParticleSystem ps in Systems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        EffectObject?.SetActive(false);

    }

    public bool Refresh()
    {
        return true;
    }

    public bool IsStale()
    {
        return EffectObject == null;
    }
}
