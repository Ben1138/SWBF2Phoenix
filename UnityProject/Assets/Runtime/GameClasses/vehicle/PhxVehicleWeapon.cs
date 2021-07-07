using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;



public class PhxWeapon : PhxInstance<PhxWeapon.ClassProperties>, IPhxWeapon
{
	PhxGameRuntime Game => PhxGameRuntime.Instance;
    PhxRuntimeScene Scene => PhxGameRuntime.GetScene();

    Action ShotCallback;
    Action ReloadCallback;
    AudioSource Audio;

    int Ammunition;
    int MagazineAmmo;

    float FireDelay;
    float ReloadDelay;

	public class ClassProperties : PhxClass
	{
		public PhxProp<float> ShotDelay     = new PhxProp<float>(0.2f);
	    public PhxProp<float> ReloadTime    = new PhxProp<float>(1.0f);
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
        if (true)
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
                if (bolt != null)
                {
                	Debug.Log("Firing bolt!");

                    Vector3 dirVec = (targetPos - transform.position).normalized;
                    Quaternion dir = Quaternion.LookRotation(dirVec);
                    Scene.FireProjectile(owner, transform.position, dir, bolt);
                    Debug.DrawRay(transform.position, dirVec * 1000f, Color.red);
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
        return "";
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

    public override void TickPhysics(float deltaTime)
    {

    }
}







/*
classlabel = weapon

Equivalent to WEAPONSECTIONs in vehicles.  Maintains aimers, barrels, firing configurations,
and fires weapons.
*/


public class PhxWeaponSystem
{
    static PhxRuntimeScene SCENE => PhxGameRuntime.GetScene();

    IPhxWeapon Weapon;
    Transform WeaponTransform;

    PhxVehicleSection OwnerSection; 

    public List<PhxAimer> Aimers;



    public void AddAimer(PhxAimer Aimer)
    {
    	Aimer.Init();
        if (Aimers.Count > 0 && Aimers.Last().HierarchyLevel > Aimer.HierarchyLevel)
        {
            Aimers[Aimers.Count - 1].ChildAimer = Aimer;
        }
        else 
        {
            Aimers.Add(Aimer);
        }
    }


    public void SetWeapon(string WeaponName)
    {
    	Weapon = SCENE.CreateInstance(SCENE.GetClass(WeaponName), false) as IPhxWeapon;

    	if (Weapon == null)
    	{
    		Debug.LogErrorFormat("Couldn't get weapon class: {0}", WeaponName);
    	}
    	else 
    	{
    		WeaponTransform = Weapon.GetInstance().gameObject.transform;
    	}
    }


    public PhxWeaponSystem(PhxVehicleSection Section)
    {
    	OwnerSection = Section;
        Aimers = new List<PhxAimer>();
    }


    public void AdjustAimers(Vector3 Target)
    {
        foreach (PhxAimer Aimer in Aimers)
        {
            Aimer.AdjustAim(Target);
        }
    }



    int CurrBarrel;
    int CurrAimer;

    public bool Fire()
    {
        if (Aimers.Count > 0 && WeaponTransform != null)
        {
        	Aimers[CurrAimer].GetLeafTarget(0, out Vector3 pos, out Quaternion rot);

        	WeaponTransform.rotation = rot;
        	WeaponTransform.position = pos;

        	Weapon.Fire(OwnerSection.Occupant.GetController(), pos + WeaponTransform.forward);
        	
        	
            Aimers[CurrAimer].Fire();

            CurrAimer++;
            
            if (CurrAimer >= Aimers.Count)
            {
                CurrAimer = 0;
            }
        }
        else 
        {
        	Debug.LogError("No aimers to fire with!");
        }

        return true;
    }


    public void Update(Vector3 TargetPos)
    {
        foreach (var Aimer in Aimers)
        {
            Aimer.AdjustAim(TargetPos, false);
            Aimer.UpdateBarrel();
        }
    }
}


