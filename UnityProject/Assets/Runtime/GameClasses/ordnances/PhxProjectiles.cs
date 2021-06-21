using System.Collections.Generic;
using UnityEngine;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using Unity.Mathematics;
//using Unity.Rendering;
//using Unity.Transforms;


public class PhxPool<T> where T : Component
{
    public T[] Objects { get; private set; }

    public GameObject Root { get; private set; }

    Dictionary<T, int> ObjToIdx;
    
    float MaxLifeTime;
    float[] LifeTimes;

    public PhxPool(T prefab, string rootName, int size, float maxLifeTime=float.PositiveInfinity)
    {
        Objects = new T[size];
        ObjToIdx = new Dictionary<T, int>();

        MaxLifeTime = maxLifeTime;
        LifeTimes = new float[size];

        Root = new GameObject(rootName);
        for (int i = 0; i < Objects.Length; ++i)
        {
            Objects[i] = Object.Instantiate(prefab, Root.transform);
            Objects[i].gameObject.SetActive(false);
            ObjToIdx.Add(Objects[i], i);
        }
    }

    public T Alloc()
    {
        // TODO: linear search is bad
        for (int i = 0; i < Objects.Length; ++i)
        {
            if (!Objects[i].gameObject.activeSelf)
            {
                Objects[i].gameObject.SetActive(true);
                LifeTimes[i] = MaxLifeTime;
                return Objects[i];
            }
        }

        return null;
    }

    public void Free(T obj)
    {
        if (ObjToIdx.TryGetValue(obj, out int idx))
        {
            Objects[idx].gameObject.SetActive(false);
            return;
        }
        Debug.LogWarning($"Tried to free unknown object '{obj.name}'!");
    }

    public void Destroy()
    {
        for (int i = 0; i < Objects.Length; ++i)
        {
            Object.Destroy(Objects[i].gameObject);
        }
        Objects = null;
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < Objects.Length; ++i)
        {
            if (Objects[i].gameObject.activeSelf)
            {
                LifeTimes[i] -= deltaTime;
                if (LifeTimes[i] <= 0f)
                {
                    Objects[i].gameObject.SetActive(false);
                }
            }
        }
    }
}



public class PhxProjectiles
{
    const int COUNT = 1024;

    PhxGameRuntime Game => PhxGameRuntime.Instance;

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

    Dictionary<PhxOrdnanceClass, PhxPool<PhxOrdnance>> PoolDB;

    /*
    To avoid calling Update() in more trivial types like bolts,
    we can check lifetimes from here. 
    */ 
    List<PhxPool<PhxOrdnance>> TickablePools;

    // This won't be needed when effects are integrated
    // PhxPool<ParticleSystem> Sparks;

    public PhxProjectiles()
    {
        ProjectileRoot = new GameObject("Projectiles");
    
        PoolDB = new Dictionary<PhxOrdnanceClass, PhxPool<PhxOrdnance>>();
        TickablePools = new List<PhxPool<PhxOrdnance>>(); 
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
        if (!PoolDB.TryGetValue(OrdnanceClass, out PhxPool<PhxOrdnance> Pool)) 
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
                Pool = new PhxPool<PhxOrdnance>(Missile, MissileClass.EntityClass.Name, 25); 
                GameObject.Destroy(MissileObj);
            }
            else if (OClassType == typeof(PhxBeamClass))
            {
                Pool = new PhxPool<PhxOrdnance>(Game.BeamPrefab, OrdnanceClass.EntityClass.Name, 5);  
            }
            else if (OClassType == typeof(PhxBoltClass))
            {
                Pool = new PhxPool<PhxOrdnance>(Game.BoltPrefab, OrdnanceClass.EntityClass.Name, 20, (OrdnanceClass as PhxBoltClass).LifeSpan);                
                // To avoid Update() call in MonoBehavior
                TickablePools.Add(Pool);
            }
            else 
            {
                return;
            }

            Pool.Root.transform.SetParent(ProjectileRoot.transform);
            PoolDB[OrdnanceClass] = Pool;
        }


        PhxOrdnance Ordnance = Pool.Alloc();
        if (Ordnance != null)
        {
            if (!Ordnance.IsInitialized)
            {
                Ordnance.Init(OrdnanceClass);
            }

            Ordnance.Setup(OriginatorWeapon, Pos, Rot); 
        }           
    }


    public void Destroy()
    {
        TickablePools.Clear();
        PoolDB.Clear();
        GameObject.Destroy(ProjectileRoot);
    }


    public void Tick(float deltaTime)
    {
        foreach (PhxPool<PhxOrdnance> TickablePool in TickablePools)
        {
            TickablePool.Tick(deltaTime);
        }
    }
}

////[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(EndFramePhysicsSystem))]
//public class PhxProjectiles : JobComponentSystem
//{
//    public static PhxProjectiles Instance { get; private set; }

//    PhxGameRuntime Game => PhxGameRuntime.Instance;

//    const int SIZE = 16384;

//    BuildPhysicsWorld BuildWorld;
//    StepPhysicsWorld StepWorld;

//    PhxPool<Light> Lights;
//    NativeArray<Entity> Projectiles;

