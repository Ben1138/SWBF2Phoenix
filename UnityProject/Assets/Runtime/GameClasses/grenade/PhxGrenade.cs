using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxGrenade : PhxInstance<PhxSoldier.ClassProperties>
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(true);
    }


    public override void Init()
    {
        // constructor
    }

    public override void BindEvents()
    {

    }

    void Update()
    {

    }
}
