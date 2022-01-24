
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


    /*
    A custom parser is needed because mungers do not enforce any syntax rules.  The game will often read strange syntax
    correctly without crashing.

    Some examples: -3.0-3.0-3.0 is a valid vec3 which the game will interpret correctly, despite the lack of spacing
                   -3_0 will be interpreted as -3.0
                   -3.3.0 will be interpreted as -3.3
    */
    /*
    private static float FloatFromString(string FloatString, int StartIndex, out int EndIndex)
    {
        EndIndex = StartIndex;
        if (StartIndex >= FloatString.Length) return 0f;

        bool FoundPoint = false;
        bool FoundSign  = false;
        bool ParseStarted = false;

        float Divisor = 1f;
        float RawNum = 0f;

        char CurrChar;

        int i;

        for (i = StartIndex; i < FloatString.Length; i++) {
            
            CurrChar = FloatString.charAt(i);

            if (CurrChar == '.') 
            {
                ParseStarted = true;

                if (FoundPoint) 
                {
                    break;
                }
                else 
                {
                    FoundPoint = true;
                    continue;
                }
            }
            else if (CurrChar == '-')
            {
                if (ParseStarted)
                {
                    break;
                }
                else 
                {
                    ParseStarted = true;
                    FoundSign = true;
                    continue;
                }
            }
            else if (Char.IsNumber(CurrChar))
            {
                ParseStarted = true;

                RawNum *= 10.0f;
                RawNum += (float) (CurrChar - '0');

                if (FoundPoint)
                {
                    Divisor *= 10f;
                }                
            }
            else if (CurrChar == '/')
            {
                break;
            }
            else 
            {
                if (ParseStarted)
                {
                    break;
                }
                else 
                {
                    continue;
                }
            }
        }

        EndIndex = i - 1;
        RawNum /= Divisor;
        return FoundSign ? -RawNum : RawNum;
    }


    public static float FloatFromString(string FloatString)
    {
        return FloatFromString(FloatString, 0, out _);
    }
    */


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
}
