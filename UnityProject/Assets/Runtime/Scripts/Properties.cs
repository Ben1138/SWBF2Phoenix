using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Enums;
using JetBrains.Annotations;
using System.Collections;

public interface Ref
{
    void SetFromString(string val);
    void Set(object val);
}

/// <summary>
/// Encapsulates a value to be passable as reference.
/// </summary>
public sealed class Prop<T> : Ref
{
    static RuntimeEnvironment ENV => GameRuntime.GetEnvironment();
    static RuntimeScene RTS => GameRuntime.GetScene();

    public Action<T> OnValueChanged;
    T Value;

    public Prop(T val)
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
        Value = PropertyDB.FromString<T>(val);
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

    public static implicit operator T(Prop<T> refVal)
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
public sealed class MultiProp : Ref, IEnumerator, IEnumerable
{
    static RuntimeEnvironment ENV => GameRuntime.GetEnvironment();


    List<object[]> Values = new List<object[]>();
    Type[] ExpectedTypes;
    int IterPos = -1;


    public MultiProp(params Type[] expectedTypes)
    {
        ExpectedTypes = expectedTypes;
    }

    public T Get<T>(int argIdx)
    {
        return Values.Count > 0 ? (T)Values[0][argIdx] : default;
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
        for (int i = 0; i < vals.Length; ++i)
        {
            split[i] = split[i].Trim();
            try
            {
                vals[i] = PropertyDB.FromString(ExpectedTypes[i], split[i]);
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

    //IEnumerator and IEnumerable require these methods.
    public IEnumerator GetEnumerator()
    {
        return this;
    }

    //IEnumerator
    public bool MoveNext()
    {
        return (++IterPos < Values.Count);
    }

    //IEnumerable
    public void Reset()
    {
        IterPos = 0;
    }

    //IEnumerable
    public object Current => Values[IterPos];
}

/// <summary>
/// Stores references to properties.
/// Properties are case insensitive!
/// </summary>
public sealed class PropertyDB
{
    static RuntimeScene RTS => GameRuntime.GetScene();

    Dictionary<string, Ref> Properties = new Dictionary<string, Ref>();


    public T Get<T>(string propName) where T : Ref
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out Ref value))
        {
            return (T)value;
        }
        return default;
    }

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

    public static void AssignProp<T>(T instOrClass, string propName, Ref value) where T : ISWBFProperties
    {
        if (instOrClass.GetProperty(propName, out string[] outVal))
        {
            for (int i = 0; i < outVal.Length; ++i)
            {
                value.SetFromString(outVal[i]);
            }
        }
    }

    public static T FromString<T>(string value)
    {
        return (T)FromString(typeof(T), value);
    }

    public static object FromString(Type destType, string value)
    {
        object outVal;
        if (destType == typeof(Region))
        {
            outVal = Convert.ChangeType(RTS.GetRegion(value), destType, CultureInfo.InvariantCulture);
        }
        else if (destType == typeof(Texture2D))
        {
            outVal = Convert.ChangeType(TextureLoader.Instance.ImportTexture(value), destType, CultureInfo.InvariantCulture);
        }
        else if (destType == typeof(AudioClip))
        {
            outVal = Convert.ChangeType(SoundLoader.LoadSound(value), destType, CultureInfo.InvariantCulture);
        }
        else if (destType == typeof(bool))
        {
            if (value == "0" || value == "1")
            {
                outVal = value != "0" ? true : false;
            }
            else
            {
                outVal = Convert.ChangeType(value != "0" ? true : false, destType, CultureInfo.InvariantCulture);
            }
        }
        else
        {
            outVal = Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
        }
        return outVal;
    }
}

public abstract class ISWBFInstance : MonoBehaviour
{
    public PropertyDB P { get; private set; } = new PropertyDB();

    public abstract void InitInstance(Instance inst, ISWBFClass classProperties);
}

public abstract class ISWBFInstance<T> : ISWBFInstance where T : ISWBFClass
{
    public T C { get; private set; } = null;


    // Use this as constructor (MonoBehaviour constructors don't get called, and
    // Awake() won't be called until next frame)
    public abstract void Init();

    // Override this method in inheriting instance classes.
    // In this method, bind your custom property events and implement
    // the intended behaviour (e.g. Team.OnValueChanged)
    public abstract void BindEvents();

    public override void InitInstance(Instance inst, ISWBFClass classProperties)
    {
        C = (T)classProperties;

        Init();

        Type type = GetType();
        MemberInfo[] members = type.GetMembers();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(Ref).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                Ref refValue = (Ref)type.GetField(member.Name).GetValue(this);
                P.Register(member.Name, refValue);
            }
        }

        // make sure the instance is listening on property change events
        // before assigning the actual instance property value
        BindEvents();

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(Ref).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                Ref refValue = (Ref)type.GetField(member.Name).GetValue(this);
                PropertyDB.AssignProp(inst, member.Name, refValue);
            }
        }
    }
}

public abstract class ISWBFClass
{
    static RuntimeEnvironment ENV => GameRuntime.GetEnvironment();

    public PropertyDB       P { get; private set; } = new PropertyDB();
    public string           Name { get; private set; }
    public EEntityClassType ClassType { get; private set; }
    public string           LocalizedName { get; private set; }

    public void InitClass(EntityClass ec)
    {
        Name = ec.Name;

        ClassType = ec.ClassType;
        if (ClassType == EEntityClassType.WeaponClass)
        {
            string locPath = "weapons.";
            int splitIdx = Name.IndexOf('_');
            locPath += Name.Substring(0, splitIdx) + ".weap." + Name.Substring(splitIdx + 1).Replace("weap_", "");
            LocalizedName = ENV.GetLocalized(locPath);
        }
        else
        {
            string locPath = "entity.";
            int splitIdx = Name.IndexOf('_');
            locPath += Name.Substring(0, splitIdx) + '.' + Name.Substring(splitIdx + 1);
            LocalizedName = ENV.GetLocalized(locPath);
        }


        Type type = GetType();
        MemberInfo[] members = type.GetMembers();
        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Field && typeof(Ref).IsAssignableFrom(type.GetField(member.Name).FieldType))
            {
                Ref refValue = type.GetField(member.Name).GetValue(this) as Ref;
                P.Register(member.Name, refValue);
                PropertyDB.AssignProp(ec, member.Name, refValue);
            }
        }
    }
}