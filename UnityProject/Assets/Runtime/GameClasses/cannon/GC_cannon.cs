using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class GC_cannon : ISWBFInstance<GC_soldier.ClassProperties>
{
    public class ClassProperties : ISWBFClass
    {
        public Prop<string> AnimationBank = new Prop<string>("rifle");
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
