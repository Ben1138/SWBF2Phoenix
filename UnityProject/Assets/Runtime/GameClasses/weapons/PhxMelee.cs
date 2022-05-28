using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class PhxMelee : PhxGenericWeapon<PhxMelee.ClassProperties>
{
    // NOTE: Number of damage edges != number of blades!
    // For example, darth maul's third damage edge is a foot kick

    public new class ClassProperties : PhxGenericWeapon<ClassProperties>.ClassProperties
    {
        public PhxProp<int> NumDamageEdges = new PhxProp<int>(1);
        public PhxImpliedSection LightSabers = new PhxImpliedSection(
            ("OffhandGeometryName", new PhxProp<string>("")),
            ("OffhandFirePointName", new PhxMultiProp(typeof(string), typeof(string))),
            ("FirePointName", new PhxProp<string>("hp_fire")),
            ("LightSaberLength", new PhxProp<float>(1f)),
            ("LightSaberWidth", new PhxProp<float>(1f)),
            ("LightSaberTexture", new PhxProp<Texture2D>(null)),
            ("LightSaberTrailColor", new PhxProp<Color>(Color.white))
        );

        public PhxImpliedSection AttachedEdges = new PhxImpliedSection(
            ("AttachedFirePoint", new PhxMultiProp(typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float))),
            ("DamageEdgeLength", new PhxProp<float>(1f)),
            ("DamageEdgeWidth", new PhxProp<float>(1f))
        );
    }

    List<Transform> Edges = new List<Transform>();

    static Material LightsaberMat;
    LineRenderer Blade;
    AudioSource SwingAudio;
    Dictionary<uint, AudioClip> SwingSounds = new Dictionary<uint, AudioClip>();

    public override void Init()
    {
        base.Init();
        RemoveWeaponCollision();

        if (LightsaberMat == null)
        {
            LightsaberMat = Resources.Load<Material>("PhxMaterial_lightsabre");
        }

        AudioClip idleSound = SoundLoader.Instance.LoadSound("saber_idle02");
        if (idleSound != null)
        {
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.playOnAwake = true;
            Audio.spatialBlend = 1.0f;
            Audio.rolloffMode = AudioRolloffMode.Linear;
            Audio.minDistance = 2.0f;
            Audio.maxDistance = 30.0f;
            Audio.loop = true;
            Audio.clip = idleSound;
        }

        SwingAudio = gameObject.AddComponent<AudioSource>();
        SwingAudio.playOnAwake = false;
        SwingAudio.spatialBlend = 1.0f;
        SwingAudio.rolloffMode = AudioRolloffMode.Linear;
        SwingAudio.minDistance = 2.0f;
        SwingAudio.maxDistance = 30.0f;
        SwingAudio.loop = false;
        SwingAudio.clip = null;
    }

    public void CreateSabers()
    {
        int i = 0;
        foreach (Dictionary<string, IPhxPropRef> section in C.LightSabers)
        {
            CreateBlade(section, i++);
        }
    }

    public void PlaySwingSound(uint soundHash)
    {
        if (soundHash == 0)
        {
            return;
        }

        if (!SwingSounds.TryGetValue(soundHash, out AudioClip swingSound))
        {
            swingSound = SoundLoader.Instance.LoadSound(soundHash);
            if (swingSound == null)
            {
                Debug.LogError($"Couldn't find swing sound from hash: {soundHash}!");
                return;
            }
            SwingSounds.Add(soundHash, swingSound);
        }
        SwingAudio.Stop();
        SwingAudio.PlayOneShot(swingSound);
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

    public void Attack(int edgeIdx, string swingSound)
    {

    }

    public Transform GetEdge(int idx)
    {
        if (idx >= 0 && idx < Edges.Count)
        {
            return Edges[idx];
        }
        return null;
    }

    void CreateBlade(Dictionary<string, IPhxPropRef> bladeProps, int bladeIdx)
    {
        var firePointName         = bladeProps["FirePointName"]        as PhxProp<string>;
        var offhandFirePointName  = bladeProps["OffhandFirePointName"] as PhxMultiProp;
        var offhandGeometryName   = bladeProps["OffhandGeometryName"]  as PhxProp<string>;
        var lightSaberWidth       = bladeProps["LightSaberWidth"]      as PhxProp<float>;
        var lightSaberLength      = bladeProps["LightSaberLength"]     as PhxProp<float>;
        var lightSaberTexture     = bladeProps["LightSaberTexture"]    as PhxProp<Texture2D>;
        var lightSaberTrailColor  = bladeProps["LightSaberTrailColor"] as PhxProp<Color>;

        string hpWeaponName = "hp_weapons";
        string hpFireName   = firePointName;
        if (!string.IsNullOrEmpty(offhandGeometryName))
        {
            // Seems to be the default when no weapon bone is defined.
            // See: rep_weap_lightsaber_aalya.odf
            hpWeaponName = "bone_l_hand";
        }

        if (offhandFirePointName.Values.Count > 0)
        {
            string ohfpn0 = offhandFirePointName.Values[bladeIdx][0] as string;
            if (!string.IsNullOrEmpty(ohfpn0))
            {
                hpFireName = ohfpn0;
            }
        }
        if (offhandFirePointName.Values.Count > 1)
        {
            string ohfpn1 = offhandFirePointName.Values[bladeIdx][1] as string;
            if (!string.IsNullOrEmpty(ohfpn1))
            {
                hpWeaponName = ohfpn1;
            }
        }

        Transform hpWeapon = PhxUtils.FindTransformRecursive(OwnerSkeletonRoot, hpWeaponName);
        if (hpWeapon == null)
        {
            Debug.LogError($"Couldn't find '{hpWeaponName}' parent bone for melee weapon in '{OwnerSkeletonRoot}'!");
            return;
        }

        if (!string.IsNullOrEmpty(offhandGeometryName))
        {
            GameObject SaberModel = ModelLoader.Instance.GetGameObjectFromModel(offhandGeometryName);
            SaberModel.transform.SetParent(hpWeapon);
            SaberModel.transform.localPosition = Vector3.zero;
            SaberModel.transform.localRotation = Quaternion.identity;
            SaberModel.transform.localScale    = new Vector3(1f, 1f, 1f);
        }

        Transform firePoint = PhxUtils.FindTransformRecursive(hpWeapon, hpFireName);
        if (firePoint == null)
        {
            Debug.LogError($"Couldn't find '{hpFireName}' for lightsaber blade!");
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