using System;
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
    PhxWeapon Weapon;
    List<PhxAimer> Aimers;

    List<Transform> Barrels;
    List<Transform> FirePoints;

    int CurrentAimer;

    float ShotTimer;


    public void AddAimer(PhxAimer Aimer)
    {
        Aimers.Add(Aimer);
    }

    public void AddBarrel(Transform Barrel)
    {
        Barrels.Add(Barrel);
    }

    //public void SetBarrelRecoil( )

    public void SetWeapon(string WeaponName)
    {
        Weapon = (PhxWeapon) PhxGameRuntime.GetScene().GetClass(WeaponName);
        ShotTimer = Weapon.ShotDelay;
    }


    public PhxWeaponSystem(){}


    public void AdjustAimers(Vector3 Target)
    {
        foreach (PhxAimer Aimer in Aimers)
        {
            Aimer.AdjustAim(Target);
        }
    }


    int CurrBarrel;
    public bool Fire()
    {
        if (ShotTimer <= 0f)
        {
            //if (Audio != null)
            //{
                //float half = C.PitchSpread / 2f;
                //Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                //Audio.Play();
            //}


            for (int i = 0; i < Weapon.ShotsPerSalvo; i++)
            {
                CurrBarrel = CurrBarrel % FirePoints.Count;
                //Emit from barrel...
                CurrBarrel++;
            }

            //ShotCallback?.Invoke();
            ShotTimer = Weapon.ShotDelay;
        }

        return false;
    }


    void Update()
    {
        if (ShotTimer > 0.0f)
        {
            ShotTimer -= Time.deltaTime;
        }
    }
}


