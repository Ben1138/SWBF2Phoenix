using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;


public static class PhxAnimLoader
{
    public const float BakeFPS = 120f;

    public static Container Con;

    static Dictionary<uint, CraClip> ClipDB = new Dictionary<uint, CraClip>();
    static Dictionary<CraClip, RootMotion> RootMotions = new Dictionary<CraClip, RootMotion>();

    static readonly uint Hash_dummyroot = HashUtils.GetCRC("dummyroot");


    struct RootMotion
    {
        // First array index is the channel. Channels:
        // 0 : rot X
        // 1 : rot Y
        // 2 : rot Z
        // 3 : rot W
        // 4 : pos X
        // 5 : pos Y
        // 6 : pos Z
        public float[][] Times;
        public float[][] Values;
    }

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
        RootMotions.Clear();
    }

    public unsafe static PhxTransform GetRootMotion(CraClip clip, float time)
    {
        if (RootMotions.TryGetValue(clip, out RootMotion rootMotion) && rootMotion.Times.Length > 0)
        {
            Debug.Assert(rootMotion.Times.Length == rootMotion.Values.Length);

            float* channelValues = stackalloc float[7];
            for (int i = 0; i < 7; i++)
            {
                int startIdx = 0;
                for (int j = 0; j < rootMotion.Times[i].Length; j++)
                {
                    if (rootMotion.Times[i][j] <= time)
                    {
                        startIdx = j;
                    }
                    else // rootMotion.Times[i][j] > time
                    {
                        break; // startIdx found
                    }
                }

                int endIdx = Mathf.Min(startIdx + 1, rootMotion.Times[i].Length - 1);
                if (startIdx == endIdx)
                {
                    channelValues[i] = rootMotion.Values[i][endIdx];
                }
                else
                {
                    float t = (time - rootMotion.Times[i][startIdx]) / (rootMotion.Times[i][endIdx] - rootMotion.Times[i][startIdx]);
                    Debug.Assert(t >= 0f && t <= 1f);
                    channelValues[i] = Mathf.Lerp(rootMotion.Values[i][startIdx], rootMotion.Values[i][endIdx], t);
                }
            }

            return new PhxTransform
            {
                Rotation = new Quaternion (channelValues[0], channelValues[1], channelValues[2], channelValues[3]),
                Position = new Vector3    (channelValues[4], channelValues[5], channelValues[6]),
            };
        }
        return PhxTransform.None;
    }

    public static PhxTransform GetRootMotionDelta(CraClip clip, float timeStart, float timeEnd)
    {
        Debug.Assert(timeStart < timeEnd);
        PhxTransform start = GetRootMotion(clip, timeStart);
        PhxTransform end   = GetRootMotion(clip, timeEnd);

        return new PhxTransform
        {
            Position = end.Position - start.Position,
            Rotation = end.Rotation * Quaternion.Inverse(start.Rotation) // TODO: Not sure whether the order should be flipped
        };     
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

    public static CraClip SearchImport(string bankName, string animName)
    {
        if (bankName == "human")
        {
            return SearchImport(HUMANM_BANKS, animName);
        }
        return Import(bankName, animName);
    }

    public static CraClip SearchImport(string[] animBanks, string animName)
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

    public static CraClip Import(string bankName, string animName)
    {
        uint animNameCRC = HashUtils.GetCRC(animName);
        uint animID      = HashUtils.GetCRC(bankName) * animNameCRC;
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
        srcClip.Name = animName;

        uint[] boneCRCs = bank.GetBoneCRCs(animNameCRC);
        RootMotion rootMotion = new RootMotion();

        List<CraBone> bones = new List<CraBone>();
        for (int i = 0; i < boneCRCs.Length; ++i)
        {
            if (boneCRCs[i] == Hash_dummyroot)
            {
                ushort[][] indices = new ushort[7][];
                rootMotion.Times   = new float [7][];
                rootMotion.Values  = new float [7][];

                for (uint j = 0; j < 7; ++j)
                {
                    if (!bank.GetCurve(animNameCRC, boneCRCs[i], j, out indices[j], out rootMotion.Values[j]))
                    {
                        Debug.LogWarning($"Getting curve in animation '{animNameCRC}' of bone '{boneCRCs[i]}' at component 'X' failed!");
                        continue;
                    }
                    Debug.Assert(indices[j].Length == rootMotion.Values[j].Length);

                    rootMotion.Times[j] = new float[indices[j].Length];
                    for (uint k = 0; k < indices[j].Length; ++k)
                    {
                        rootMotion.Times[j][k] = indices[j][k] / 30.0f;
                    }
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
#if UNITY_EDITOR
        clip.SetName(animName);
#endif
        ClipDB.Add(animID, clip);

        if (rootMotion.Times != null)
        {
            Debug.Assert(rootMotion.Values != null);
            Debug.Assert(!RootMotions.ContainsKey(clip));
            RootMotions.Add(clip, rootMotion);
        }

        return clip;
    }
}
