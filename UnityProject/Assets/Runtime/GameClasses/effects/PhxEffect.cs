using System.Collections.Generic;
using UnityEngine;

using LibSWBF2.Utils;



public class PhxEffect
{
    public GameObject EffectObject;
    List<ParticleSystem> Systems;

    public readonly string EffectName = "";

    public bool IsFree = true;

    public bool IsPlaying = false;

    public bool IsStillPlaying()
    {
        foreach (ParticleSystem ps in Systems)
        {
            if (ps.isPlaying) return true;
        }

        return false;
    }


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

            fx.SetActive(false);
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

            EffectObject.SetActive(false);
        }
    }

    public void SetParent(Transform Parent)
    {
        if (EffectObject != null)
        {
            EffectObject.transform.parent = Parent;
            EffectObject.transform.localPosition = Vector3.zero;
            EffectObject.transform.localRotation = Quaternion.identity;             
        }
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

    // Needs to be set if source is moving!!!
    public void SetDynamic(bool Dynamic = true)
    {
        foreach (ParticleSystem ps in Systems)
        {
            var velLifetime = ps.velocityOverLifetime;

            // If velLifetime is not enabled, we know the effect is stationary and thus should be left alone.
            // If it is enabled, we know all simulation must be done in world space so things like missile trails
            // and smoke plumes from damage effects will linger realistically.

            // TODO: If an effect has a dummy acceleration (e.g. (0,0,0)) in its position transformation,
            // does that mean it should simulate in world space?
            if (velLifetime.enabled)
            {
                velLifetime.space = Dynamic ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
                
                var mainModule = ps.main;
                mainModule.simulationSpace = Dynamic ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            }
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
