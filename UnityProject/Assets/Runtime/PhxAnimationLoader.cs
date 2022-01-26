using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;

public static class PhxAnimationLoader
{
    public static Container Con;

    static Dictionary<uint, CraClip> ClipDB = new Dictionary<uint, CraClip>();

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

    public static CraClip Import(string bankName, uint animNameCRC, string clipNameOverride=null)
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
            //Debug.LogError($"Cannot find Animation '{animNameCRC}' in AnimationBank '{bankName}'!");
            return null;
        }

        clip = new CraClip();
        clip.Name = string.IsNullOrEmpty(clipNameOverride) ? animNameCRC.ToString() : clipNameOverride;

        uint dummyroot = HashUtils.GetCRC("dummyroot");

        uint[] boneCRCs = bank.GetBoneCRCs();
        List<CraBone> bones = new List<CraBone>();
        for (int i = 0; i < boneCRCs.Length; ++i)
        {
            // no root motion
            if (boneCRCs[i] == dummyroot) continue;

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
                    int index = indices[k];
                    float time = index < numFrames ? index / 30.0f : numFrames / 30.0f;
                    float value = values[k] * ComponentMultipliers[j];

                    bone.Curve.Curves[j].EditKeys.Add(new CraKey(time, value));
                }
            }

            bones.Add(bone);
        }
        clip.SetBones(bones.ToArray());
        clip.Bake(120f);
        ClipDB.Add(animID, clip);
        return clip;
    }

    public static CraPlayer CreatePlayer(Transform root, string animBank, string animName, bool loop, string maskBone = null)
    {
        CraClip clip = Import(animBank, animName);
        if (clip == null)
        {
            Debug.LogWarning($"Cannot find animation clip '{animName}' in bank '{animBank}'!");
            return CraPlayer.CreateEmpty();
        }

        CraPlayer player = CraPlayer.CreateNew();
        player.SetClip(clip);

        if (string.IsNullOrEmpty(maskBone))
        {
            player.Assign(root);
        }
        else
        {
            player.Assign(root, new CraMask(true, maskBone));
        }

        player.SetLooping(loop);
        return player;
    }

    public static CraPlayer CreatePlayer(Transform root, string[] animBanks, string animName, bool loop, string maskBone = null)
    {
        CraClip clip = null;
        for (int i = 0; i < animBanks.Length; ++i)
        {
            clip = Import(animBanks[i], animName);
            if (clip != null)
            {
                break;
            }
        }
        if (clip == null)
        {
            Debug.LogWarning($"Cannot find animation clip '{animName}' in any of the specified banks '{animBanks}'!");
            return CraPlayer.CreateEmpty();
        }

        CraPlayer player = CraPlayer.CreateNew();
        player.SetClip(clip);

        if (string.IsNullOrEmpty(maskBone))
        {
            player.Assign(root);
        }
        else
        {
            player.Assign(root, new CraMask(true, maskBone));
        }

        player.SetLooping(loop);
        return player;
    }
}