//    ProjHitJob ProjJob;

//    struct ProjData : IComponentData
//    {
//        public bool InUse;
//    }

//    [BurstCompile]
//    struct ProjHitJob : ITriggerEventsJob
//    {
//        public ComponentDataFromEntity<ProjData> Projectiles;
//        public ComponentDataFromEntity<Translation> Transforms;
//        public ComponentDataFromEntity<Rotation> Rotations;
//        //public ComponentDataFromEntity<PhysicsVelocity> Velocities;


//        public void Execute(TriggerEvent triggerEvent)
//        {
//            Entity proj;
//            if (Projectiles.HasComponent(triggerEvent.EntityA))
//            {
//                proj = triggerEvent.EntityA;
//            }
//            else if (Projectiles.HasComponent(triggerEvent.EntityB))
//            {
//                proj = triggerEvent.EntityB;
//            }
//            else
//            {
//                // neither of both entities is a projectile
//                return;
//            }

//            if (!Projectiles[proj].InUse)
//            {
//                // noithing to do
//                return;
//            }

//            Translation pos = Transforms[proj];
//            pos.Value = float3.zero;
//            Transforms[proj] = pos;

//            Rotation rot = Rotations[proj];
//            rot.Value = quaternion.identity;
//            Rotations[proj] = rot;

//            //PhysicsVelocity vel = Velocities[proj];
//            //vel.Linear = float3.zero;
//            //Velocities[proj] = vel;

//            // TODO: Fire damage event
//        }
//    }

//    public void FireProjectile(float3 pos, quaternion rot, float3 vel, float2 size)
//    {
//        for (int i = 0; i < SIZE; ++i)
//        {
//            if (!EntityManager.GetComponentData<ProjData>(Projectiles[i]).InUse)
//            {
//                EntityManager.SetComponentData(Projectiles[i], new ProjData { InUse = true });

//                EntityManager.SetComponentData(Projectiles[i], new Translation { Value = pos });
//                EntityManager.SetComponentData(Projectiles[i], new Rotation { Value = rot });
//                EntityManager.SetComponentData(Projectiles[i], new NonUniformScale { Value = new float3(size.x, 0f, size.y) });

//                //EntityManager.SetComponentData(Projectiles[i], new PhysicsVelocity { Linear = vel });
//                return;
//            }
//        }

//        Debug.LogWarning("Projectiles exhausted! Consider increasing SIZE!");
//    }

//    public void InitProjectileMeshes()
//    {
//        for (int i = 0; i < SIZE; ++i)
//        {
//            EntityManager.SetSharedComponentData(Projectiles[i], new RenderMesh
//            {
//                mesh = Game.ProjectileMesh,
//                material = Game.ProjectileMaterial
//            });
//        }

//        Debug.Log("Projectile meshes initialized.");
//    }

//    protected override void OnCreate()
//    {
//        base.OnCreate();

//        BuildWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//        StepWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

//        EntityArchetype proj = EntityManager.CreateArchetype(
//            typeof(ProjData),
//            typeof(Translation),
//            typeof(Rotation),
//            typeof(NonUniformScale),
//            typeof(LocalToWorld),
//            typeof(RenderMesh),
//            typeof(RenderBounds)
//            //typeof(PhysicsVelocity)
//            //typeof(PhysicsCollider)
//        );

//        Lights = new PhxPool<Light>("ProjectileLights", SIZE);

//        Projectiles = EntityManager.CreateEntity(proj, SIZE, Allocator.Persistent);
//        for (int i = 0; i < SIZE; ++i)
//        {
//            EntityManager.SetComponentData(Projectiles[i], new Rotation
//            {
//                Value = quaternion.identity
//            });

//            EntityManager.SetComponentData(Projectiles[i], new NonUniformScale
//            {
//                Value = new float3(1f, 1f, 1f)
//            });

//            //EntityManager.SetComponentData(Projectiles[i], new PhysicsCollider
//            //{
//            //    Value = BoxCollider.Create(new BoxGeometry
//            //    {
//            //        Size = new float3(1f, .1f, 1f),
//            //        Orientation = quaternion.identity
//            //    }),
//            //});
//        }

//        ProjJob = new ProjHitJob();

//        Instance = this;
//        Debug.Log("PhxProjectiles created!");
//    }

//    protected override void OnDestroy()
//    {
//        Instance = null;
//        base.OnDestroy();
//        Projectiles.Dispose();
//        Debug.Log("PhxProjectiles destroyed!");
//    }

//    protected override JobHandle OnUpdate(JobHandle dependency)
//    {
//        ProjJob.Projectiles = GetComponentDataFromEntity<ProjData>();
//        ProjJob.Transforms = GetComponentDataFromEntity<Translation>();
//        ProjJob.Rotations = GetComponentDataFromEntity<Rotation>();
//        //ProjJob.Velocities = GetComponentDataFromEntity<PhysicsVelocity>();

//        JobHandle handle = ProjJob.Schedule(StepWorld.Simulation, ref BuildWorld.PhysicsWorld, dependency);
//        return handle;
//    }
//}
