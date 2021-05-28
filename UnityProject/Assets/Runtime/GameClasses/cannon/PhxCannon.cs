using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxCannon : PhxInstance<PhxSoldier.ClassProperties>
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<string> AnimationBank = new PhxProp<string>("rifle");
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

    }
}
