using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;



public class PhxWeapon : PhxClass
{         
    public PhxProp<float> ShotDelay     = new PhxProp<float>(0.2f);
    public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
    public PhxProp<int>   SalvoCount    = new PhxProp<int>(1);
    public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

    public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

    public PhxProp<PhxClass> OrdnanceName = new PhxProp<PhxClass>(null);

    // Sound
    public PhxProp<float>  PitchSpread = new PhxProp<float>(0.1f);
    public PhxProp<string> FireSound = new PhxProp<string>(null);

    public PhxProp<float> HeatRecoverRate = new PhxProp<float>(0.25f);
    public PhxProp<float> HeatThreshold = new PhxProp<float>(0.2f); 
    public PhxProp<float> HeatPerShot = new PhxProp<float>(0.12f);
}


/*
classlabel = weapon

Equivalent to WEAPONSECTIONs in vehicles.  Maintains aimers, barrels, firing configurations,
and fires weapons.
*/


public class PhxWeaponSystem
{
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    PhxClass Weapon;
    public List<PhxAimer> Aimers;

    List<Transform> Barrels;
    List<Transform> FirePoints;

    int CurrentAimer;

    float ShotTimer;


    public void AddAimer(PhxAimer Aimer)
    {
    	Aimer.Init();
        if (Aimers.Count > 0 && Aimers.Last().HierarchyLevel > Aimer.HierarchyLevel)
        {
            Aimers[Aimers.Count - 1].ChildAimer = Aimer;
        }
        else 
        {
            Aimers.Add(Aimer);
        }
    }


    //public void SetBarrelRecoil( )

    public void SetWeapon(string WeaponName)
    {
    	Weapon = SCENE.GetClass(WeaponName);
    	if (Weapon == null)
    	{
    		Debug.LogErrorFormat("Couldn't get weapon class: {0}", WeaponName);
    	}
        //Weapon = PhxGameRuntime.GetScene().GetClass(WeaponName) as PhxWeapon;
        //ShotTimer = Weapon.ShotDelay;
    }


    public PhxWeaponSystem()
    {
        Aimers = new List<PhxAimer>();
    }


    public void AdjustAimers(Vector3 Target)
    {
        foreach (PhxAimer Aimer in Aimers)
        {
            Aimer.AdjustAim(Target);
        }
    }


    private void EmitShot(Vector3 pos, Vector3 dir)
    {
        /*
        if (C.OrdnanceName.Get() != null)
        {
            PhxBolt bolt = C.OrdnanceName.Get() as PhxBolt;
            if (bolt != null)
            {
                Quaternion dir = Quaternion.LookRotation(dir);
                Scene.FireProjectile(Occupant.GetController(), pos, dir, bolt);
                //Debug.DrawRay(HpFire.position, dirVec * 1000f, Color.red);
            }
        }
        */
    }


    int CurrBarrel;
    int CurrAimer;

    public bool Fire()
    {
        if (Aimers.Count > 0)
        {
            if (Aimers[CurrAimer].Fire())
            {
                CurrAimer++;
            }

            if (CurrAimer >= Aimers.Count)
            {
                CurrAimer = 0;
            }
        }

        return true;
    }


    public void Update(Vector3 TargetPos)
    {
        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos, false);
            Aimer.UpdateBarrel();
        }
    }
}


