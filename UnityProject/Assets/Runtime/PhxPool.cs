using System;
using System.Collections.Generic;
using UnityEngine;


public class PhxPool : IPhxInstantiable, IPhxTickable, IPhxTickablePhysics
{
    public bool IsInit => Objects != null;
    public int ActiveCount => ActiveIndices.Count;
    public int TotalCount => Objects.Length;

    protected string Name;
    protected bool CallObjectInit = true;
    protected float GeneralLifeTime;

    protected IPhxInstantiable[] Objects;
    float[] LifeTimes;
    
    Dictionary<IPhxInstantiable, int> ObjToIdx;

    HashSet<int> ActiveIndices;
    Queue<int> InactiveIndices;

    HashSet<int> TickableIndices;
    HashSet<int> TickablePhysicsIndices;


    public PhxPool(string name, int size, float generalLifeTime = float.PositiveInfinity)
    {
        Name = name;
        Objects = new IPhxInstantiable[size];
        LifeTimes = new float[size];
        ObjToIdx = new Dictionary<IPhxInstantiable, int>();

        GeneralLifeTime = generalLifeTime;

        ActiveIndices = new HashSet<int>();
        InactiveIndices = new Queue<int>();
        TickableIndices = new HashSet<int>();
        TickablePhysicsIndices = new HashSet<int>();
    }

    public virtual void Init()
    {
        for (int i = 0; i < Objects.Length; ++i)
        {
            Objects[i] = ConstructObject();
            ObjToIdx.Add(Objects[i], i);
            InactiveIndices.Enqueue(i);
        }
    }

    public void Destroy()
    {
        Debug.Assert(IsInit);

        for (int i = 0; i < Objects.Length; ++i)
        {
            DestroyObject(ref Objects[i]);
        }
        Objects = null;
    }

    public bool Alloc(out IPhxInstantiable obj)
    {
        Debug.Assert(IsInit);
        return Alloc(out obj, GeneralLifeTime);
    }

    public bool Alloc(out IPhxInstantiable obj, float lifeTime)
    {
        Debug.Assert(IsInit);

        if (InactiveIndices.Count == 0)
        {
            obj = default(IPhxInstantiable);
            return false;
        }

        int idx = InactiveIndices.Dequeue();

        LifeTimes[idx] = GeneralLifeTime;
        ObjectStateChanged(ref Objects[idx], true);

        if (Objects[idx] is IPhxTickable)
        {
            TickableIndices.Add(idx);
        }
        if (Objects[idx] is IPhxTickablePhysics)
        {
            TickablePhysicsIndices.Add(idx);
        }

        ActiveIndices.Add(idx);
        obj = Objects[idx];
        return true;

    }

    public void Free(IPhxInstantiable obj)
    {
        Debug.Assert(IsInit);

        if (ObjToIdx.TryGetValue(obj, out int idx))
        {
            Free(idx);
            return;
        }
        Debug.LogWarning($"Tried to free unknown object '{obj}'!");
    }

    protected void Free(int idx)
    {
        Debug.Assert(IsInit && idx >=0 && idx < Objects.Length);

        ObjectStateChanged(ref Objects[idx], false);
        ActiveIndices.Remove(idx);
        TickableIndices.Remove(idx);
        TickablePhysicsIndices.Remove(idx);
        InactiveIndices.Enqueue(idx);
        return;
    }

    public unsafe void Tick(float deltaTime)
    {
        if (Objects == null)
        {
            return;
        }

        const int MAX = 128;
        int* toFree = stackalloc int[MAX];
        int count = 0;

        foreach (int idx in ActiveIndices)
        {
            if (count >= MAX)
            {
                // Remaining will be freed next frame
                Debug.LogWarning($"More than {MAX} instances to free in Pool in one Frame!");
                break;
            }

            LifeTimes[idx] -= deltaTime;
            if (LifeTimes[idx] <= 0f)
            {
                if (count < MAX)
                {
                    // Can't free here, since we want to continue
                    // iterating the remaining active indices.
                    // So let's remember all indices to free.
                    toFree[count++] = idx;
                }
            }
            else if (TickableIndices.Contains(idx))
            {
                ((IPhxTickable)Objects[idx]).Tick(deltaTime);
            }
        }

        for (int i = 0; i < count; ++i)
        {
            Free(toFree[i]);
        }
    }

    public void TickPhysics(float deltaTime)
    {
        foreach (int idx in TickablePhysicsIndices)
        {
            ((IPhxTickablePhysics)Objects[idx]).TickPhysics(deltaTime);
        }
    }

    protected virtual IPhxInstantiable ConstructObject()
    {
        IPhxInstantiable obj = default(IPhxInstantiable);
        Debug.Assert(obj != null);
        if (CallObjectInit)
        {
            obj.Init();
        }
        return obj;
    }

    protected virtual void DestroyObject(ref IPhxInstantiable obj)
    {
        obj.Destroy();
    }

    protected virtual void ObjectStateChanged(ref IPhxInstantiable obj, bool inUse)
    {
        
    }
}

public class PhxComponentPool<T> : PhxPool where T : PhxComponent
{
    public GameObject Root { get; private set; }
    T Prefab;

    public PhxComponentPool(T prefab, string name, int size, float maxLifeTime = float.PositiveInfinity)
        : base(name, size, maxLifeTime)
    {
        Debug.Assert(prefab != null);

        Prefab = prefab;
        Root = new GameObject(name);
    }

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < Objects.Length; ++i)
        {
            T casted = Objects[i] as T;
            Debug.Assert(casted != null);
            casted.gameObject.SetActive(false);
        }
    }

    public bool Alloc(out T obj)
    {
        return Alloc(out obj, GeneralLifeTime);
    }

    public bool Alloc(out T obj, float lifeTime)
    {
        IPhxInstantiable inst;
        bool success = Alloc(out inst, lifeTime);
        obj = (T)inst;
        return success;
    }

    protected override IPhxInstantiable ConstructObject()
    {
        T inst = UnityEngine.Object.Instantiate(Prefab);
        inst.gameObject.transform.SetParent(Root.transform);
        inst.ParentPool = this;
        if (CallObjectInit)
        {
            inst.Init();
        }
        return inst;
    }

    protected override void DestroyObject(ref IPhxInstantiable obj)
    {
        base.DestroyObject(ref obj);
        T casted = obj as T;
        Debug.Assert(casted != null);
        UnityEngine.Object.Destroy(casted);
    }

    protected override void ObjectStateChanged(ref IPhxInstantiable obj, bool inUse)
    {
        T casted = obj as T;
        Debug.Assert(casted != null);
        casted.gameObject.SetActive(inUse);
    }
}