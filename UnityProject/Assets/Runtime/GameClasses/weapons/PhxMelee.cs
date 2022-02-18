using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PhxMelee : PhxGenericWeapon<PhxMelee.ClassProperties>
{
    public new class ClassProperties : PhxGenericWeapon<ClassProperties>.ClassProperties
    {
        public PhxProp<float> LightSaberLength = new PhxProp<float>(1.0f);
        public PhxProp<float> LightSaberWidth = new PhxProp<float>(0.08f);
        public PhxProp<Texture2D> LightSaberTexture = new PhxProp<Texture2D>(null);
        public PhxProp<Color> LightSaberTrailColor = new PhxProp<Color>(Color.white);
    }

    protected string WeaponAnimBankName = "sabre";

    static Material LightsaberMat;
    LineRenderer Blade;

    public override void Init()
    {
        if (transform.childCount > 0)
        {
            FirePoint = transform.GetChild(0).Find(C.FirePointName);
        }

        if (C.ComboAnimationBank.Values.Count > 0)
        {
            WeaponAnimBankName = C.ComboAnimationBank.Get<string>(0).Split('_')[1];
        }

        if (LightsaberMat == null)
        {
            LightsaberMat = Resources.Load<Material>("PhxMaterial_lightsabre");
        }

        Blade = FirePoint.gameObject.AddComponent<LineRenderer>();
        Blade.shadowCastingMode = ShadowCastingMode.Off;
        Blade.lightProbeUsage = LightProbeUsage.Off;
        Blade.textureMode = LineTextureMode.DistributePerSegment;
        Blade.useWorldSpace = false;
        Blade.startWidth = C.LightSaberWidth;
        Blade.endWidth = C.LightSaberWidth;
        Blade.positionCount = 4;

        const float tipLength = 0.2f;
        Blade.SetPosition(0, new Vector3(0f, 0f, C.LightSaberLength));
        Blade.SetPosition(1, new Vector3(0f, 0f, C.LightSaberLength - tipLength));
        Blade.SetPosition(2, Vector3.zero);
        Blade.SetPosition(3, Vector3.zero);

        Blade.material = LightsaberMat;
        Blade.material.SetTexture("_UnlitColorMap", C.LightSaberTexture);
        Blade.material.SetTexture("_EmissiveColorMap", C.LightSaberTexture);
        Blade.material.SetInt("_UseEmissiveIntensity", 0);
        Blade.material.SetColor("_EmissiveColor", C.LightSaberTrailColor.Get() * Mathf.Pow(8.5f, 2.71828f));
        Blade.material.SetFloat("_EmissiveExposureWeight", 0.0f);

        GameObject pointLightGO = new GameObject("LightsaberLight");
        pointLightGO.transform.SetParent(FirePoint);
        pointLightGO.transform.localPosition = new Vector3(0f, 0f, C.LightSaberLength / 2f);

        var pointLight = pointLightGO.AddHDLight(HDLightTypeAndShape.Point);
        pointLight.color = C.LightSaberTrailColor;
        pointLight.intensity = 1e+07f;
        pointLight.range = 5f;
    }

    public override void Destroy()
    {
        
    }

    public PhxInstance GetInstance()
    {
        return this;
    }
    public override bool Fire(PhxPawnController owner, Vector3 targetPos)
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
    public override string GetAnimBankName()
    {
        return WeaponAnimBankName;
    }

    public void SetFirePoint(Transform FirePoint)
    {

    }

    public override Transform GetFirePoint()
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