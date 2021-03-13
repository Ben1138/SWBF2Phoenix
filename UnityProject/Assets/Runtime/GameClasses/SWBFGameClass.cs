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
    T Value;

    public Ref(T val)
    {
        Value = val;
    }

    public void Set(object val)
    {
        Value = (T)val;
    }

    //public static implicit operator Ref<T>(T value)
    //{
    //    return new Ref<T>(value);
    //}

    public static implicit operator T(Ref<T> refVal)
    {
        return refVal.Value;
    }
}

/// <summary>
/// Stores references to class members
/// </summary>
public sealed class PropertyDB
{
    Dictionary<string, VariableRef> Properties = new Dictionary<string, VariableRef>();

    class VariableRef
    {
        public Action<object> Set { get; private set; }
        public VariableRef(Action<object> setter)
        {
            Set = setter;
        }
    }

    public void Register<T>(string propName, T variable) where T : Ref
    {
        Properties.Add(propName, new VariableRef(
            val => { variable.Set(val); })
        );
    }

    public void SetProperty(string propName, object propValue)
    {
        if (Properties.TryGetValue(propName, out VariableRef variable))
        {
            variable.Set(propValue);
        }
    }
}

public abstract class ISWBFInstance : MonoBehaviour
{
    protected PropertyDB P { get; private set; } = new PropertyDB();
    public abstract void InitInstance(Instance inst, ISWBFClass classProperties);
}

public abstract class ISWBFClass
{
    protected PropertyDB P { get; private set; } = new PropertyDB();
    public abstract void InitClass(EntityClass ec);
}

//public class OdfClass
//{
//    Dictionary<string, string[]> Properties = new Dictionary<string, string[]>();

//    public bool GetProperty(string propName, out string[] propValues)
//    {
//        return Properties.TryGetValue(propName, out propValues);
//    }

//    public bool GetProperty<T>(string propName, out T propValue)
//    {
//        if (Properties.TryGetValue(propName, out string[] propValues) && propValues.Length > 0)
//        {
//            propValue = (T)Convert.ChangeType(propValues[0], typeof(T), CultureInfo.InvariantCulture);
//            return true;
//        }
//        propValue = default;
//        return false;
//    }
//}

public class ClassDB
{
    Dictionary<string, ISWBFClass> OdfClasses = new Dictionary<string, ISWBFClass>();
}