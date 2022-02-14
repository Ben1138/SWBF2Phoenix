using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;





public class PhxGrenade : PhxGenericWeapon
{
    public class ClassProperties : PhxGenericWeapon.ClassProperties
    {
    	// Dont know what goes here for sure yet, grenade anim bank is not used
        public ClassProperties()
        {
            //OffhandWeapon = new PhxProp<bool>(true);
        }
    }
}
