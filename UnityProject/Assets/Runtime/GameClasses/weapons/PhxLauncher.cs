using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxLauncher : PhxGenericWeapon<PhxLauncher.ClassProperties>
{
    public new class ClassProperties : PhxGenericWeapon<ClassProperties>.ClassProperties
    {
        
    }

    public override void Init()
    {
        base.Init();
    }

    public override PhxAnimWeapon GetAnimInfo()
    {
        PhxAnimWeapon info = base.GetAnimInfo();
        info.SupportsAlert = false;
        return info;
    }
}
