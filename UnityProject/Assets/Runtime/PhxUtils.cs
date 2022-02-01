
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;

public static class PhxUtils
{
    public static Vector4 Vec4FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector4 vOut = new Vector4();
        for (int i = 0; i < 4 && i < v.Length; i++)
        {
            vOut[i] = FloatFromString(v[i]);
        }
        return vOut;
    }

	public static Vector3 Vec3FromString(string val)
	{
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector3 vOut = new Vector3();
        for (int i = 0; i < 3 && i < v.Length; i++)
        {
            vOut[i] = FloatFromString(v[i]);        
        }
        return vOut;
    }

	public static Vector2 Vec2FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector2 vOut = new Vector2();
        for (int i = 0; i < 2 && i < v.Length; i++)
        {
            vOut[i] = FloatFromString(v[i]);
        }
        return vOut;
    }

    public static unsafe float FloatFromString(string FloatString)
    {
        Debug.Assert(FloatString.Length < 32);
        char* buffer = stackalloc char[32];
        fixed (char* str = FloatString)
        {
            for (int si = 0, bi = 0, nd = 0; si < FloatString.Length; ++si)
            {
                if (str[si] == '_') continue;     // ignore underscores
                bool decPoint = str[si] == '.';
                if (decPoint) ++nd;
                if (decPoint && nd > 1) continue; // ignore more than one decimal points

                buffer[bi++] = str[si];
            }
        }

        if (!float.TryParse(new string(buffer), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            result = 0f;
            Debug.LogErrorFormat("Failed to parse a float from: {0}", FloatString);
        }
        return result;
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


    public static Transform FindTransformRecursive(Transform root, string transformName)
    {
        Debug.Assert(root != null);
        if (root.name == transformName)
        {
            return root;
        }
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform child = FindTransformRecursive(root.GetChild(i), transformName);
            if (child != null)
            {
                return child;
            }
        }
        return null;
    }

    // format string using SWBFs C-style printf format (%s, ...)
    public static string Format(string fmt, params object[] args)
    {
        fmt = ConvertFormat(fmt);
        return string.Format(fmt, args);
    }

    // convert C-style printf format to C# format
    static string ConvertFormat(string swbfFormat)
    {
        int GetNextIndex(string format)
        {
            int idx = format.IndexOf("%s");
            if (idx >= 0) return idx;

            idx = format.IndexOf("%i");
            if (idx >= 0) return idx;

            idx = format.IndexOf("%d");
            if (idx >= 0) return idx;

            idx = format.IndexOf("%f");
            if (idx >= 0) return idx;

            return -1;
        }

        // convert C-style printf format to C# format
        string format = swbfFormat;
        int idx = GetNextIndex(format);
        for (int i = 0; idx >= 0; idx = GetNextIndex(format), ++i)
        {
            string sub = format.Substring(0, idx);
            sub += "{" + i + "}";
            sub += format.Substring(idx + 2, format.Length - idx - 2);
            format = sub;
        }
        return format;
    }
}