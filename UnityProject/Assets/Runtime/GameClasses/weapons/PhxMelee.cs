using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxMelee : PhxInstance<PhxMelee.ClassProperties>, IPhxWeapon
{
    public class ClassProperties : PhxClass
    {
        // TODO: This probably needs to go to PhxGenericWeapon.ClassProperties
        public PhxMultiProp ComboAnimationBank = new PhxMultiProp(typeof(string), typeof(string), typeof(string));
        //public PhxMultiProp CustomAnimationBank = new PhxMultiProp(typeof(string), typeof(string), typeof(string));
    }

    protected Transform FirePoint;
    protected string WeaponAnimBankName = "sabre";

    public override void Init()
    {
        if (transform.childCount > 0)
        {
            FirePoint = transform.GetChild(0).Find("hp_fire");
        }

        if (C.ComboAnimationBank.Values.Count > 0)
        {
            WeaponAnimBankName = C.ComboAnimationBank.Get<string>(0).Split('_')[1];
        }
    }

    public override void Destroy()
    {
        
    }

    public PhxInstance GetInstance()
    {
        return this;
    }
    public bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        return false;
    }

    public void Reload()
    {

    }
    public void OnShot(Action callback)
    {

    }
    public void OnReload(Action callback)
    {

    }
    public string GetAnimBankName()
    {
        return WeaponAnimBankName;
    }

    public void SetFirePoint(Transform FirePoint)
    {

    }

    public Transform GetFirePoint()
    {
        return FirePoint;
    }
    public void GetFirePoint(out Vector3 Pos, out Quaternion Rot)
    {
        Pos = FirePoint.position;
        Rot = FirePoint.rotation;
    }


    public void SetIgnoredColliders(List<Collider> Colliders)
    {

    }
    public List<Collider> GetIgnoredColliders()
    {
        return null;
    }

    public PhxPawnController GetOwnerController()
    {
        return null;
    }
    public bool IsFiring() { return false; }

    public int GetMagazineSize() { return 0; }
    public int GetTotalAmmo() { return 0; }
    public int GetMagazineAmmo() { return 0; }
    public int GetAvailableAmmo() { return 0; }
    public float GetReloadTime() { return 0f; }
    public float GetReloadProgress() { return 1f; }
}