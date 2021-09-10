using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhxDamageEffect 
{
    public float DamageStartPercent;
    public float DamageStopPercent;
    public PhxEffect Effect;
    public Transform DamageAttachPoint;

    public bool IsOn;

    bool IsInitialized;

    public void Update(float healthPercent)
    {
    	if (!IsInitialized)
    	{
    		Init();
    	}

        if (Effect != null)
        {
        	if (healthPercent <= DamageStartPercent && healthPercent > DamageStopPercent)
        	{
        		if (!Effect.IsPlaying)
        		{
        			//Debug.LogFormat("Playing damage effect {0}", Effect.EffectObject.name);
        			Effect.Play();
        		}  

        		IsOn = true;  		
        	}
        	else 
        	{
        		if (Effect.IsPlaying)
        		{
                    //Debug.LogFormat("Stopping damage effect {0}", Effect.EffectObject.name);
        			Effect.Stop();
        		}

        		IsOn = false;
        	}
        }
    }


    void Init()
    {
    	if (DamageAttachPoint != null && Effect != null)
    	{
    		Effect.SetParent(DamageAttachPoint);
    		Effect.SetLocalTransform(Vector3.zero, Quaternion.identity);
    		Effect.SetLooping(true);
            Effect.SetDynamic(true);
    		Effect.Stop();
    	}

    	IsInitialized = true;
    }
}