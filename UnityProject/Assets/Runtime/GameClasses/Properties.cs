using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using LibSWBF2.Wrappers;
using JetBrains.Annotations;


public interface Ref
{
    void Set(object val);
}

/// <summary>
/// Encapsules a value to be passable as reference.
/// </summary>
public sealed class Ref<T> : Ref
{
    public Action OnValueChanged;
    T Value;

    public Ref(T val)
    {
        Value = val;
    }

    public T Get()
    {
        return Value;
    }

    public void Set(object val)
    {
        try
        {
            Value = (T)val;
        }
        catch
        {
            Value = (T)Convert.ChangeType(val, typeof(T));
        }
        OnValueChanged?.Invoke();
    }

    public void Set(T val)
    {
        Value = val;
        OnValueChanged?.Invoke();
    }

    public static implicit operator T(Ref<T> refVal)
    {
        return refVal.Value;
    }
}

/// <summary>
/// Stores references to properties.
/// Properties are case insensitive!
/// </summary>
public sealed class PropertyDB
{
    Dictionary<string, Ref> Properties = new Dictionary<string, Ref>();

    public void Register<T>(string propName, T variable) where T : Ref
    {
        Properties.Add(propName.ToLowerInvariant(), variable);
    }

    public void SetProperty(string propName, object propValue)
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out Ref variable))
        {
            variable.Set(propValue);
            return;
        }
        Debug.LogWarningFormat("Could not find property '{0}'!", propName);
    }
}

public abstract class ISWBFInstance : MonoBehaviour
{
    public PropertyDB P { get; private set; } = new PropertyDB();
    public abstract void InitInstance(Instance inst, ISWBFClass classProperties);
}

public abstract class ISWBFClass
{
    public PropertyDB P { get; private set; } = new PropertyDB();
    public string Name { get; private set; }
    public virtual void InitClass(EntityClass ec)
    {
        Name = ec.Name;
    }
}