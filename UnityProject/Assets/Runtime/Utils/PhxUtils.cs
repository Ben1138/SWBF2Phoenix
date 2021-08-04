
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public static class PhxUtils
{
    public static Vector4 Vec4FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector4 vOut = new Vector4();
        for (int i = 0; i < 4 && i < v.Length; i++)
        {
            try {
                vOut[i] = float.Parse(v[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch 
            {
                vOut[i] = 0f;
                Debug.LogErrorFormat("Failed to convert {0} to Vector4", val);
            }
        }
        return vOut;
    }

	public static Vector3 Vec3FromString(string val)
	{
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector3 vOut = new Vector3();
        for (int i = 0; i < 3 && i < v.Length; i++)
        {
            try {
                vOut[i] = float.Parse(v[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch 
            {
                vOut[i] = 0f;
                Debug.LogErrorFormat("Failed to convert {0} to Vector3", val);
            }
        }
        return vOut;
    }

	public static Vector2 Vec2FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector2 vOut = new Vector2();
        for (int i = 0; i < 2; i++)
        {
            try {
                vOut[i] = float.Parse(v[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            catch 
            {
                vOut[i] = 0f;
                Debug.LogErrorFormat("Failed to convert {0} to Vector2", val);
            }
        }
        return vOut;
    }


    // Unfortunately, a custom parser is needed because mungers do not enforce any
    // standards of string-float representations
    static char[] FloatChars = new char[100];
    static int NumDigits = 0;
    static int FloatPosition = 0;
    static bool Positive;
    public static float FloatFromString(string FloatString)
    {
        float r;
        try 
        {
            r = float.Parse(FloatString, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch 
        {
            Debug.LogErrorFormat("Failed to parse float from: {0}", FloatString);
            r = 0f;
        }

        return r;
    }


    public static void SanitizeEuler(ref Vector3 euler)
    {
        while (euler.x > 180f) euler.x -= 360f;
        while (euler.y > 180f) euler.y -= 360f;
        while (euler.z > 180f) euler.z -= 360f;
        while (euler.x < -180f) euler.x += 360f;
        while (euler.y < -180f) euler.y += 360f;
        while (euler.z < -180f) euler.z += 360f;
    }


    public static void SanitizeEuler(ref Vector2 euler)
    {
        while (euler.x > 180f) euler.x -= 360f;
        while (euler.y > 180f) euler.y -= 360f;
        while (euler.x < -180f) euler.x += 360f;
        while (euler.y < -180f) euler.y += 360f;
    }
}
