using System.Collections.Generic;
using UnityEngine;

using LibSWBF2.Utils;



public class PhxEffectsManager
{
    PhxGame Game => PhxGame.Instance;

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
            if (EffectsList.Count < 20)
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
                NewEffect.SetLooping(false);
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
            return;
        }

        Effect.SetLooping(false);

        Effect.SetLocalTransform(position, rotation);
        Effect.Play();
    }


    public PhxEffect LendEffect(string Name)
    {
        PhxEffect Effect = GetFreeEffect(Name);
        if (Effect == null)
        {
            Debug.LogWarningFormat("Failed to lend effect: {0}", Name);
            return null;
        }
        Effect.SetLooping(false);

        return Effect;
    }
}
