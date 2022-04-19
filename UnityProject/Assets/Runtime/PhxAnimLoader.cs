using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;


public static class PhxAnimLoader
{
    public const float BakeFPS = 120f;

    public static Container Con;

    static Dictionary<uint, CraClip> ClipDB = new Dictionary<uint, CraClip>();
    static Dictionary<CraClip, Vector3> RootMotionVelocities = new Dictionary<CraClip, Vector3>();

    static readonly uint Hash_dummyroot = HashUtils.GetCRC("dummyroot");

    static readonly float[] ComponentMultipliers = 
    {
        -1.0f,
         1.0f,
         1.0f,
        -1.0f,
        -1.0f,
         1.0f,
         1.0f  
    };

    static readonly string[] HUMANM_BANKS =
    {
        "human_0",
        "human_1",
        "human_2",
        "human_3",
        "human_4",
        "human_sabre"
    };

    public static void ClearDB()
    {
        ClipDB.Clear();
        RootMotionVelocities.Clear();
    }

    public static Vector3 GetRootMotionVelocity(CraClip clip)
    {
        if (RootMotionVelocities.TryGetValue(clip, out Vector3 rootMotionVelocity))
        {
            return rootMotionVelocity;
        }
        return Vector3.zero;
    }

    public static bool Exists(string bankName, string animName)
    {
        if (bankName == "human")
        {
            return Exists(HUMANM_BANKS, animName);
        }
        return Exists(bankName, HashUtils.GetCRC(animName));
    }

    public static bool Exists(string[] animBanks, string animName)
    {
        for (int i = 0; i < animBanks.Length; ++i)
        {
            if (Exists(animBanks[i], animName))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Exists(string bankName, uint animNameCRC)
    {
        uint animID = HashUtils.GetCRC(bankName) * animNameCRC;
        if (ClipDB.TryGetValue(animID, out CraClip clip))
        {
            return true;
        }

        AnimationBank bank = Con.Get<AnimationBank>(bankName);
        if (bank == null)
        {
            return false;
        }

        return bank.GetAnimationMetadata(animNameCRC, out _, out _);
    }

    public static CraClip Import(string bankName, string animName)
    {
        if (bankName == "human")
        {
            return Import(HUMANM_BANKS, animName);
        }
        if (animName.Contains("runforward"))
        {
            Debug.Log("NOOOOOOOO");
        }
        return Import(bankName, HashUtils.GetCRC(animName), animName);
    }

    public static CraClip Import(string[] animBanks, string animName)
    {
        CraClip clip = CraClip.None;
        for (int i = 0; i < animBanks.Length; ++i)
        {
            if (Exists(animBanks[i], animName))
            {
                clip = Import(animBanks[i], animName);
                Debug.Assert(clip.IsValid());
                break;
            }
        }
        return clip;
    }

    public static CraClip Import(string bankName, uint animNameCRC, string clipNameOverride=null)
    {
        uint animID = HashUtils.GetCRC(bankName) * animNameCRC;
        if (ClipDB.TryGetValue(animID, out CraClip clip))
        {
            return clip;
        }

        AnimationBank bank = Con.Get<AnimationBank>(bankName);
        if (bank == null)
        {
            //Debug.LogError($"Cannot find AnimationBank '{bankName}'!");
            return CraClip.None;
        }

        if (!bank.GetAnimationMetadata(animNameCRC, out int numFrames, out int numBones))
        {
            //Debug.LogError($"Cannot find Animation '{animNameCRC}' in AnimationBank '{bankName}'!");
            return CraClip.None;
        }

        CraSourceClip srcClip = new CraSourceClip();
        srcClip.Name = string.IsNullOrEmpty(clipNameOverride) ? animNameCRC.ToString() : clipNameOverride;

        uint[] boneCRCs = bank.GetBoneCRCs(animNameCRC);
        Vector3 rootMotionVelocity = Vector3.zero;

        List<CraBone> bones = new List<CraBone>();
        for (int i = 0; i < boneCRCs.Length; ++i)
        {
            if (boneCRCs[i] == Hash_dummyroot)
            {
                ushort[][] indices = new ushort[3][];
                float[][] values = new float[3][];

                for (uint j = 0; j < 3; ++j)
                {
                    if (!bank.GetCurve(animNameCRC, boneCRCs[i], j + 4, out indices[j], out values[j]))
                    {
                        Debug.LogWarning($"Getting curve in animation '{animNameCRC}' of bone '{boneCRCs[i]}' at component 'X' failed!");
                        continue;
                    }
                    Debug.Assert(indices[j].Length == values[j].Length);
                }

                // Assumption: Root motions are always linear
                if (values[0].Length > 0 && values[2].Length > 0)
                {
                    int endX = values[0].Length - 1;
                    int endY = values[2].Length - 1;
                    Vector2 start = new Vector2(values[0][0], values[2][0]);
                    Vector2 end = new Vector2(values[0][endX], values[2][endY]);
                    float duration = numFrames / 30f;
                    rootMotionVelocity = (start - end) / duration;

                    Debug.Log($"Clip '{animNameCRC}' has root motion: {rootMotionVelocity.magnitude}");
                }
            }
            else
            {
                CraBone bone = new CraBone();
                bone.BoneHash = (int)boneCRCs[i];
                bone.Curve = new CraSourceTransformCurve();

                for (int j = 0; j < 7; ++j)
                {
                    if (!bank.GetCurve(animNameCRC, boneCRCs[i], (uint)j, out ushort[] indices, out float[] values))
                    {
                        Debug.LogWarning($"Getting curve in animation '{animNameCRC}' of bone '{boneCRCs[i]}' at component '{j}' failed!");
                        continue;
                    }

                    Debug.Assert(indices.Length == values.Length);

                    for (int k = 0; k < indices.Length; ++k)
                    {
                        int index = indices[k];
                        float time = index < numFrames ? index / 30.0f : numFrames / 30.0f;
                        float value = values[k] * ComponentMultipliers[j];

                        bone.Curve.Curves[j].EditKeys.Add(new CraKey(time, value));
                    }
                }

                bones.Add(bone);
            }

        }
        srcClip.SetBones(bones.ToArray());
        srcClip.Bake(BakeFPS);
        clip = CraClip.CreateNew(srcClip);
        ClipDB.Add(animID, clip);

        Debug.Assert(!RootMotionVelocities.ContainsKey(clip));
        RootMotionVelocities.Add(clip, rootMotionVelocity);

        return clip;
    }
}
