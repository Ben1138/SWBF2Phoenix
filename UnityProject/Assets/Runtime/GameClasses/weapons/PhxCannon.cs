using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;



/*
Don't know how cannons differ from weapons yet.  
*/

public class PhxCannon : PhxGenericWeapon, IPhxWeapon
{
    public class ClassProperties : PhxGenericWeapon.ClassProperties
    {
        public PhxProp<int> MedalsTypeToUnlock = new PhxProp<int>(0);
        public PhxProp<int> ScoreForMedalsType = new PhxProp<int>(0);
    } 

    PhxCannon.ClassProperties CannonClass;


    public override void Init()
    {
        base.Init();

        CannonClass = C as PhxCannon.ClassProperties;
    }
}