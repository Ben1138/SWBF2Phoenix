using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

using LibSWBF2.Wrappers;
using LibSWBF2.Utils;

// For turrets/pilots
public enum PhxFivePoseState : int
{   
    TurnUp = 1,
    TurnRight = 3,
    Idle = 4,
    TurnLeft = 5,
    TurnDown = 7,
}

// For vehicles/pilots
public enum PhxNinePoseState : int
{
    ForwardTurnLeft,
    Forward,
    ForwardTurnRight,
    StrafeLeft,
    Idle,
    StrafeRight,
    BackwardsTurnLeft,
    Backwards,
    BackwardsTurnRight,
}


public class PhxPoser
{
    private Transform[] Bones;
    private Quaternion[] Rotations;
    private Vector3[] Positions;

    uint IgnoreMask = 0;


    private static (Quaternion[], Vector3[])? GetFullCurve(AnimationBank bank, uint animCRC, uint boneCRC, int numFrames = 9)
    {
        Quaternion[] rots = new Quaternion[numFrames];
        Vector3[] locs = new Vector3[numFrames];
        uint ComponentIndex = 0;
        float[] values;
        ushort[] inds;

        do {
            if (!bank.GetCurve(animCRC, boneCRC, ComponentIndex, out inds, out values))
            {
                return null;
            }

            float mult = (ComponentIndex == 0 || ComponentIndex == 3 || ComponentIndex == 4) ? -1f : 1f;
            float curValue;
            int frameIndex = 0;
            int valueIndex = 0;

            for (int i = 0; i < numFrames; i++)
            {
                if (i > inds[frameIndex] && frameIndex < inds.Length - 1)
                {
                    if (i == inds[frameIndex + 1])
                    {
                        frameIndex++;
                    }
                }

                curValue = values[frameIndex];

                if (ComponentIndex < 4)
                {
                    rots[i][(int) ComponentIndex] = mult * curValue;
                }
                else 
                {
                    locs[i][(int) ComponentIndex - 4] = mult * curValue;
                }

            }            
        } while (++ComponentIndex < 7);

        return (rots, locs);
    }

    /*
    private static Vector3[] GetCurvePositions(AnimationBank bank, uint animCRC, uint boneCRC, int numFrames = 9)
    {
        Vector3[] result = new Vector3[numFrames];
        uint ComponentIndex = 4;
        float[] values;
        ushort[] inds;

        do {
            if (!bank.GetCurve(animCRC, boneCRC, ComponentIndex, out inds, out values))
            {
                return null;
            }

            float mult = ComponentIndex == 4 ? -1f : 1f;
            float curValue;
            int frameIndex = 0;
            int valueIndex = 0;

            for (int i = 0; i < numFrames; i++)
            {
                if (i > inds[frameIndex] && frameIndex < inds.Length - 1)
                {
                    if (i == inds[frameIndex + 1])
                    {
                        frameIndex++;
                    }
                }

                curValue = values[frameIndex];

                result[i][(int) ComponentIndex - 4] = mult * curValue;
            }            
        } while (++ComponentIndex < 7);

        return result;
    }
    */


    public PhxPoser(string animBankName, string animName, Transform objRoot, bool IgnoreRoot = true)
    {
        AnimationBank NinePose = AnimationLoader.Instance.GetRawAnimationBank(animBankName);

        if (NinePose != null && NinePose.GetAnimationMetadata(HashUtils.GetCRC(animName), out int numFrames, out int numBones))
        {
            if (numFrames != 9)
            {
                Debug.LogErrorFormat("Found NinePose ({0}, {1}), but has {2} frames...", animBankName, animName, numFrames);
            }

            List<Transform> children = UnityUtils.GetChildTransforms(objRoot);
            children.Add(objRoot);

            Rotations = new Quaternion[9 * numBones];
            Bones = new Transform[numBones];
            Positions = new Vector3[9 * numBones];

            int offset = 0;
            foreach (Transform childTx in children)
            {
                if (IgnoreRoot && childTx.parent == objRoot) continue;

                (Quaternion[], Vector3[])? Curve = GetFullCurve(NinePose, HashUtils.GetCRC(animName), HashUtils.GetCRC(childTx.name));

                if (!Curve.HasValue) continue;

                Bones[offset / 9] = childTx;

                for (int i = 0; i < 9; i++)
                {
                    Rotations[offset + i] = Curve.Value.Item1[i];
                    Positions[offset + i] = Curve.Value.Item2[i];
                }

                offset += 9;
            }
        }
        else
        {
            Debug.LogErrorFormat("NinePose not found... ({0}, {1})", animBankName, animName);
        }
    }


    public void IgnoreBone(string name)
    {

    }


    public void SetState(PhxFivePoseState state, float blendValue)
    {
        SetStateByFrame((int) state, blendValue);
    }

    public void SetState(PhxNinePoseState state, float blendValue)
    {
        SetStateByFrame((int) state, blendValue);
    }

    private void SetStateByFrame(int frameNum, float blendValue)
    {
        if (Rotations == null)
        {
            Debug.LogErrorFormat("No info set!!!!");
            return;
        }

        for (int i = 0; i < Bones.Length; i++)
        {
            if (Bones[i] == null) continue;

            Bones[i].localPosition = Vector3.Lerp(Bones[i].localPosition, Positions[i * 9 + frameNum], blendValue);
            Bones[i].localRotation = Quaternion.Slerp(Bones[i].localRotation, Rotations[i * 9 + frameNum], blendValue);
        }
    }



    public static void PoseSkeletonStatically(string animBankName, string animName, Transform objRoot)
    {
        AnimationBank Pose = AnimationLoader.Instance.GetRawAnimationBank(animBankName);

        if (Pose != null && Pose.GetAnimationMetadata(HashUtils.GetCRC(animName), out int numFrames, out int numBones))
        {
            if (numFrames != 1)
            {
                Debug.LogErrorFormat("Found static pose ({0}, {1}), but has {2} frames...", animBankName, animName, numFrames);
            }

            List<Transform> children = UnityUtils.GetChildTransforms(objRoot);

            foreach (Transform childTx in children)
            {
                (Quaternion[], Vector3[])? Curve = GetFullCurve(Pose, HashUtils.GetCRC(animName), HashUtils.GetCRC(childTx.name), 1);

                if (!Curve.HasValue) continue;

                childTx.localRotation = Curve.Value.Item1[0];
                childTx.localPosition = Curve.Value.Item2[0];
            }
        }
        else
        {
            Debug.LogErrorFormat("Static pose not found... ({0}, {1})", animBankName, animName);
        }

    }


}



