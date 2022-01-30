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


/*
Class for posing hierarchies.  Assumes one is passing a 9Pose animation,
but if IsStatic is passed then only a single pose is maintained.  In both
cases, the frames of the animation will be extracted as separate poses.

Will implement jobs/burst functionality soon.  
*/

public class PhxPoser
{
    private Transform[] Bones;
    private Quaternion[] Rotations;
    private Vector3[] Positions;

    int NumFrames;


    // Takes bank + anim + bone and extracts numFrames different states as rotations/positions.
    // numFrames can be greater than the actual number of frames in the anim.
    private static (Quaternion[], Vector3[])? GetFullCurve(AnimationBank bank, uint animCRC, uint boneCRC, int numFrames)
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



    public PhxPoser(string animBankName, string animName, Transform objRoot, bool IsStatic = false)
    {
        AnimationBank NinePose = AnimationLoader.Instance.GetRawAnimationBank(animBankName);
        NumFrames = IsStatic ? 1 : 9;

        if (NinePose != null && NinePose.GetAnimationMetadata(HashUtils.GetCRC(animName), out int actualNumFrames, out int numBones))
        {
            if (!IsStatic && actualNumFrames != 9)
            {
                // For sake of knowledge
                Debug.LogWarningFormat("Found NinePose ({0}, {1}), but has {2} frames...", animBankName, animName, actualNumFrames);
            }

            List<Transform> children = UnityUtils.GetChildTransforms(objRoot);
            children.Add(objRoot);

            Rotations = new Quaternion[NumFrames * numBones];
            Bones = new Transform[numBones];
            Positions = new Vector3[NumFrames * numBones];

            int offset = 0, AcutalNumBones = 0;
            foreach (Transform childTx in children)
            {
                // No root motion
                if (childTx.parent == objRoot) continue;

                (Quaternion[], Vector3[])? Curve = GetFullCurve(NinePose, HashUtils.GetCRC(animName), HashUtils.GetCRC(childTx.name), NumFrames);

                if (!Curve.HasValue) continue;

                Bones[offset / NumFrames] = childTx;

                for (int i = 0; i < NumFrames; i++)
                {
                    Rotations[offset + i] = Curve.Value.Item1[i];
                    Positions[offset + i] = Curve.Value.Item2[i];
                }

                offset += NumFrames;
                AcutalNumBones++;
            }

            //Array.Resize(ref Rotations, AcutalNumBones * NumFrames);
            //Array.Resize(ref Bones, AcutalNumBones);
            //Array.Resize(ref Positions, AcutalNumBones * NumFrames);
        }
        else
        {
            Debug.LogErrorFormat("Pose not found... ({0}, {1})", animBankName, animName);
        }
    }



    public void SetState()
    {
        SetStateByFrame(0, 1f);
    }

    public void SetState(PhxFivePoseState state, float blendValue)
    {
        SetStateByFrame(NumFrames == 9 ? (int) state : 0, blendValue);
    }

    public void SetState(PhxNinePoseState state, float blendValue)
    {
        SetStateByFrame(NumFrames == 9 ? (int) state : 0, blendValue);
    }

    // Probably slow, will use jobs soon.
    private void SetStateByFrame(int frameNum, float blendValue)
    {
        if (Rotations == null || Bones == null || Positions == null)
        {
            Debug.LogErrorFormat("No pose data initialized.");
            return;
        }

        int Index;
        for (int i = 0; i < Bones.Length; i++)
        {
            if (Bones[i] == null) continue;

            Index = i * NumFrames + frameNum;

            Bones[i].localPosition = Vector3.Lerp(Bones[i].localPosition, Positions[Index], blendValue);
            Bones[i].localRotation = Quaternion.Slerp(Bones[i].localRotation, Rotations[Index], blendValue);
        }
    }
}



