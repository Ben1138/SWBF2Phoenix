using System.Collections.Generic;
using UnityEngine;


public class PhxOrdnancePool : PhxComponentPool<PhxOrdnance>
{
    PhxOrdnanceClass OrdnanceClass;

    public PhxOrdnancePool(PhxOrdnance prefab, PhxOrdnanceClass ordClass, int size)
        : base(prefab, ordClass.EntityClass.Name, size, ordClass.LifeSpan)
    {
        // We will call them manually
        CallObjectInit = false;

        OrdnanceClass = ordClass;
    }

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < Objects.Length; ++i)
        {
            PhxOrdnance casted = Objects[i] as PhxOrdnance;
            Debug.Assert(casted != null);
            casted.OrdnanceClass = OrdnanceClass;
            casted.Init();
        }
    }
}

public class PhxProjectiles : IPhxTickable, IPhxTickablePhysics
{
    PhxGame Game => PhxGame.Instance;

    GameObject ProjectileRoot;

    /*
    Per class pooling, figured it would be needed for more complex types
    like missiles, which have a varying number of transforms/colliders etc.
    Could also be useful for types that have varying upper bounds on numbers of
    instances, eg, there will be a ton of standard blaster bolts, but few 
    purple award bolts.

    Feel free to change, of course, this could be too inefficient, or we could move
    to per-weapon ordnance pooling.

    See note in PhxOrdnance about why PhxClass is used instead of PhxOrdnance.ClassProperties
    */

    Dictionary<PhxOrdnanceClass, PhxOrdnancePool> PoolDB;

    // This won't be needed when effects are integrated
    // PhxPool<ParticleSystem> Sparks;

    public PhxProjectiles()
    {
        ProjectileRoot = new GameObject("Projectiles");
    
        PoolDB = new Dictionary<PhxOrdnanceClass, PhxOrdnancePool>();
    }

    /*
    Checks if class has a pool, if not makes a new one, and returns available
    instance from said pool.  Num instances in pool is thoughtlessly
    hardcoded for now.  Lifetimes are set for trivial types like 'bolt'.
    */

    public void FireProjectile(IPhxWeapon OriginatorWeapon,
                                PhxOrdnanceClass OrdnanceClass, 
                                Vector3 Pos, Quaternion Rot)
    {
        if (!PoolDB.TryGetValue(OrdnanceClass, out PhxOrdnancePool Pool)) 
        {   
            System.Type OClassType = OrdnanceClass.GetType();
            
            if (OClassType == typeof(PhxMissileClass))
            {
                PhxMissileClass MissileClass = OrdnanceClass as PhxMissileClass;
                
                // Messy, will get a prefab sorted out.  That requires another
                // method in ModelLoader for attaching meshes instead of freshly instantiating them...
                GameObject MissileObj = ModelLoader.Instance.GetGameObjectFromModel(MissileClass.GeometryName.Get(),null);
                
                if (MissileObj == null) 
                {
                    // Not sure if there's a default geometry for missiles/shells
                    // Debug.LogWarningFormat("Failed to get geometry: {0}", MissileClass.GeometryName.Get());
                    MissileObj = new GameObject(MissileClass.EntityClass.Name);
                    SphereCollider coll = MissileObj.AddComponent<SphereCollider>();
                    coll.radius = .4f;
                } 

                PhxMissile Missile = MissileObj.AddComponent<PhxMissile>();
                Pool = new PhxOrdnancePool(Missile, MissileClass, 25);
                Pool.Init();
                GameObject.Destroy(MissileObj);
            }
            else if (OClassType == typeof(PhxBeamClass))
            {
                Pool = new PhxOrdnancePool(Game.BeamPrefab, OrdnanceClass, 5);
                Pool.Init();
            }
            else if (OClassType == typeof(PhxBoltClass))
            {
                Pool = new PhxOrdnancePool(Game.BoltPrefab, OrdnanceClass, 20);
                Pool.Init();
                // To avoid Update() call in MonoBehavior
            }
            else 
            {
                return;
            }

            Pool.Root.transform.SetParent(ProjectileRoot.transform);
            PoolDB[OrdnanceClass] = Pool;
        }
        
        if (Pool.Alloc(out PhxOrdnance Ordnance, OrdnanceClass.LifeSpan))
        {
            Ordnance.Setup(OriginatorWeapon, Pos, Rot); 
        }           
    }


    public void Destroy()
    {
        PoolDB.Clear();
        GameObject.Destroy(ProjectileRoot);
    }


    public void Tick(float deltaTime)
    {
        foreach (var entry in PoolDB)
        {
            entry.Value.Tick(deltaTime);
        }
    }

    public void TickPhysics(float deltaTime)
    {
        foreach (var entry in PoolDB)
        {
            entry.Value.TickPhysics(deltaTime);
        }
    }

#if UNITY_EDITOR
    public int GetActiveCount()
    {
        int count = 0;
        foreach (var entry in PoolDB)
        {
            count += entry.Value.ActiveCount;
        }
        return count;
    }
    public int GetTotalCount()
    {
        int count = 0;
        foreach (var entry in PoolDB)
        {
            count += entry.Value.TotalCount;
        }
        return count;
    }
#endif
}