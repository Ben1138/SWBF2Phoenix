using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxCannon : PhxGenericWeapon<PhxCannon.ClassProperties>
{
    public new class ClassProperties : PhxGenericWeapon<ClassProperties>.ClassProperties
    {
        public PhxProp<int> MedalsTypeToUnlock = new PhxProp<int>(0);
        public PhxProp<int> ScoreForMedalsType = new PhxProp<int>(0);
    }

    public override void Init()
    {
        base.Init();
    }
}