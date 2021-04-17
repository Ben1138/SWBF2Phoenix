using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;

public static class PhxAnimationLoader
{
    public static Container Con;

    static Dictionary<uint, CraClip> ClipDB = new Dictionary<uint, CraClip>();

    static readonly float[] ComponentMultipliers = {  -1.0f,
                                                      1.0f,
                                                     1.0f,
                                                     -1.0f,
                                                      -1.0f,
                                                      1.0f,
                                                     1.0f  };

    static PhxAnimationLoader()
    {
        CraSettings.BoneHashFunction = (string str) => (int)HashUtils.GetCRC(str);
    }

    public static void ClearDB()
    {
        ClipDB.Clear();
    }

    public static CraClip Import(string bankName, string animName)
    {
        return Import(bankName, HashUtils.GetCRC(animName), animName);
    }

    public static CraClip Import(string bankName, uint animNameCRC, string clipNaming=null)
    {
        CraClip clip;

        uint animID = HashUtils.GetCRC(bankName) * animNameCRC;
        if (ClipDB.TryGetValue(animID, out clip))
        {
            return clip;
        }

        AnimationBank bank = Con.Get<AnimationBank>(bankName);
        if (bank == null)
        {
            Debug.LogError($"Cannot find AnimationBank '{bankName}'!");
            return null;
        }

        if (!bank.GetAnimationMetadata(animNameCRC, out int numFrames, out int numBones))
        {
            Debug.LogError($"Cannot find Animation '{animNameCRC}' in AnimationBank '{bankName}'!");
            return null;
        }

        clip = new CraClip();
        clip.Name = string.IsNullOrEmpty(clipNaming) ? animNameCRC.ToString() : clipNaming;

        uint[] boneCRCs = bank.GetBoneCRCs();
        clip.Bones = new CraBone[boneCRCs.Length];
        for (int i = 0; i < boneCRCs.Length; ++i)
        {
            CraBone bone = new CraBone();
            bone.BoneHash = (int)boneCRCs[i];
            bone.Curve = new CraTransformCurve();

            for (uint j = 0; j < 7; ++j)
            {
                if (!bank.GetCurve(animNameCRC, boneCRCs[i], j, out ushort[] indices, out float[] values))
                {
                    Debug.LogWarning($"Getting curve in animation '{animNameCRC}' of bone '{boneCRCs[i]}' at component '{j}' failed!");
                    continue;
                }

                Debug.Assert(indices.Length == values.Length);

                for (int k = 0; k < indices.Length; ++k)
                {
                    int index = (int)indices[k];
                    float time = index < numFrames ? index / 30.0f : numFrames / 30.0f;
                    float value = values[k] * ComponentMultipliers[j];

                    bone.Curve.Curves[j].EditKeys.Add(new CraKey(time, value));
                }
            }

            clip.Bones[i] = bone;
        }
        clip.Bake(60f);
        ClipDB.Add(animID, clip);
        return clip;
    }
}
