using System;
using System.Collections.Generic;
using UnityEngine;


public class PhxPool : IPhxInstantiable
{
    public bool IsInit => Objects != null;

    protected bool CallObjectInit = true;
    protected float GeneralLifeTime;

    protected IPhxInstantiable[] Objects;
    bool[] InUse;
    float[] LifeTimes;
    
    Dictionary<IPhxInstantiable, int> ObjToIdx;

    HashSet<int> TickablesIndices;


    public PhxPool(int size, float generalLifeTime = float.PositiveInfinity)
    {
        Objects = new IPhxInstantiable[size];
        InUse = new bool[size];
        LifeTimes = new float[size];
        ObjToIdx = new Dictionary<IPhxInstantiable, int>();

        GeneralLifeTime = generalLifeTime;

        TickablesIndices = new HashSet<int>();
    }

    public virtual void Init()
    {
        for (int i = 0; i < Objects.Length; ++i)
        {
            Objects[i] = ConstructObject();
            InUse[i] = false;
            ObjToIdx.Add(Objects[i], i);
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

        // TODO: linear search is bad
        for (int i = 0; i < Objects.Length; ++i)
        {
            if (!InUse[i])
            {
                InUse[i] = true;
                LifeTimes[i] = GeneralLifeTime;
                ObjectStateChanged(ref Objects[i], InUse[i]);

                TickablesIndices.Add(i);

                obj = Objects[i];
                return true;
            }
        }

        obj = default(IPhxInstantiable);
        return false;
    }

    public void Free(IPhxInstantiable obj)
    {
        Debug.Assert(IsInit);

        if (ObjToIdx.TryGetValue(obj, out int idx))
        {
            InUse[idx] = false;
            ObjectStateChanged(ref Objects[idx], InUse[idx]);
            TickablesIndices.Remove(idx);
            return;
        }
        Debug.LogWarning($"Tried to free unknown object '{obj}'!");
    }

    public void Tick(float deltaTime)
    {
        if (Objects == null)
        {
            return;
        }

        foreach (int idx in TickablesIndices)
        {
            Objects[idx].Tick(deltaTime);

            LifeTimes[idx] -= deltaTime;
            if (LifeTimes[idx] <= 0f)
            {
                InUse[idx] = false;
                ObjectStateChanged(ref Objects[idx], InUse[idx]);
            }
        }
    }

    public void TickPhysics(float deltaTime)
    {
        foreach (int idx in TickablesIndices)
        {
            Objects[idx].TickPhysics(deltaTime);
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

    public PhxComponentPool(T prefab, string rootName, int size, float maxLifeTime = float.PositiveInfinity)
        : base(size, maxLifeTime)
    {
        Debug.Assert(prefab != null);

        Prefab = prefab;
        Root = new GameObject(rootName);
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