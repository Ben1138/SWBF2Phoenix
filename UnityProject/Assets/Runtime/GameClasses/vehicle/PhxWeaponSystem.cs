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



MAYBE PhxGenericWeapon should just read from assigned aimers and PhxSeat + PhxSoldier
should assign and maintain aimers?
*/

public class PhxWeaponSystem
{
    static PhxScene SCENE => PhxGame.GetScene();

    public IPhxWeapon Weapon;
    Transform WeaponTransform;

    PhxSeat OwnerSeat; 

    List<PhxAimer> Aimers;

    public List<Collider> IgnoredColliders;


    public PhxWeaponSystem(PhxSeat Section)
    {
        OwnerSeat = Section;
        Aimers = new List<PhxAimer>();
    }



    public void InitManual(EntityClass EC, int StartIndex, string WeaponIndex = "1")
    {
        EC.GetAllProperties(out uint[] properties, out string[] values);

        PhxAimer CurrAimer = new PhxAimer();
        PhxBarrel CurrBarrel = null;

        int i = StartIndex;


        while (i < properties.Length)
        {
            if (properties[i] == 0x407e801e /*HierarchyLevel*/)
            {
                CurrAimer.HierarchyLevel = Int32.Parse(values[i]);
            }
            else if (properties[i] == 0x2d6487bd /*AimerPitchLimits*/)
            {
                CurrAimer.PitchRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == 0xa9c3675c /*AimerYawLimits*/)
            {
                CurrAimer.YawRange = PhxUtils.Vec2FromString(values[i]);
            }
            else if (properties[i] == 0x1e534b12 /*BarrelNodeName*/)
            {
                if (CurrBarrel == null)
                {
                    CurrBarrel = new PhxBarrel();
                    CurrAimer.AddBarrel(CurrBarrel);
                }
                CurrBarrel.Node = UnityUtils.FindChildTransform(OwnerSeat.Owner.GetRootTransform(), values[i]);
            } 
            else if (properties[i] == 0x5a29d1cb /*BarrelRecoil*/)
            {
                if (CurrBarrel != null)
                {
                    CurrBarrel.RecoilDistance = PhxUtils.FloatFromString(values[i]);
                }
            }     
            else if (properties[i] == 0xc182abc2 /*AimerNodeName*/)
            {
                CurrAimer.Node = UnityUtils.FindChildTransform(OwnerSeat.Owner.GetRootTransform(), values[i]);
            }
            else if (properties[i] == 0x4274fc96 /*FirePointName*/ || properties[i] == 0x2e5375da /*FireNodeName*/)
            {
                Transform FirePoint = UnityUtils.FindChildTransform(OwnerSeat.Owner.GetRootTransform(), values[i]);
                if (CurrBarrel != null)
                {
                    CurrBarrel.FirePoint = FirePoint;
                }
                else 
                {
                    CurrAimer.FireNode = FirePoint;
                }
            }
            else if (properties[i] == 0xe98f377c /*NextBarrel*/)
            {
                CurrBarrel = new PhxBarrel();
                CurrAimer.AddBarrel(CurrBarrel);
            }
            else if (properties[i] == 0x665d96ea /*NextAimer*/)
            {
                AddAimer(CurrAimer);
                CurrAimer = new PhxAimer();
                CurrBarrel = null;
            }
            else if (properties[i] == 0xfbf47dba /*WeaponName*/)
            {
                SetWeapon(values[i]);
            }  
            else if (properties[i] == 0xd0329e80 /*WEAPONSECTION*/)
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
        try 
        {
        	Weapon = string.IsNullOrEmpty(WeaponName) ? null : SCENE.CreateInstance(SCENE.GetClass(WeaponName), false) as IPhxWeapon;

        	if (Weapon == null)
        	{
        		Debug.LogErrorFormat("Couldn't get weapon class: {0}", WeaponName);
        	}
        	else 
        	{
        		WeaponTransform = Weapon.GetInstance().gameObject.transform;
                WeaponTransform.SetParent(OwnerSeat.Owner.GetRootTransform());
                WeaponTransform.localPosition = Vector3.zero;
                WeaponTransform.localRotation = Quaternion.identity;
                Weapon.SetIgnoredColliders(IgnoredColliders);
        	}
        }            
        catch 
        {
            Debug.LogErrorFormat("Failed to assign weapon to weapon system {0}", WeaponName == null ? "" : WeaponName);
        }
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

        do 
        {
            FirePointIndex = (FirePointIndex + 1) % FirePoints.Count;

        } while (FirePoints[FirePointIndex] == null);
        
        Weapon?.SetFirePoint(FirePoints[FirePointIndex]);
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
                Weapon?.SetFirePoint(FirePoints[0]);
            }

            Weapon?.OnShot(AimerFire);

            WeaponFirePointsSet = true;
        }


        if (Aimers.Count > 0 && WeaponTransform != null)
        {
            if (Weapon == null) return false;

        	if (Weapon.Fire(OwnerSeat.Occupant.GetController(), Target))
        	{
                return true;
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


