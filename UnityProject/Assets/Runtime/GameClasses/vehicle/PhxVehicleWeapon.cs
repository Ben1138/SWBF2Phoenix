using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;


/*
classlabel = weapon

Equivalent to WEAPONSECTIONs in vehicles.  Maintains Aimers.
*/

public class PhxVehicleWeapon : PhxInstance<PhxVehicleWeapon.ClassProperties>, IPhxWeapon
{
    Action ShotCallback;
    Action ReloadCallback;
    AudioSource Audio;


    float FireDelay;
    float ReloadDelay = 0f;


    public class ClassProperties : PhxClass
    {         
        public PhxProp<float> ShotDelay     = new PhxProp<float>(0.2f);
        public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
        public PhxProp<int>   SalvoCount    = new PhxProp<int>(1);
        public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

        public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

        // Sound
        public PhxProp<float>  PitchSpread = new PhxProp<float>(0.1f);
        public PhxProp<string> FireSound = new PhxProp<string>(null);

        public PhxProp<float> HeatRecoverRate = new PhxProp<float>(0.25f);
        public PhxProp<float> HeatThreshold = new PhxProp<float>(0.2f); 
        public PhxProp<float> HeatPerShot = new PhxProp<float>(0.12f);
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
    }


    public override void BindEvents(){}

    public PhxInstance GetInstance(){return this;}

    public void Fire()
    {
        if (FireDelay <= 0f)
        {
            if (Audio != null)
            {
                float half = C.PitchSpread / 2f;
                Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                Audio.Play();
            }

            for (int i = 0; i < NumBarrelsPerShot; i++)
            {
                CurrBarrel = CurrBarrel % FirePoints.Count;
                //Emit from barrel...
                CurrBarrel++;
            }

            ShotCallback?.Invoke();
            FireDelay = C.ShotDelay;
        }
    }

    public void Reload(){}
    public void OnShot(Action callback){ShotCallback += callback;}
    public void OnReload(Action callback){ReloadCallback += callback;}
    public string GetAnimBankName(){return "";}
    public int GetMagazineSize(){return C.RoundsPerClip;}
    public int GetTotalAmmo(){return 0;}
    public int GetMagazineAmmo(){return 0;}
    public int GetAvailableAmmo(){return 0;}

    public float GetReloadProgress(){return 1f - ReloadDelay / C.ReloadTime;}


    private int CurrBarrel = 0;
    private int NumBarrelsPerShot = 1;
    private List<Transform> FirePoints = null;

    public void ConfigureFirePattern(List<Transform> firePoints, int numBarrelsPerShot)
    {
        NumBarrelsPerShot = numBarrelsPerShot;
        FirePoints = firePoints;
    }


    void Update()
    {
        if (FireDelay > 0.0f)
        {
            FireDelay -= Time.deltaTime;
        }
    }
}

