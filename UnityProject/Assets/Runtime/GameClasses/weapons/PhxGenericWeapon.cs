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
    Charging,
    Free
}


public class PhxWeapon : PhxGenericWeapon<PhxCannon.ClassProperties> { }

public class PhxGenericWeapon<T> : PhxInstance<T> , IPhxWeapon, IPhxTickable where T : PhxGenericWeapon<T>.ClassProperties
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
        public PhxProp<string> GeometryName = new PhxProp<string>("");

        public PhxProp<string> AnimationBank = new PhxProp<string>("");
        public PhxMultiProp ComboAnimationBank = new PhxMultiProp(typeof(string), typeof(string), typeof(string));
        public PhxMultiProp CustomAnimationBank = new PhxMultiProp(typeof(string), typeof(string), typeof(string));

        public PhxProp<string> FirePointName = new PhxProp<string>("hp_fire");

        // Various state time values
        public PhxProp<float> ShotDelay = new PhxProp<float>(0.5f);
        public PhxProp<float> InitialSalvoDelay = new PhxProp<float>(0f);
        public PhxProp<float> ReloadTime = new PhxProp<float>(1.0f);
	    public PhxProp<string> SalvoDelay = new PhxProp<string>("1.0");
	    public PhxProp<int>   SalvoCount = new PhxProp<int>(1);
	    public PhxProp<int>   ShotsPerSalvo = new PhxProp<int>(1);

        public PhxProp<bool> TriggerSingle = new PhxProp<bool>(false);

        public PhxProp<bool> OffhandWeapon = new PhxProp<bool>(false);

	    public PhxProp<int> RoundsPerClip = new PhxProp<int>(50);

	    public PhxProp<PhxClass> OrdnanceName = new PhxProp<PhxClass>(null);

	    // Sound
	    public PhxProp<string> FireSound = new PhxProp<string>(null);

	    public PhxProp<float> HeatRecoverRate = new PhxProp<float>(0.25f);
	    public PhxProp<float> HeatThreshold = new PhxProp<float>(0.2f); 
	    public PhxProp<float> HeatPerShot = new PhxProp<float>(0.12f);

        // Old spread system
        public PhxProp<float> PitchSpread = new PhxProp<float>(0f);
        public PhxProp<float> YawSpread   = new PhxProp<float>(0f);

        // New spread system see: https://sites.google.com/site/swbf2modtoolsdocumentation/weapon_notes
        public PhxProp<float> SpreadPerShot = new PhxProp<float>(0f);
        public PhxProp<float> SpreadRecoverRate = new PhxProp<float>(0f); 
        public PhxProp<float> SpreadThreshold = new PhxProp<float>(0f); 
        public PhxProp<float> SpreadLimit = new PhxProp<float>(0f);

        // Applied before or after spread?
        public PhxProp<float> ShotElevate = new PhxProp<float>(0f);        
	}


    // Need a better name for this, but will replace IPhxWeapon.IsFiring()
    public bool IsTriggerPressed;

    protected bool CanFire = true;

    float SalvoDelay;


    public override void Init()
    {   
        if (C.FireSound.Get() != null)
        {
            AudioClip FireSound = SoundLoader.LoadSound(C.FireSound.Get());

            if (FireSound != null)
            {
                Audio = gameObject.AddComponent<AudioSource>();
                Audio.playOnAwake = false;
                Audio.spatialBlend = 1.0f;
                Audio.rolloffMode = AudioRolloffMode.Linear;
                Audio.minDistance = 2.0f;
                Audio.maxDistance = 30.0f;
                Audio.loop = false;
                Audio.clip = FireSound;
            }
        }

        if (C.OrdnanceName.Get() == null)
        {
            Debug.LogWarning($"Missing Ordnance class in weapon '{name}'!");
        }

        if (transform.childCount > 0)
        {
            FirePoint = transform.GetChild(0).Find(C.FirePointName);
        }

        if (FirePoint == null)
        {
            //Debug.LogWarning($"Cannot find 'hp_fire' in '{name}', class '{C.Name}'!");
            FirePoint = transform;
        }

        // Total amount of 4 magazines
        Ammunition = C.RoundsPerClip * 3;
        MagazineAmmo = C.RoundsPerClip;


        // Implied, but need to confirm 
        if (C.SpreadPerShot > 0.001f)
        {
            bUsesNewSpreadSystem = true;
        }

        SalvoDelay = float.Parse(C.SalvoDelay.Get().Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries)[0],
                                System.Globalization.CultureInfo.InvariantCulture);
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

    Vector3 SpreadAxis = Vector3.zero;
    float CurrSpread, EffectiveSpread;
    Quaternion SpreadQuat, ShotElevationQuat;

    protected bool bUsesNewSpreadSystem = false;

	public virtual bool Fire(PhxPawnController owner, Vector3 targetPos)
    {
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
                // PitchSpread was previously used here to vary the sound pitch, that was a misunderstanding
                // as PitchSpread is part of the weapon's aim spread, not sound
                Audio.Play();
            }

            if (SalvoIndex < C.SalvoCount)
            {
                PhxOrdnanceClass Ordnance = C.OrdnanceName.Get() as PhxOrdnanceClass;
                if (Ordnance != null) 
                {
                    /*
                    New SPREAD, per salvo

                    Most of this function could use some optimization, maybe keeping everything local until needed in global space
                    for Scene.FireProjectile?
                    */
                    if (bUsesNewSpreadSystem)
                    {
                        CurrSpread = Mathf.Min(CurrSpread + C.SpreadPerShot, C.SpreadLimit + C.SpreadThreshold);
                        EffectiveSpread = Mathf.Max(CurrSpread - C.SpreadThreshold, 0f);

                        if (EffectiveSpread < .0001f)
                        {
                            SpreadQuat = Quaternion.identity;
                        }
                        else 
                        {
                            // Maybe we replace this with a fixed sequence of axes
                            SpreadAxis.x = UnityEngine.Random.Range(-1f, 1f);
                            SpreadAxis.y = UnityEngine.Random.Range(-1f, 1f);
                            SpreadQuat = Quaternion.AngleAxis(EffectiveSpread, FirePoint.TransformDirection(SpreadAxis)); 
                        }
                    }

                    // Debug.LogFormat("SpreadAxis: {0}, CurrSpread: {1}, EffectiveSpread: {2} ", SpreadAxis.ToString("F4"), CurrSpread, EffectiveSpread);


                    /*
                    SHOT ELEVATION
                    */

                    ShotElevationQuat = Quaternion.AngleAxis(C.ShotElevate, -FirePoint.right);

                    for (int i = 0; i < C.ShotsPerSalvo; i++)
                    {
                        /*
                        Old SPREAD, per shot (just from PitchSpread and YawSpread)
                        */
                        if (!bUsesNewSpreadSystem)
                        {
                            SpreadQuat = Quaternion.AngleAxis(UnityEngine.Random.Range(-C.PitchSpread, C.PitchSpread), -FirePoint.right) * 
                                         Quaternion.AngleAxis(UnityEngine.Random.Range(-C.YawSpread, C.YawSpread), FirePoint.up);
                        }
                        
                        Scene.FireProjectile(this, Ordnance, FirePoint.position,
                            SpreadQuat * ShotElevationQuat * Quaternion.LookRotation(SalvoTargetPosition - FirePoint.position, Vector3.up)); 

                        ShotCallback?.Invoke();
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
                SalvoDelayTimer = SalvoDelay;
            }


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

    public virtual PhxAnimWeapon GetAnimInfo()
    {
        PhxAnimWeapon info = new PhxAnimWeapon
        {
            AnimationBank = C.AnimationBank.Get(),
            Combo = C.CustomAnimationBank.Get<string>(2),
            Parent = C.CustomAnimationBank.Get<string>(1),
            SupportsAlert = true,
            SupportsReload = true
        };
        if (C.ComboAnimationBank.Values.Count > 0)
        {
            info.AnimationBank = C.ComboAnimationBank.Get<string>(0).Split('_')[1];
        }
        return info;
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
        CurrSpread = Mathf.Max(CurrSpread - deltaTime * C.SpreadRecoverRate, 0f);

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
        else if (WeaponState == PhxWeaponState.Free)
        {
            if (IsTriggerPressed)
            {
                if (CanFire)
                {
                    Fire(null, SalvoTargetPosition);
                    if (C.TriggerSingle == true)
                    {
                        CanFire = false;
                    }
                }
            }
            else 
            {
                if (C.TriggerSingle == true)
                {
                    CanFire = true;
                }
            }
        }
    }
}