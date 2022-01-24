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
    static PhxScene RTS => PhxGame.GetScene();

    Dictionary<string, IPhxPropRef> Properties = new Dictionary<string, IPhxPropRef>();


    public T Get<T>(string propName) where T : IPhxPropRef
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out IPhxPropRef value))
        {
            return (T)value;
        }
        return default;
    }

    public void Register<T>(string propName, T variable) where T : IPhxPropRef
    {
        Properties[propName.ToLowerInvariant()] = variable;
    }

    public void SetProperty(string propName, object propValue)
    {
        if (Properties.TryGetValue(propName.ToLowerInvariant(), out IPhxPropRef variable))
        {
            variable.Set(propValue);
            return;
        }
        Debug.LogWarningFormat("Could not find property '{0}'!", propName);
    }

    public static void AssignProp(ISWBFProperties instOrClass, string propName, IPhxPropRef value)
    {
        if (value is PhxMultiProp)
        {
            if (instOrClass.GetProperty(propName, out string[] outVal))
            {
                for (int i = 0; i < outVal.Length; ++i)
                {
                    value.SetFromString(outVal[i]);
                }
            }
        }
        else
        {
            if (instOrClass.GetProperty(propName, out string outVal))
            {
                value.SetFromString(outVal);
            }
        }
    }

    public static T FromString<T>(string value)
    {
        return (T)FromString(typeof(T), value);
    }

    public static object FromString(Type destType, string value)
    {
        //Strip comments
        int index = value.IndexOf("/");
        if (index >= 0)
        {
           value = value.Substring(0, index);
        }

        object outVal;
        if (destType == typeof(PhxClass))
        {
            outVal = RTS.GetClass(value);
        }
        else if (destType == typeof(PhxRegion))
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
            outVal = Convert.ChangeType(RTS.GetPath(value), destType, CultureInfo.InvariantCulture);
        }
        else if (destType == typeof(Vector4))
        {
            outVal = PhxUtils.Vec4FromString(value); 
        }
        else if (destType == typeof(Vector3))
        {
            outVal = PhxUtils.Vec3FromString(value); 
        }
        else if (destType == typeof(Vector2))
        {
            outVal = PhxUtils.Vec2FromString(value); 
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
        else if (destType == typeof(Color))
        {
            string[] vals = value.Split(' ');
            if (vals.Length < 3)
            {
                Debug.LogError($"Expected 3 or 4 color arguments (rgb / rgba), but got {vals.Length}!");
                outVal = Color.black;
            }
            else
            {
                Color outCol = new Color();
                outCol.r = PhxUtils.FloatFromString(vals[0]) / 255f;
                outCol.g = PhxUtils.FloatFromString(vals[1]) / 255f;
                outCol.b = PhxUtils.FloatFromString(vals[2]) / 255f;
                outCol.a = 1f;

                if (vals.Length == 4)
                {
                    outCol.a = PhxUtils.FloatFromString(vals[0]) / 255f;
                }

                outVal = outCol;
            }
        }
        else if (destType == typeof(int))
        {
            float floatVal = PhxUtils.FloatFromString(value);
            outVal = (int) floatVal;
        }
        else if (destType == typeof(float))
        {
            outVal = PhxUtils.FloatFromString(value);                       
        }
        else
        {
            try {
                outVal = Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Failed to convert string {0} to type {1}...", value, destType.ToString());
                outVal = 0;
            }
        }
        return outVal;
    }
}