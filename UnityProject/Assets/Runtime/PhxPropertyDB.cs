using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using LibSWBF2.Wrappers;

/// <summary>
/// Stores references to properties.
/// Properties are case insensitive!
/// </summary>
public sealed class PhxPropertyDB
{
    static PhxRuntimeScene RTS => PhxGameRuntime.GetScene();

    Dictionary<string, PhxPropRef> Properties = new Dictionary<string, PhxPropRef>();


    public T Get<T>(string propName) where T : PhxPropRef
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out PhxPropRef value))
        {
            return (T)value;
        }
        return default;
    }

    public void Register<T>(string propName, T variable) where T : PhxPropRef
    {
        Properties.Add(propName.ToLowerInvariant(), variable);
    }

    public void SetProperty(string propName, object propValue)
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out PhxPropRef variable))
        {
            variable.Set(propValue);
            return;
        }
        Debug.LogWarningFormat("Could not find property '{0}'!", propName);
    }

    public static void AssignProp(ISWBFProperties instOrClass, string propName, PhxPropRef value)
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
        if (destType == typeof(PhxRegion))
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
        else if (destType == typeof(SWBFPath))
        {
            outVal = Convert.ChangeType(WorldLoader.Instance.ImportPath(value), destType, CultureInfo.InvariantCulture);
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