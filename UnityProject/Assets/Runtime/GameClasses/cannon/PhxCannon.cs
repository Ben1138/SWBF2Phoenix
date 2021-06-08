using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxCannon : PhxInstance<PhxCannon.ClassProperties>
{
    public bool Fire;

    public Action Shot;

    float FireDelay;

    public class ClassProperties : PhxClass
    {
        public PhxProp<string> AnimationBank = new PhxProp<string>("rifle");
        public PhxProp<float>  ShotDelay = new PhxProp<float>(0.2f);
    }


    public override void Init()
    {
        // constructor
        // Use this to create required Unity components (like AudioSource, SpotLight, Rigidbody, etc...)
    }

    public override void BindEvents()
    {
        
    }

    void Update()
    {
        FireDelay -= Time.deltaTime;
        if (FireDelay <= 0f && Fire)
        {
            Shot?.Invoke();
            FireDelay = C.ShotDelay;
        }
    }
}
