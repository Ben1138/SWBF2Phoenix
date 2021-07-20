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
    Dictionary<T, int> ObjToIdx;
    
    float MaxLifeTime;
    float[] LifeTimes;

    public PhxPool(T prefab, string rootName, int size, float maxLifeTime=float.PositiveInfinity)
    {
        Objects = new T[size];
        ObjToIdx = new Dictionary<T, int>();

        MaxLifeTime = maxLifeTime;
        LifeTimes = new float[size];

        GameObject root = new GameObject(rootName);
        for (int i = 0; i < Objects.Length; ++i)
        {
            Objects[i] = Object.Instantiate(prefab, root.transform);
            //Objects[i].gameObject.SetActive(false);
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

    PhxPool<PhxProjectile> Projectiles;
    PhxPool<ParticleSystem> Sparks;

    public PhxProjectiles()
    {
        Projectiles = new PhxPool<PhxProjectile>(Game.ProjPrefab, "Projectiles", COUNT, 2f);
        for (int i = 0; i < Projectiles.Objects.Length; ++i)
        {
            Projectiles.Objects[i].OnHit += ProjectileHit;
        }

        Sparks = new PhxPool<ParticleSystem>(Game.SparkPrefab, "Sparks", COUNT, 1.5f);
    }


    public void FireProjectile(PhxPawnController owner, Vector3 pos, Quaternion rot, PhxBolt bolt, List<Collider> Colliders)
    {
        PhxProjectile proj = Projectiles.Alloc();
        if (proj == null)
        {
            Debug.LogWarning($"Ran out of projectile instances! Maximum of {Projectiles.Objects.Length} reached!");
            return;
        }

        foreach (Collider coll in Colliders)
        {
            Physics.IgnoreCollision(proj.Coll, coll);
        }

        if (proj != null)
        {
            proj.Setup(owner, pos, rot, bolt);
        }
    }


    public void FireProjectile(PhxPawnController owner, Vector3 pos, Quaternion rot, PhxBolt bolt)
    {
        PhxProjectile proj = Projectiles.Alloc();
        if (proj == null)
        {
            Debug.LogWarning($"Ran out of projectile instances! Maximum of {Projectiles.Objects.Length} reached!");
            return;
        }
        PhxInstance inst = owner.Pawn.GetInstance();
        if (inst != null)
        {
            CapsuleCollider coll = inst.GetComponent<CapsuleCollider>();
            if (coll != null)
            {
                Physics.IgnoreCollision(proj.Coll, coll);
            }            
        }
        if (proj != null)
        {
            proj.Setup(owner, pos, rot, bolt);
        }
    }

    public void Destroy()
    {
        Projectiles.Destroy();
        Projectiles = null;
    }

    public void Tick(float deltaTime)
    {
        Projectiles.Tick(deltaTime);
        Sparks.Tick(deltaTime);
    }

    void ProjectileHit(PhxProjectile proj, Collision coll)
    {
        Projectiles.Free(proj);
        ParticleSystem spark = Sparks.Alloc();
        if (spark != null)
        {
            spark.transform.position = coll.contacts[0].point;
            spark.transform.forward = coll.contacts[0].normal;
        }
        else
        {
            Debug.LogWarning($"Exceeded available spark effect instances of {Sparks.Objects.Length}!");
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
