using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;




public class PhxCannon : PhxGenericWeapon, IPhxWeapon
{
    public class ClassProperties : PhxGenericWeapon.ClassProperties
    {
        public PhxProp<int> MedalsTypeToUnlock = new PhxProp<int>(0);
        public PhxProp<int> ScoreForMedalsType = new PhxProp<int>(0);

        public PhxProp<string> GeometryName = new PhxProp<string>("");
    } 

    PhxCannon.ClassProperties CannonClass;


    public override void Init()
    {
        CannonClass = C as PhxCannon.ClassProperties;

        //if (CannonClass.FireSound.Get() != null)
        //{
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.playOnAwake = false;
            Audio.spatialBlend = 1.0f;
            Audio.rolloffMode = AudioRolloffMode.Linear;
            Audio.minDistance = 2.0f;
            Audio.maxDistance = 30.0f;
            Audio.loop = false;

            // TODO: replace with class sound, once we can load sound LVLs
            Audio.clip = SoundLoader.LoadSound("wpn_rep_blaster_fire");
        //}

        if (CannonClass.OrdnanceName.Get() == null)
        {
            Debug.LogWarning($"Missing Ordnance class in cannon '{name}'!");
        }

        if (transform.childCount > 0)
        {
            FirePoint = transform.GetChild(0).Find("hp_fire");
        }

        if (FirePoint == null)
        {
            //Debug.LogWarning($"Cannot find 'hp_fire' in '{name}', class '{C.Name}'!");
        }


        // Total amount of 4 magazines
        Ammunition = CannonClass.RoundsPerClip * 3;
        MagazineAmmo = CannonClass.RoundsPerClip;
    }


    /*
    public override bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        if (FireDelay <= 0f && ReloadDelay == 0f)
        {
            for (int i = 0; i < C.SalvoCount; i++)
            {            
                if (Audio != null)
                {
                    float half = CannonClass.PitchSpread / 2f;
                    Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                    Audio.Play();
                }

                if (CannonClass.OrdnanceName.Get() != null)
                {
                    //Debug.LogFormat("Firing ord: {0}", C.OrdnanceName.Get().EntityClass.Name);
                    Scene.FireProjectile(this, CannonClass.OrdnanceName.Get() as PhxOrdnanceClass);   
                }
            }

            ShotCallback?.Invoke();
            FireDelay = CannonClass.ShotDelay;

            if (!Game.InfiniteAmmo)
            {
                MagazineAmmo -= CannonClass.ShotsPerSalvo;
            }
            if (MagazineAmmo < CannonClass.ShotsPerSalvo)
            {
                Reload();
            }
            

            return true;
        }
        else 
        {
            return false;
        }
    }
    */
}
