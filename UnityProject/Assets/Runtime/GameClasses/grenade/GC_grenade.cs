using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class GC_grenade : ISWBFInstance<GC_soldier.ClassProperties>
{
    public class ClassProperties : ISWBFClass
    {
        public Prop<bool> OffhandWeapon = new Prop<bool>(true);
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
