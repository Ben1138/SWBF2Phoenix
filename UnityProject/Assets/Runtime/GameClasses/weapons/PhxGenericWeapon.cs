using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;


public enum PhxWeaponState : int
{
    Reloading,
    Overheated,
    ShotDelayed,
    SalvoDelayed,
    Free
}


public class PhxGenericWeapon : PhxInstance<PhxGenericWeapon.ClassProperties>, IPhxWeapon
{
	protected PhxGameRuntime Game => PhxGameRuntime.Instance;
    protected PhxRuntimeScene Scene => PhxGameRuntime.GetScene();

    protected Action ShotCallback;
    protected Action ReloadCallback;
    protected AudioSource Audio;

    protected int Ammunition;
    protected int MagazineAmmo;

    protected float FireDelay;
    protected float ReloadDelay;

    protected float SalvoDelayTimer;


    PhxWeaponState WeaponState = PhxWeaponState.Free;


    protected PhxPawnController OwnerController;

    // Colliders that spawned ordnance should not collide with, ie,
    // that of the OwnerController's soldier or vehicle
    protected List<Collider> IgnoredColliders;


    protected Transform FirePoint;


	public class ClassProperties : PhxClass
	{
        public PhxProp<string> AnimationBank = new PhxProp<string>("rifle");

        public PhxProp<float> ShotDelay = new PhxProp<float>(0.5f);
        public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
	    public PhxProp<float> SalvoDelay    = new PhxProp<float>(1.0f);
	    public PhxProp<int>   SalvoCount    = new PhxProp<int>(1);
	    public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

	    public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

	    public PhxProp<PhxClass> OrdnanceName = new PhxProp<PhxClass>(null);

	    // Sound
	    public PhxProp<float>  PitchSpread = new PhxProp<float>(0.1f);
	    public PhxProp<string> FireSound = new PhxProp<string>(null);

	    public PhxProp<float> HeatRecoverRate = new PhxProp<float>(0.25f);
	    public PhxProp<float> HeatThreshold = new PhxProp<float>(0.2f); 
	    public PhxProp<float> HeatPerShot = new PhxProp<float>(0.12f);
	}


    public override void Init()
    {
        //if (C.FireSound.Get() != null)
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

        if (C.OrdnanceName.Get() == null)
        {
            Debug.LogWarning($"Missing Ordnance class in cannon '{name}'!");
        }

        // Total amount of 4 magazines
        Ammunition = C.RoundsPerClip * 3;
        MagazineAmmo = C.RoundsPerClip;
    }

    public override void BindEvents()
    {
        
    }


    public void SetFirePoint(Transform FP)
    {
        FirePoint = FP;
    }



    public virtual void GetFirePoint(out Vector3 Pos, out Quaternion Rot)
    {
        if (FirePoint == null)
        {
            Pos = Vector3.zero;
            Rot = Quaternion.identity;
        }
        else 
        {
            Pos = FirePoint.position;
            Rot = FirePoint.rotation;
        }
    }


    public virtual Transform GetFirePoint()
    {
        return FirePoint;
    }


    public bool IsFiring()
    {
        return true;
    }


    public void SetOwnerController(PhxPawnController Owner)
    {
        OwnerController = Owner;
    }


    public PhxPawnController GetOwnerController()
    {
        return OwnerController;
    }


    public void SetIgnoredColliders(List<Collider> Colliders)
    {
        IgnoredColliders = Colliders;
    }

    public List<Collider> GetIgnoredColliders()
    {
        if (IgnoredColliders == null)
        {
            return new List<Collider>();
        }
        return IgnoredColliders;
    }


    public PhxInstance GetInstance()
    {
        return this;
    }


    int SalvoIndex = 0;

	public virtual bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        if (WeaponState == PhxWeaponState.Free)
        {
            if (Audio != null)
            {
                float half = C.PitchSpread / 2f;
                Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                Audio.Play();
            }

            if (SalvoIndex < C.SalvoCount)
            {
                PhxOrdnance.ClassProperties Ordnance = C.OrdnanceName.Get() as PhxOrdnance.ClassProperties;
                if (Ordnance != null) 
                {
                    //Debug.LogFormat("Firing ordnance: {0}", Ordnance.EntityClass.Name);
                    Scene.FireProjectile(this, Ordnance);   
                }
            }

            if (++SalvoIndex >= C.SalvoCount)
            {
                SalvoIndex = 0;
                WeaponState = PhxWeaponState.ShotDelayed;
                FireDelay = C.ShotDelay;
            }
            else 
            {
                WeaponState = PhxWeaponState.SalvoDelayed;
                SalvoDelayTimer = C.SalvoDelay;
            }

            ShotCallback?.Invoke();

            if (!Game.InfiniteAmmo)
            {
                MagazineAmmo -= C.ShotsPerSalvo;
            }
            if (MagazineAmmo < C.ShotsPerSalvo)
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


    public void Reload()
    {
        if (WeaponState == PhxWeaponState.Reloading)
        {
            // already busy reloading
            return;
        }

        if (Ammunition > 0)
        {
            WeaponState = PhxWeaponState.Reloading;
            
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

    public virtual string GetAnimBankName()
    {
        return C.AnimationBank.Get();
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
        if (WeaponState == PhxWeaponState.Reloading)
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

                WeaponState = PhxWeaponState.Free;
            }
        }
        else if (WeaponState == PhxWeaponState.SalvoDelayed)
        {
            SalvoDelayTimer -= deltaTime;
            if (SalvoDelayTimer < 0f)
            {
                WeaponState = PhxWeaponState.Free;
                Fire(null, Vector3.zero);
            }
        }
        else if (WeaponState == PhxWeaponState.ShotDelayed)
        {
            FireDelay-= deltaTime;
            if (FireDelay < 0f)
            {
                WeaponState = PhxWeaponState.Free;
            }
        }
    }

    public override void TickPhysics(float deltaTime){}
}