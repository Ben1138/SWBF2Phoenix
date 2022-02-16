using System.Collections.Generic;
using UnityEngine;
using LibSWBF2.Wrappers;
using LibSWBF2.Utils;

public static class PhxAnimLoader
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
    }

    public static CraClip Import(string bankName, string animName)
    {
        if (bankName == "human")
        {
            return Import(HUMANM_BANKS, animName);
        }
        return Import(bankName, HashUtils.GetCRC(animName), animName);
    }

    public static CraClip Import(string[] animBanks, string animName)
    {
        CraClip clip = CraClip.None;
        for (int i = 0; i < animBanks.Length; ++i)
        {
            clip = Import(animBanks[i], animName);
            if (clip.IsValid())
            {
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

        if (animNameCRC == HashUtils.GetCRC("human_sabre_sprint_full"))
        {
            Debug.Log("");
        }

        uint dummyroot = HashUtils.GetCRC("dummyroot");
        uint[] boneCRCs = bank.GetBoneCRCs(animNameCRC);

        //if (animNameCRC == HashUtils.GetCRC("human_sabre_sprint_full"))
        //{
        //    Dictionary<uint, string> hashes = new Dictionary<uint, string>()
        //    {
        //        { HashUtils.GetCRC("bone_root"), "bone_root" },
        //        { HashUtils.GetCRC("root_a_spine"), "root_a_spine" },
        //        { HashUtils.GetCRC("bone_a_spine"), "bone_a_spine" },
        //        { HashUtils.GetCRC("bone_b_spine"), "bone_b_spine" },
        //        { HashUtils.GetCRC("bone_ribcage"), "bone_ribcage" },
        //        { HashUtils.GetCRC("eff_ribcage"), "eff_ribcage" },
        //        { HashUtils.GetCRC("root_l_clavicle"), "root_l_clavicle" },
        //        { HashUtils.GetCRC("bone_l_clavicle"), "bone_l_clavicle" },
        //        { HashUtils.GetCRC("eff_l_clavicle"), "eff_l_clavicle" },
        //        { HashUtils.GetCRC("root_l_upperarm"), "root_l_upperarm" },
        //        { HashUtils.GetCRC("bone_l_upperarm"), "bone_l_upperarm" },
        //        { HashUtils.GetCRC("bone_l_forearm"), "bone_l_forearm" },
        //        { HashUtils.GetCRC("eff_l_forearm"), "eff_l_forearm" },
        //        { HashUtils.GetCRC("root_l_hand"), "root_l_hand" },
        //        { HashUtils.GetCRC("bone_l_hand"), "bone_l_hand" },
        //        { HashUtils.GetCRC("root_r_clavicle"), "root_r_clavicle" },
        //        { HashUtils.GetCRC("bone_r_clavicle"), "bone_r_clavicle" },
        //        { HashUtils.GetCRC("eff_r_clavicle"), "eff_r_clavicle" },
        //        { HashUtils.GetCRC("root_r_upperarm"), "root_r_upperarm" },
        //        { HashUtils.GetCRC("bone_r_upperarm"), "bone_r_upperarm" },
        //        { HashUtils.GetCRC("bone_r_forearm"), "bone_r_forearm" },
        //        { HashUtils.GetCRC("eff_r_forearm"), "eff_r_forearm" },
        //        { HashUtils.GetCRC("root_r_hand"), "root_r_hand" },
        //        { HashUtils.GetCRC("bone_r_hand"), "bone_r_hand" },
        //        { HashUtils.GetCRC("root_neck"), "root_neck" },
        //        { HashUtils.GetCRC("bone_neck"), "bone_neck" },
        //        { HashUtils.GetCRC("bone_head"), "bone_head" },
        //        { HashUtils.GetCRC("bone_pelvis"), "bone_pelvis" },
        //        { HashUtils.GetCRC("root_r_thigh"), "root_r_thigh" },
        //        { HashUtils.GetCRC("bone_r_thigh"), "bone_r_thigh" },
        //        { HashUtils.GetCRC("bone_r_calf"), "bone_r_calf" },
        //        { HashUtils.GetCRC("eff_r_calf"), "eff_r_calf" },
        //        { HashUtils.GetCRC("root_r_foot"), "root_r_foot" },
        //        { HashUtils.GetCRC("bone_r_foot"), "bone_r_foot" },
        //        { HashUtils.GetCRC("bone_r_toe"), "bone_r_toe" },
        //        { HashUtils.GetCRC("root_l_thigh"), "root_l_thigh" },
        //        { HashUtils.GetCRC("bone_l_thigh"), "bone_l_thigh" },
        //        { HashUtils.GetCRC("bone_l_calf"), "bone_l_calf" },
        //        { HashUtils.GetCRC("eff_l_calf"), "eff_l_calf" },
        //        { HashUtils.GetCRC("root_l_foot"), "root_l_foot" },
        //        { HashUtils.GetCRC("bone_l_foot"), "bone_l_foot" },
        //        { HashUtils.GetCRC("bone_l_toe"), "bone_l_toe" },
        //    };

        //    for (int i = 0; i < boneCRCs.Length; ++i)
        //    {
        //        if (hashes.TryGetValue(boneCRCs[i], out string str))
        //            Debug.Log($"Animated Bone: {str}");
        //        else
        //            Debug.Log($"Animated Bone: {boneCRCs[i]}");
        //    }
        //}

        List<CraBone> bones = new List<CraBone>();
        for (int i = 0; i < boneCRCs.Length; ++i)
        {
            // no root motion
            if (boneCRCs[i] == dummyroot) continue;

            CraBone bone = new CraBone();
            bone.BoneHash = (int)boneCRCs[i];
            bone.Curve = new CraSourceTransformCurve();

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
        srcClip.SetBones(bones.ToArray());
        srcClip.Bake(120f);
        clip = CraClip.CreateNew(srcClip);
        ClipDB.Add(animID, clip);
        return clip;
    }
}
