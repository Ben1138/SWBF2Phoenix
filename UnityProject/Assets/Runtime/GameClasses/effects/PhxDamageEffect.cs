using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


public class PhxDamageEffect 
{
    public float DamageStartPercent;
    public float DamageStopPercent;
    public PhxEffect Effect;
    public Transform DamageAttachPoint;

    public bool IsOn;

    public void Update(float health)
    {
    	if (health < DamageStartPercent && health > DamageStopPercent)
    	{
    		if (!Effect.IsPlaying)
    		{
    			Effect.Play();
    		}    		
    	}
    	else 
    	{
    		if (Effect.IsPlaying)
    		{
    			Effect.Stop();
    		}
    	}
    }
}