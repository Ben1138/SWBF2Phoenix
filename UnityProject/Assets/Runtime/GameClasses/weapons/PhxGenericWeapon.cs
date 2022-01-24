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


public class PhxGenericWeapon : PhxInstance<PhxGenericWeapon.ClassProperties>, IPhxWeapon, IPhxTickable
{
	protected PhxGame Game => PhxGame.Instance;
    protected PhxScene Scene => PhxGame.GetScene();

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
        public PhxProp<float> InitialSalvoDelay = new PhxProp<float>(0f);
        public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
	    public PhxProp<float> SalvoDelay    = new PhxProp<float>(1.0f);
	    public PhxProp<int>   SalvoCount    = new PhxProp<int>(1);
	    public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

        public PhxProp<float> ShotElevate = new PhxProp<float>(0f);        

        public PhxProp<bool> TriggerSingle = new PhxProp<bool>(false);

        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(false);

	    public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

	    public PhxProp<PhxClass> OrdnanceName = new PhxProp<PhxClass>(null);

        // Spread
        public PhxProp<float> PitchSpread = new PhxProp<float>(0f);
        public PhxProp<float> YawSpread   = new PhxProp<float>(0f);


	    // Sound
	    public PhxProp<string> FireSound = new PhxProp<string>(null);

	    public PhxProp<float> HeatRecoverRate = new PhxProp<float>(0.25f);
	    public PhxProp<float> HeatThreshold = new PhxProp<float>(0.2f); 
	    public PhxProp<float> HeatPerShot = new PhxProp<float>(0.12f);
	}


    public override void Init()
    {   
        /*
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
        */


        if (C.OrdnanceName.Get() == null)
        {
            Debug.LogWarning($"Missing Ordnance class in weapon '{name}'!");
        }

        if (transform.childCount > 0)
        {
            FirePoint = transform.GetChild(0).Find("hp_fire");
        }

        if (FirePoint == null)
        {
            //Debug.LogWarning($"Cannot find 'hp_fire' in '{name}', class '{C.Name}'!");
            FirePoint = transform;
        }

        // Total amount of 4 magazines
        Ammunition = C.RoundsPerClip * 3;
        MagazineAmmo = C.RoundsPerClip;
    }

    public override void Destroy()
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
            Pos = transform.position;
            Rot = transform.rotation;
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
    Vector3 SalvoTargetPosition;

	public virtual bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
        //Debug.LogFormat("Attempting to fire weapon {0}", C.EntityClass.Name);

        if (WeaponState == PhxWeaponState.Free)
        {
            SalvoTargetPosition = targetPos;

            WeaponState = PhxWeaponState.SalvoDelayed;
            SalvoDelayTimer = C.InitialSalvoDelay;   
        }


        if (WeaponState == PhxWeaponState.SalvoDelayed && SalvoDelayTimer < .0001)
        {
            if (Audio != null)
            {
                float half = C.PitchSpread / 2f;
                Audio.pitch = UnityEngine.Random.Range(1f - half, 1f + half);
                Audio.Play();
            }

            if (SalvoIndex < C.SalvoCount)
            {
                PhxOrdnanceClass Ordnance = C.OrdnanceName.Get() as PhxOrdnanceClass;
                if (Ordnance != null) 
                {
                    for (int i = 0; i < C.ShotsPerSalvo; i++)
                    {
                        Vector3 TargetPosition = SalvoTargetPosition;
                        
                        /*
                        if (FirePoint == null)
                        {
                            FirePoint = transform;
                        }
                        float ShotLength = Vector3.Magnitude(SalvoTargetPosition - FirePoint.position);

                        // We'll say 1 spread = 0.436332 rad of fp rotation
                        TargetPosition += Mathf.Tan(C.PitchSpread * .436332f) * ShotLength * FirePoint.up;
                        TargetPosition += Mathf.Tan(C.YawSpread * .436332f) * ShotLength * FirePoint.right;
                        */
                        
                        Scene.FireProjectile(this, Ordnance, FirePoint.position + C.ShotElevate * Vector3.up,
                                            Quaternion.LookRotation(TargetPosition - FirePoint.position, Vector3.up));   
                    }
                }
            }

            if (++SalvoIndex >= C.SalvoCount || C.TriggerSingle.Get())
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

            if (!Game.Settings.InfiniteAmmo)
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

    public void Tick(float deltaTime)
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
                Fire(null, SalvoTargetPosition);
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
}