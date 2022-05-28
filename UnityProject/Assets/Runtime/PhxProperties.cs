using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;
using JetBrains.Annotations;
using System.Collections;

public interface IPhxPropRef
{
    void SetFromString(string val);
    void Set(object val);

    IPhxPropRef ShallowCopy();
}

/// <summary>
/// Encapsulates a value to be passable as reference.
/// </summary>
public sealed class PhxProp<T> : IPhxPropRef
{
    public Action<T> OnValueChanged;
    T Value;

    public PhxProp(T val)
    {
        Value = val;
    }

    public T Get()
    {
        return Value;
    }

    public void SetFromString(string val)
    {
        T oldValue = Value;
        Value = PhxPropertyDB.FromString<T>(val);
        OnValueChanged?.Invoke(oldValue);
    }

    public void Set(object val)
    {
        T oldValue = Value;
        if (val.GetType() == typeof(string))
        {
            SetFromString(val as string);
            return;
        }

        try
        {
            Value = (T)val;
        }
        catch
        {
            Value = (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
        }
        OnValueChanged?.Invoke(oldValue);
    }

    public void Set(T val)
    {
        T oldValue = Value;
        Value = val;
        OnValueChanged?.Invoke(oldValue);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public IPhxPropRef ShallowCopy()
    {
        return (IPhxPropRef)MemberwiseClone();
    }

    public static implicit operator T(PhxProp<T> refVal)
    {
        return refVal.Value;
    }
}

/// <summary>
/// Encapsulates multiple values of the same property name to be passable as reference.<br/>
/// Example from com_bldg_controlzone:<br/>
///     AmbientSound = "all com_blg_commandpost_goodie defer"<br/>
///     AmbientSound = "cis com_blg_commandpost_baddie defer"<br/>
///     AmbientSound = "imp com_blg_commandpost_baddie defer"<br/>
///     AmbientSound = "rep com_blg_commandpost_goodie defer"
/// </summary>
public sealed class PhxMultiProp : IPhxPropRef
{
    public List<object[]> Values { get; private set; } = new List<object[]>();
    Type[] ExpectedTypes;


    public PhxMultiProp(params Type[] expectedTypes)
    {
        ExpectedTypes = expectedTypes;
    }

    public T Get<T>(int argIdx)
    {
        return Values.Count > 0 ? (T)Values[0][argIdx] : default;
    }

    //Should mix both with an optional argument (public T Get<T>(int argIdx, int secIdx = 0))
    public T Get<T>(int argIdx, int secIdx)
    {
        return Values.Count > 0 ? (T)Values[secIdx][argIdx] : default;
    }

    public int GetCount()
    {
        return Values.Count;
    }

    // "Set" here actually adds another prop entry
    public void SetFromString(string val)
    {
        object[] vals = new object[ExpectedTypes.Length];
        List<string> split = new List<string>(val.Split(' '));
        split.RemoveAll(str => string.IsNullOrEmpty(str));
        if (split.Count > ExpectedTypes.Length)
        {
            Debug.LogWarning($"Encountered more property args ({split.Count}) than expected ({ExpectedTypes.Length})! Ignoring surplus...");
        }
        for (int i = 0; i < ExpectedTypes.Length; ++i)
        {
            if (i >= split.Count)
            {
                vals[i] = "";
                continue;
            }

            split[i] = split[i].Trim();
            try
            {
                vals[i] = PhxPropertyDB.FromString(ExpectedTypes[i], split[i]);
            }
            catch
            {
                Debug.LogError($"Property arg value '{split[i]}' does not match expected arg type '{ExpectedTypes[i]}'");
            }
        }
        Values.Add(vals);
    }

    // "Set" here actually adds another prop entry
    public void Set(object val)
    {
        if (val.GetType() != typeof(string))
        {
            Debug.LogError("MultiProp's can only be added from strings!");
            return;
        }
        SetFromString(val as string);
    }

    public IPhxPropRef ShallowCopy()
    {
        return (IPhxPropRef)MemberwiseClone();
    }
}


/// <summary>
/// Properties within a property section are NOT registered to the 
/// properties database and are as such not setable from lua!
/// A property section may or may not start with a header (section name).
/// When no section name is present, section properties are enumerated like:
///     WeaponName1
///     WeaponName2
///     ...
/// </summary>
public class PhxPropertySection : IEnumerable
{
    public (string, IPhxPropRef)[] Properties { get; private set; }
    public uint NameHash { get; private set; }

    Dictionary<string, IPhxPropRef>[] Sections;

    public PhxPropertySection(string name, params (string, IPhxPropRef)[] properties)
    {
        NameHash = LibSWBF2.Utils.HashUtils.GetFNV(name);
        Properties = properties;
    }

    public void SetSections(Dictionary<string, IPhxPropRef>[] sections)
    {
        Sections = sections;
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Sections.Length; ++i)
        {
            yield return Sections[i];
        }
    }

    public bool ContainsProperty(uint propNameHash, out int propIdx, out int sectionIdx)
    {
        for (int i = 0; i < Properties.Length; ++i)
        {
            string sectionPropName = Properties[i].Item1;
            // Since property sections sometimes don't have a header, their property names
            // will have a number postfix on them instead.
            for (int j = 0; j < 10; ++j)
            {
                if (propNameHash == HashUtils.GetFNV($"{sectionPropName}{(j == 0 ? "" : j.ToString())}"))
                {
                    propIdx = i;
                    sectionIdx = j;
                    return true;
                }
            }
        }
        propIdx = -1;
        sectionIdx = -1;
        return false;
    }
}


// Property order actually matters here!
public class PhxImpliedSection : IEnumerable
{
    public (string, IPhxPropRef)[] Properties { get; private set; }

    public Dictionary<string, IPhxPropRef>[] Sections { get; private set; }


    List<uint> PropertyHashes;


    public PhxImpliedSection(params (string, IPhxPropRef)[] properties)
    {
        Properties = properties;
        PropertyHashes = new List<uint>();
        foreach (var prop in Properties)
        {
            PropertyHashes.Add(HashUtils.GetFNV(prop.Item1));
        }
    }


    public bool HasProperty(uint Hash, out string PropName)
    {
        PropName = null;

        if (PropertyHashes.Contains(Hash))
        {
            foreach (var propPair in Properties)
            {
                if (HashUtils.GetFNV(propPair.Item1) == Hash)
                {
                    PropName = propPair.Item1;
                    break;
                }
            }

            return true;
        }
        else 
        {
            return false;
        }
    }


    public void SetSections(Dictionary<string, IPhxPropRef>[] sections)
    {
        Sections = sections;
    }


    public Dictionary<string, IPhxPropRef> GetDefault()
    {
        Dictionary<string, IPhxPropRef> DefaultSectionCopy = new Dictionary<string, IPhxPropRef>();
        foreach ((string, IPhxPropRef) Pair in Properties)
        {
            DefaultSectionCopy[Pair.Item1] = Pair.Item2.ShallowCopy();
        }
        return DefaultSectionCopy;
    }


    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Sections.Length; ++i)
        {
            yield return Sections[i];
        }
    }
}


