using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;





public class PhxGrenade : PhxGenericWeapon
{
    public class ClassProperties : PhxGenericWeapon.ClassProperties
    {
        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(true);

        public ClassProperties()
        {
            AnimationBank = new PhxProp<string>("grenade");
        }
    }
}





/*
public class PhxGrenade : PhxInstance<PhxGrenade.ClassProperties>, IPhxWeapon
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(true);

        public ClassProperties()
        {
            AnimationBank = new PhxProp<string>("grenade");
        }
    }


    public override void Init()
    {
        // Hide grenade mesh in hand
        gameObject.layer = LayerMask.NameToLayer("OrdnanceAll");

        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public List<Collider> GetIgnoredColliders()
    {
        return null;
    }

    public void SetFirePoint(Transform Point)
    {
        //FirePoint = FP;
    }

    public virtual void GetFirePoint(out Vector3 Pos, out Quaternion Rot)
    {
        Pos = Vector3.zero;
        Rot = Quaternion.identity;
    }

    public virtual Transform GetFirePoint()
    {
        //return FirePoint;
        return null;
    }



    public void SetIgnoredColliders(List<Collider> Colliders){}

    public override void BindEvents()
    {

    }

    public PhxInstance GetInstance()
    {
        return this;
    }

    public bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        return true;   
    }

    public void OnShot(Action callback)
    {
        
    }

    public string GetAnimBankName()
    {
        return null;
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void TickPhysics(float deltaTime)
    {

    }

    public void Reload()
    {
        
    }

    public void OnReload(Action callback)
    {
        
    }

    public int GetMagazineSize()
    {
        return 0;
    }

    public int GetTotalAmmo()
    {
        return 0;
    }

    public int GetMagazineAmmo()
    {
        return 0;
    }

    public int GetAvailableAmmo()
    {
        return 0;
    }

    public float GetReloadTime()
    {
        return 0f;
    }

    public float GetReloadProgress()
    {
        return 0f;
    }

    public bool IsFiring() 
    {
        return false;
    }

    public PhxPawnController GetOwnerController()
    {
        return null;
    }
}
*/
