using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;


public class PhxAimer
{
    public Transform Node;
    public Transform FireNode;


    public Vector2 PitchRange;
    public Vector2 YawRange;
    public float BarrelRecoil = .25f;
    public Transform BarrelNode = null;

    private Vector3 RestDir;
    private Vector3 BarrelRestPos;

    private float RecoilTime = 1.0f;
    private float RecoilTimer = 1.1f;

    public PhxAimer ChildAimer = null;

    public int HierarchyLevel = 0;

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
            return ChildAimer.Fire();
        }

        if (BarrelNode == null)
        {
            return true;
        }

        BarrelNode.position = BarrelNode.parent.TransformPoint(BarrelRestPos) - BarrelRecoil * BarrelNode.forward; 
        RecoilTimer = 0.0f;
        
        return true;
    }
}

