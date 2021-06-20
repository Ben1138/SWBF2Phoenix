using System.Collections.Generic;
using UnityEngine;

using LibSWBF2.Utils;




public class PhxEffect
{
    GameObject EffectObject;
    List<ParticleSystem> Systems;

    public readonly string EffectName = "";

    public bool IsFree = true;


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
            ps.loop = Loop;
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




public class PhxEffectsManager
{
    PhxGameRuntime Game => PhxGameRuntime.Instance;

    Dictionary<uint, List<PhxEffect>> Effects;
    Dictionary<uint, float> EffectFrequencies;


    public PhxEffectsManager()
    {
        Effects = new Dictionary<uint, List<PhxEffect>>();
    }


    PhxEffect GetFreeEffect(string Name)
    {
        if (Name == null) return null;

        uint NameHash = HashUtils.GetFNV(Name);

        List<PhxEffect> EffectsList = null;
        if (Effects.ContainsKey(NameHash))
        {
            EffectsList = Effects[NameHash];
            if (EffectsList.Count < 5)
            {   
                PhxEffect Effect = new PhxEffect(EffectsList[0]);
                EffectsList.Add(Effect);
                return Effect;
            }
            else 
            {
                foreach (PhxEffect fx in EffectsList)
                {
                    if (fx.IsFree)
                    {
                        return fx;
                    }
                }

                return null;
            }
        }
        else 
        {
            GameObject FXObj = EffectsLoader.Instance.ImportEffect(Name);
            if (FXObj == null)
            {
                return null;
            }
            else 
            {
                EffectsList = new List<PhxEffect>();
                PhxEffect NewEffect = new PhxEffect(FXObj);
                EffectsList.Add(NewEffect);

                Effects[NameHash] = EffectsList;

                return NewEffect;
            }    
        }      
    }


    // Fire and forget i.e. impact sparks, explosions
    public void PlayEffectOnce(string Name, Vector3 position, Quaternion rotation)
    {
        PhxEffect Effect = GetFreeEffect(Name);

        if (Effect == null)
        {
            Debug.LogWarningFormat("Failed to play effect: {0}", Name);
        }

        Effect.SetLocalTransform(position, rotation);
        Effect.Play();
    }


    public PhxEffect LendEffect(string Name)
    {
        PhxEffect Effect = GetFreeEffect(Name);
        if (Effect == null)
        {
            Debug.LogWarningFormat("Failed to lend effect: {0}", Name);
        }

        return Effect;
    }
}
