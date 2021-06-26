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

    	if (healthPercent <= DamageStartPercent && healthPercent > DamageStopPercent)
    	{
    		if (!Effect.IsPlaying)
    		{
    			Effect.Play();
    		}  

    		IsOn = true;  		
    	}
    	else 
    	{
    		if (Effect.IsPlaying)
    		{
    			Effect.Stop();
    		}

    		IsOn = false;
    	}
    }


    void Init()
    {
    	if (DamageAttachPoint != null && Effect != null)
    	{
    		Effect.SetParent(DamageAttachPoint);
    		Effect.SetLocalTransform(Vector3.zero, Quaternion.identity);
    		Effect.SetLooping(true);
    		Effect.Stop();
    	}

    	IsInitialized = true;
    }
}