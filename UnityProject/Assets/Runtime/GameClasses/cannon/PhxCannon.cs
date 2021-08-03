using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;




public class PhxCannon : PhxInstance<PhxCannon.ClassProperties>, IPhxWeapon
{
    PhxGameRuntime Game => PhxGameRuntime.Instance;
    PhxRuntimeScene Scene => PhxGameRuntime.GetScene();

    Action ShotCallback;
    Action ReloadCallback;
    AudioSource Audio;
    Transform HpFire;

    int Ammunition;
    int MagazineAmmo;

    float FireDelay;
    float ReloadDelay;


    public class ClassProperties : PhxClass
    {
        public PhxProp<string> AnimationBank = new PhxProp<string>("rifle");

        public PhxProp<PhxClass> OrdnanceName = new PhxProp<PhxClass>(null);

        public PhxProp<float> ShotDelay     = new PhxProp<float>(0.2f);
        public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
        public PhxProp<int>   SalvoCount    = new PhxProp<int>(1);
        public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

        public PhxProp<int> MedalsTypeToUnlock = new PhxProp<int>(0);
        public PhxProp<int> ScoreForMedalsType = new PhxProp<int>(0);

        public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

        // Sound
        public PhxProp<float>  PitchSpread = new PhxProp<float>(0.1f);
        public PhxProp<string> FireSound = new PhxProp<string>(null);
    }


    public override void Init()
    {
        if (C.FireSound.Get() != null)
        {
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.playOnAwake = false;
            Audio.spatialBlend = 1.0f;
            Audio.rolloffMode = AudioRolloffMode.Linear;
            Audio.minDistance = 2.0f;
            Audio.maxDistance = 30.0f;
            Audio.loop = false;

            // TODO: replace with class sound, once we can load sound LVLs
            Audio.clip = SoundLoader.LoadSound("wpn_rep_blaster_fire");
        }

        if (C.OrdnanceName.Get() == null)
        {
            Debug.LogWarning($"Missing Ordnance class in cannon '{name}'!");
        }

        if (transform.childCount > 0)
        {
            HpFire = transform.GetChild(0).Find("hp_fire");
        }
        if (HpFire == null)
        {
            Debug.LogWarning($"Cannot find 'hp_fire' in '{name}', class '{C.Name}'!");
        }

        // Total amount of 4 magazines
        Ammunition = C.RoundsPerClip * 3;
        MagazineAmmo = C.RoundsPerClip;
    }

    public override void BindEvents()
    {
        
    }

    public PhxInstance GetInstance()
    {
        return this;
    }

    public void Fire(PhxPawnController owner, Vector3 targetPos)
    {
        if (FireDelay <= 0f && ReloadDelay == 0f && MagazineAmmo >= C.ShotsPerSalvo)
        {
            if (Audio != null)
            {
                float half = C.PitchSpread / 2f;
                Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                Audio.Play();
            }

            if (C.OrdnanceName.Get() != null)
            {
                PhxBolt bolt = C.OrdnanceName.Get() as PhxBolt;
                if (HpFire != null && bolt != null)
                {
                    Vector3 dirVec = targetPos - HpFire.position;
                    Quaternion dir = Quaternion.LookRotation(dirVec);
                    Scene.FireProjectile(owner, HpFire.position, dir, bolt);
                    Debug.DrawRay(HpFire.position, dirVec, Color.red);
                }
            }

            ShotCallback?.Invoke();
            FireDelay = C.ShotDelay;

            if (!Game.InfiniteAmmo)
            {
                MagazineAmmo -= C.ShotsPerSalvo;
            }
            if (MagazineAmmo < C.ShotsPerSalvo)
            {
                Reload();
            }
        }
    }

    public void Reload()
    {
        if (ReloadDelay > 0f)
        {
            // already busy reloading
            return;
        }

        if (Ammunition > 0)
        {
            ReloadDelay = C.ReloadTime;
            ReloadCallback?.Invoke();
        }
    }

    public void OnShot(Action callback)
    {
        ShotCallback += callback;
    }

    public void OnReload(Action callback)
    {
        ReloadCallback += callback;
    }

    public string GetAnimBankName()
    {
        return C.AnimationBank;
    }

    public int GetMagazineSize()
    {
        return C.RoundsPerClip;
    }

    public int GetTotalAmmo()
    {
        return Ammunition + MagazineAmmo;
    }

    public int GetMagazineAmmo()
    {
        return MagazineAmmo;
    }

    public int GetAvailableAmmo()
    {
        return Ammunition;
    }

    public float GetReloadTime()
    {
        return C.ReloadTime;
    }

    public float GetReloadProgress()
    {
        return 1f - ReloadDelay / C.ReloadTime;
    }

    public override void Tick(float deltaTime)
    {
        FireDelay -= deltaTime;

        if (ReloadDelay > 0f)
        {
            ReloadDelay -= deltaTime;
            if (ReloadDelay <= 0f)
            {
                int reloadAmount = C.RoundsPerClip - MagazineAmmo;
                if (Ammunition < reloadAmount)
                {
                    reloadAmount = Ammunition;
                }

                MagazineAmmo += reloadAmount;
                Ammunition -= reloadAmount;

                ReloadDelay = 0f;
            }
        }
    }
}
