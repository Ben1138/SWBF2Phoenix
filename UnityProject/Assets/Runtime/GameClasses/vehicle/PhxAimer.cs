using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;


/*
Aims barrels at target. 
*/


public class PhxAimer
{
    public Transform Node;
    public Transform FireNode;


    public Vector2 PitchRange;
    public Vector2 YawRange;
    public float BarrelRecoil = .25f;
    public Transform BarrelNode;

    private Vector3 RestDir;
    private Vector3 BarrelRestPos;

    private float RecoilTime = 1.0f;
    private float RecoilTimer = 1.1f;

    public PhxAimer ChildAimer;

    public int HierarchyLevel;

    public void AdjustAim(Vector3 TargetPos)
    {
        Vector3 AimDir = Node.parent.worldToLocalMatrix * (TargetPos - Node.position);

        Vector3 Angles = Quaternion.FromToRotation(RestDir, AimDir).eulerAngles;

        //Angles.x = Mathf.Clamp(Angles.x, PitchRange.x, PitchRange.y);
        //Angles.y = Mathf.Clamp(Angles.y, YawRange.x, YawRange.y);

        Node.localEulerAngles = Angles;

        if (ChildAimer != null)
        {
            ChildAimer.AdjustAim(TargetPos);
        }
    }


    public PhxAimer(){}

    public void Init()
    {
        if (Node != null)
        {
            RestDir = Node.parent.worldToLocalMatrix * Node.forward;
        }

        if (BarrelNode != null)
        {
            BarrelRestPos = BarrelNode.localPosition;
        }

        if (FireNode == null)
        {
            FireNode = Node;
        }
    }


    public void UpdateBarrel()
    {
        //Debug.LogFormat("Firing aimer {0}", Node.name);

        if (BarrelNode == null)
        {
            if (ChildAimer != null)
            {
                ChildAimer.UpdateBarrel();
            }
            return;
        }

        if (RecoilTimer < RecoilTime)
        {
            BarrelNode.position += BarrelNode.forward * BarrelRecoil * (Time.deltaTime / RecoilTime);
            RecoilTimer += Time.deltaTime;            
        }
    }


    public bool Fire()
    {
        if (ChildAimer != null)
        {
            Debug.LogFormat("Firing child of aimer: {0}", Node.name);
            return ChildAimer.Fire();
        }

        Debug.LogFormat("Firing aimer {0}", Node.name);

        if (BarrelNode == null)
        {
            Debug.Log("Barrel null...");
            return true;
        }

        BarrelNode.position = BarrelNode.parent.TransformPoint(BarrelRestPos) - BarrelRecoil * BarrelNode.forward; 
        RecoilTimer = 0.0f;
        
        return true;
    }


    public override string ToString()
    {
        string rep = String.Format("Aimer (Node: {0}", Node.name);

        if (BarrelNode != null)
        {
            rep = rep + String.Format(", Barrel: {0}", BarrelNode.name);
        }

        if (ChildAimer != null)
        {
            rep = rep + String.Format(", Child: {0}", ChildAimer.Node.name);
        }

        return rep + ")";
    }   


}

