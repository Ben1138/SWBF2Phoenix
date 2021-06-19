using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using LibSWBF2.Wrappers;
using System.Runtime.ExceptionServices;


/*
Equivalent to WEAPONSECTIONs in vehicles.  Maintains aimers, barrels, firing configurations,
and fires weapons.
*/

public class PhxWeaponSystem
{
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    IPhxWeapon Weapon;
    Transform WeaponTransform;

    PhxVehicleSection OwnerSection; 

    List<PhxAimer> Aimers;

    List<Collider> IgnoredColliders;



    public void InitManual(EntityClass EC, int StartIndex, string WeaponIndex = "1")
    {
        EC.GetAllProperties(out uint[] properties, out string[] values);

        PhxAimer CurrAimer = new PhxAimer();
        PhxBarrel CurrBarrel = null;

        int i = StartIndex;

        while (i < properties.Length)
        {
            if (properties[i] == HashUtils.GetFNV("HierarchyLevel"))
            {
                CurrAimer.HierarchyLevel = Int32.Parse(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerPitchLimits"))
            {
                CurrAimer.PitchRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("AimerYawLimits"))
            {
                CurrAimer.YawRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("BarrelNodeName"))
            {
                if (CurrBarrel == null)
                {
                    CurrBarrel = new PhxBarrel();
                    CurrAimer.AddBarrel(CurrBarrel);
                }
                CurrBarrel.Node = UnityUtils.FindChildTransform(OwnerSection.OwnerVehicle.transform, values[i]);
            } 
            else if (properties[i] == HashUtils.GetFNV("BarrelRecoil"))
            {
                if (CurrBarrel != null)
                {
                    CurrBarrel.RecoilDistance = float.Parse(values[i]);
                }
            }     
            else if (properties[i] == HashUtils.GetFNV("AimerNodeName"))
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(OwnerSection.OwnerVehicle.transform, values[i]);
            }
            else if (properties[i] == HashUtils.GetFNV("FirePointName"))
            {
                Transform FirePoint = UnityUtils.FindChildTransform(OwnerSection.OwnerVehicle.transform, values[i]);
                if (CurrBarrel != null)
                {
                    CurrBarrel.FirePoint = FirePoint;
                }
                else 
                {
                    CurrAimer.FireNode = FirePoint;
                }
            }
            else if (properties[i] == HashUtils.GetFNV("NextBarrel"))
            {
                CurrBarrel = new PhxBarrel();
                CurrAimer.AddBarrel(CurrBarrel);
            }
            else if (properties[i] == HashUtils.GetFNV("NextAimer"))
            {
                AddAimer(CurrAimer);
                CurrAimer = new PhxAimer();
                CurrBarrel = null;
            }
            else if (properties[i] == HashUtils.GetFNV("WeaponName"))
            {
                SetWeapon(values[i]);
            }  
            else if (properties[i] == HashUtils.GetFNV("WEAPONSECTION"))
            {
                if (!values[i].Equals(WeaponIndex, StringComparison.OrdinalIgnoreCase))
                {
                    AddAimer(CurrAimer);
                    break;                    
                }
            }          
            else if (properties[i] == HashUtils.GetFNV("FLYERSECTION") ||
                    properties[i] == HashUtils.GetFNV("WALKERSECTION") ||
                    //properties[i] == HashUtils.GetFNV("TURRETSECTION") ||
                    properties[i] == HashUtils.GetFNV("BUILDINGSECTION") || 
                    i == properties.Length - 1)
            {
                AddAimer(CurrAimer); 
                break;   
            }

            i++;
        }
    }






    public void AddAimer(PhxAimer Aimer)
    {
        if (Aimers.Count > 0 && Aimers.Last().HierarchyLevel > Aimer.HierarchyLevel)
        {
            Aimers[Aimers.Count - 1].ChildAimer = Aimer;
        }
        else 
        {
            Aimers.Add(Aimer);
        }
    }


    public void SetWeapon(string WeaponName)
    {
    	Weapon = SCENE.CreateInstance(SCENE.GetClass(WeaponName), false) as IPhxWeapon;
        Weapon.SetIgnoredColliders(IgnoredColliders);

    	if (Weapon == null)
    	{
    		Debug.LogErrorFormat("Couldn't get weapon class: {0}", WeaponName);
    	}
    	else 
    	{
    		WeaponTransform = Weapon.GetInstance().gameObject.transform;
    	}
    }


    public PhxWeaponSystem(PhxVehicleSection Section)
    {
    	OwnerSection = Section;
        Aimers = new List<PhxAimer>();

        IgnoredColliders = OwnerSection.OwnerVehicle.GetOrdnanceColliders();
    }


    public void AdjustAimers(Vector3 Target)
    {
        foreach (PhxAimer Aimer in Aimers)
        {
            Aimer.AdjustAim(Target);
        }
    }



    int CurrAimer;
    int FirePointIndex;

    void AimerFire()
    {
        PhxBarrel CurrBarrel = Barrels[FirePointIndex];
        if (CurrBarrel != null)
        {
            CurrBarrel.Recoil();
        }

        //Aimers[CurrAimer++].Fire();
        //CurrAimer %= Aimers.Count;
        
        FirePointIndex = (FirePointIndex + 1) % FirePoints.Count;
        Weapon.SetFirePoint(FirePoints[FirePointIndex]);
    }


    bool WeaponFirePointsSet = false;

    List<Transform> FirePoints;
    List<PhxBarrel> Barrels;

    public bool Fire(Vector3 Target)
    {
        if (!WeaponFirePointsSet)
        {
            FirePoints = new List<Transform>();
            Barrels = new List<PhxBarrel>();

            foreach (PhxAimer Aimer in Aimers)
            {
                FirePoints.AddRange(Aimer.GetFirePointSequence());
                Barrels.AddRange(Aimer.GetBarrelSequence());
            }

            if (FirePoints.Count > 0)
            {
                Weapon.SetFirePoint(FirePoints[0]);
            }

            Weapon.OnShot(AimerFire);

            WeaponFirePointsSet = true;
        }


        if (Aimers.Count > 0 && WeaponTransform != null)
        {
        	if (!Weapon.Fire(OwnerSection.Occupant.GetController(), Target))
        	{
                return false;
            }
        }

        return true;
    }


    public void Update(Vector3 TargetPos)
    {
        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos);
        }

        if (WeaponFirePointsSet)
        {
            for (int i = 0; i < Barrels.Count; i++)
            {
                if (Barrels[i] != null)
                {
                    Barrels[i].Update();
                }
            }
        }
    }
}


