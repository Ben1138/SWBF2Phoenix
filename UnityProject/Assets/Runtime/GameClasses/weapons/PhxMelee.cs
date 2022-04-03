using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PhxMelee : PhxGenericWeapon<PhxMelee.ClassProperties>
{
    public new class ClassProperties : PhxGenericWeapon<ClassProperties>.ClassProperties
    {
        public PhxProp<int> NumDamageEdges = new PhxProp<int>(0);
        public PhxImpliedSection LightSabers = new PhxImpliedSection(
            ("FirePointName", new PhxProp<string>("")),
            ("LightSaberLength", new PhxProp<float>(1f)),
            ("LightSaberWidth", new PhxProp<float>(1f)),
            ("LightSaberTexture", new PhxProp<Texture2D>(null)),
            ("LightSaberTrailColor", new PhxProp<Color>(Color.white))
        );

        // TODO: AttachedFirePoint (see cis_weap_doublesaber.odf)
    }

    List<Transform> Edges = new List<Transform>();

    static Material LightsaberMat;
    LineRenderer Blade;
    AudioSource SwingAudio;

    public override void Init()
    {
        base.Init();
        RemoveWeaponCollision();

        if (LightsaberMat == null)
        {
            LightsaberMat = Resources.Load<Material>("PhxMaterial_lightsabre");
        }

        foreach (Dictionary<string, IPhxPropRef> section in C.LightSabers)
        {
            CreateBlade(section);
        }

        AudioClip FireSound = SoundLoader.Instance.LoadSound("saber_idle02");
        if (FireSound != null)
        {
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.playOnAwake = true;
            Audio.spatialBlend = 1.0f;
            Audio.rolloffMode = AudioRolloffMode.Linear;
            Audio.minDistance = 2.0f;
            Audio.maxDistance = 30.0f;
            Audio.loop = true;
            Audio.clip = FireSound;
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    public override bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        return false;
    }

    public override PhxAnimWeapon GetAnimInfo()
    {
        PhxAnimWeapon info = base.GetAnimInfo();
        info.SupportsAlert = false;
        info.SupportsReload = false;
        return info;
    }

    public Transform GetEdge(int idx)
    {
        if (idx >= 0 && idx < Edges.Count)
        {
            return Edges[idx];
        }
        return null;
    }

    void CreateBlade(Dictionary<string, IPhxPropRef> bladeProps)
    {
        PhxProp<string> firePointName = bladeProps["FirePointName"] as PhxProp<string>;
        PhxProp<float> lightSaberWidth = bladeProps["LightSaberWidth"] as PhxProp<float>;
        PhxProp<float> lightSaberLength = bladeProps["LightSaberLength"] as PhxProp<float>;
        PhxProp<Texture2D> lightSaberTexture = bladeProps["LightSaberTexture"] as PhxProp<Texture2D>;
        PhxProp<Color> lightSaberTrailColor = bladeProps["LightSaberTrailColor"] as PhxProp<Color>;

        Transform firePoint = transform.GetChild(0).Find(firePointName);
        if (firePoint == null)
        {
            Debug.LogError($"Couldn't find '{firePointName}' for lightsaber blade!");
            return;
        }
        Edges.Add(firePoint);

        Blade = firePoint.gameObject.AddComponent<LineRenderer>();
        Blade.shadowCastingMode = ShadowCastingMode.Off;
        Blade.lightProbeUsage = LightProbeUsage.Off;
        Blade.textureMode = LineTextureMode.DistributePerSegment;
        Blade.useWorldSpace = false;
        Blade.startWidth = lightSaberWidth;
        Blade.endWidth = lightSaberWidth;
        Blade.positionCount = 4;

        const float tipLength = 0.2f;
        Blade.SetPosition(0, new Vector3(0f, 0f, lightSaberLength));
        Blade.SetPosition(1, new Vector3(0f, 0f, lightSaberLength - tipLength));
        Blade.SetPosition(2, Vector3.zero);
        Blade.SetPosition(3, Vector3.zero);

        Blade.material = LightsaberMat;
        Blade.material.SetTexture("_UnlitColorMap", lightSaberTexture);
        Blade.material.SetTexture("_EmissiveColorMap", lightSaberTexture);
        Blade.material.SetInt("_UseEmissiveIntensity", 0);
        Blade.material.SetColor("_EmissiveColor", lightSaberTrailColor.Get() * Mathf.Pow(2f, 8.5f));
        Blade.material.SetFloat("_EmissiveExposureWeight", 0.0f);

        GameObject pointLightGO = new GameObject("LightsaberLight");
        pointLightGO.transform.SetParent(firePoint);
        pointLightGO.transform.localPosition = new Vector3(0f, 0f, lightSaberLength / 2f);

        var pointLight = pointLightGO.AddHDLight(HDLightTypeAndShape.Point);
        pointLight.color = lightSaberTrailColor;
        pointLight.intensity = 1e+07f;
        pointLight.range = 5f;
    }
}