
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public static class PhxUtils
{
	public static Vector3 Vec3FromString(string val)
	{
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector3 vOut = new Vector3();
        for (int i = 0; i < 3 && i < v.Length; i++)
        {
            vOut[i] = float.Parse(v[i]);
        }
        return vOut;
    }

	public static Vector2 Vec2FromString(string val)
    {
        string[] v = val.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
        Vector2 vOut = new Vector2();
        for (int i = 0; i < 2; i++)
        {
            vOut[i] = float.Parse(v[i]);
        }
        return vOut;
    }
}
