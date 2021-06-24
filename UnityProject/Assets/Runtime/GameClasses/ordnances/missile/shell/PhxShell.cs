using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

using LibSWBF2.Enums;


/*
Not sure how shells differ from missiles yet,
but they appear to use gravity, while missiles don't.

I suppose they also can't heatseek
*/


public class PhxShellClass : PhxMissileClass
{
    // Probably in Gs
    public PhxProp<float> Gravity = new PhxProp<float>(1f);
}



[RequireComponent(typeof(Rigidbody), typeof(Light))]
public class PhxShell : PhxMissile
{
    PhxShellClass ShellClass;

    public override void Init(PhxOrdnanceClass OClass)
    {
        base.Init(OClass);
        ShellClass = OClass as PhxShellClass;
    }


    // Only differs in that it applies gravity
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Manual gravity, since it varies
        Body.AddForce(9.8f * ShellClass.Gravity * Vector3.down, ForceMode.Acceleration);
    }
}
