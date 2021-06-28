using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxGrenade : PhxInstance<PhxGrenade.ClassProperties>, IPhxWeapon
{
    public class ClassProperties : PhxClass
    {
        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(true);
    }


    public override void Init()
    {
        MeshCollider collision = GetComponent<MeshCollider>();
        if (collision != null)
        {
            collision.convex = true;
        }
    }

    public override void BindEvents()
    {

    }

    public PhxInstance GetInstance()
    {
        return this;
    }

    public void Fire()
    {
        
    }

    public void OnShot(Action callback)
    {
        
    }

    public string GetAnimBankName()
    {
        return null;
    }

    void Update()
    {

    }

    void IPhxWeapon.Reload()
    {
        
    }

    void IPhxWeapon.OnReload(Action callback)
    {
        
    }

    int IPhxWeapon.GetMagazineSize()
    {
        return 0;
    }

    int IPhxWeapon.GetTotalAmmo()
    {
        return 0;
    }

    int IPhxWeapon.GetMagazineAmmo()
    {
        return 0;
    }

    public int GetAvailableAmmo()
    {
        return 0;
    }

    float IPhxWeapon.GetReloadProgress()
    {
        return 0f;
    }
}
