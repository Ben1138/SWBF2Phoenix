using System;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;

public class PhxCannon : PhxInstance<PhxCannon.ClassProperties>
{
    public bool Fire;
    public Action Shot;

    AudioSource Audio;
    float FireDelay;

    public class ClassProperties : PhxClass
    {
        public PhxProp<string> AnimationBank = new PhxProp<string>("rifle");
        public PhxProp<float>  ShotDelay = new PhxProp<float>(0.2f);
        public PhxProp<float>  ReloadTime = new PhxProp<float>(1.0f);
        public PhxProp<float>  PitchSpread = new PhxProp<float>(0.1f);
        public PhxProp<string> FireSound = new PhxProp<string>(null);
    }


    public override void Init()
    {
        if (C.FireSound.Get() != null)
        {
            Audio = gameObject.AddComponent<AudioSource>();
            Audio.spatialBlend = 1.0f;
            Audio.rolloffMode = AudioRolloffMode.Linear;
            Audio.minDistance = 2.0f;
            Audio.maxDistance = 30.0f;
            Audio.loop = false;

            // TODO: replace with class sound, once we can load sound LVLs
            Audio.clip = SoundLoader.LoadSound("wpn_rep_blaster_fire");
        }
    }

    public override void BindEvents()
    {
        
    }

    void Update()
    {
        FireDelay -= Time.deltaTime;
        if (FireDelay <= 0f && Fire)
        {
            if (Audio != null)
            {
                float half = C.PitchSpread / 2f;
                Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                Audio.Play();
            }
            Shot?.Invoke();
            FireDelay = C.ShotDelay;
        }
    }
}
