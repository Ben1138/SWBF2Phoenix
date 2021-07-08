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
        // Hide grenade mesh in hand
        gameObject.layer = 6; // Projectiles

        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

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
}
