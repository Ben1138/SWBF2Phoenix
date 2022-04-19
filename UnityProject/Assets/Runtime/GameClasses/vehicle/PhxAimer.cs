using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using LibSWBF2.Utils;
using System.Runtime.ExceptionServices;


/*
Simple class for handling barrel recoil.  Updates/Recoils
are called from PhxWeaponSystem!!  Perhaps this is too 
convoluted, but handling this behavior within PhxAimer
made it too difficult to link to PhxWeaponSystem.
*/

public class PhxBarrel
{
    public Transform Node;
    public Transform FirePoint;    
    public float RecoilDistance = 0f;


    float CurrentRecoil = 0f;

    // Node will never be null given current
    // PhxWeaponSystem InitManual functionality
    public Transform GetFirePoint()
    {
        return FirePoint == null ? Node : FirePoint;
    }

    // Execute recoil
    public void Recoil()
    {
        Node.localPosition -= (RecoilDistance - CurrentRecoil) * Vector3.forward;
        CurrentRecoil = RecoilDistance; 
    }

    // Move barrel back to start
    public void Update()
    {
        if (CurrentRecoil > 0f)
        {
            CurrentRecoil -= Time.deltaTime;
            Node.localPosition += Time.deltaTime * Vector3.forward;
        }
        else
        {
            CurrentRecoil = 0f;
        }
    }
}


/*
Class is a bit messy, but will remain so until
the space of possible aimers is well understood.

Still dont know:
    - Can aimers have multiple children?
    - What does it mean if BarellRecoil is set but BarrelNode isn't?

Still must implement:
    - Pitch/yaw limits
    - Optimization
*/

public class PhxAimer
{
    // Node that aims, also emits ordnance if FireNode isn't set. 
    public Transform Node;

    /*
    If barrels aren't set, ordnance emits from FireNode.
    Of course, use the GetFirePointSequence and GetBarrelSequence 
    to get the information needed to execute firing behavior properly
    in PhxWeaponSystem.
    */
    public Transform FireNode;

    // Rotation limits for Node
    public Vector2 PitchRange = new Vector2(-180f,180f);
    public Vector2 YawRange = new Vector2(-180f, 180f);

    // Barrels exist on a per-aimer basis, but are maintained by the WeaponSystem
    List<PhxBarrel> Barrels;

    // More than likely you can have more than one child aimer, we'll see
    public PhxAimer ChildAimer;

    // Only 1 or 0 seen so far
    public int HierarchyLevel;


    bool IsInitialized;

    Vector3 RestDir;


    public void AddBarrel(PhxBarrel NewBarrel)
    {
        if (Barrels == null)
        {
            Barrels = new List<PhxBarrel>();
        }
        Barrels.Add(NewBarrel);
    }

    /*
    public List<PhxBarrel> GetBarrels()
    {
        if (ChildAimer != null)
        {
            return ChildAimer.GetBarrels();
        }
        else 
        {
            return Barrels;
        }
    }
    */


    /*
    Adjusts aimers to point to target, needs to be finished! 
    */
    public void AdjustAim(Vector3 TargetPos)
    {
        if (!IsInitialized) Init();

        if (Node == null)
        {
            //Debug.LogFormat("Node is null..., has child: {0}", ChildAimer != null);            
            return;
        }

        //Debug.LogFormat("Adjusting aim of node: {0} to TargetPos: {1}", Node.name, TargetPos.ToString());

        Vector3 AimDir = Node.parent.worldToLocalMatrix * (TargetPos - Node.position);

        Vector3 Angles = Quaternion.FromToRotation(RestDir, AimDir).eulerAngles;


        PhxUtils.SanitizeEuler180(ref Angles);

        // These are wrong, needs fixing
        //Angles.x = Mathf.Clamp(Angles.x, PitchRange.x, PitchRange.y);
        //Angles.y = Mathf.Clamp(Angles.y, YawRange.x, YawRange.y);

        // This (with the sanitization) is also wrong, see cis_tread_snailtank, 
        Node.localEulerAngles = Angles;

        if (ChildAimer != null)
        {
            ChildAimer.AdjustAim(TargetPos);
        }
    }


    // Would like to get rid of this, keeping for now
    public void Init()
    {
        IsInitialized = true;

        if (Node != null)
        {
            RestDir = Node.parent.worldToLocalMatrix * Node.forward;
        }
    }



    /*
    Returns the sequence of fire points for an aimer,
    depth-first assuming aimers with children don't have firepoints.
    */

    public List<Transform> GetFirePointSequence()
    {        
        if (ChildAimer == null)
        {
            List<Transform> FirePoints = new List<Transform>();

            if (Barrels != null)
            {
                foreach (PhxBarrel Barrel in Barrels)
                {
                    FirePoints.Add(Barrel.GetFirePoint());
                }
            }
            else 
            {
                FirePoints.Add(FireNode == null ? Node : FireNode);
            }

            return FirePoints;
        }
        else 
        {
            return ChildAimer.GetFirePointSequence();
        }
    }


    /*
    Returns the sequence of barrels aligned with GetFirePointSequence's result.

    RETURNED LIST MAY CONTAIN NULL ENTRIES!  This is because aimers do not need barrels
    to define fire points.
    */

    public List<PhxBarrel> GetBarrelSequence()
    {
        List<PhxBarrel> Result = new List<PhxBarrel>();
        if (ChildAimer == null)
        {
            if (Barrels == null)
            {
                Result.Add(null);
            }
            else 
            {
                Result.AddRange(Barrels);
            }
            return Result;
        }
        else
        {
            return ChildAimer.GetBarrelSequence();
        }
    }



    public override string ToString()
    {
        string rep = String.Format("Aimer (Node: {0}", Node.name);

        if (Barrels != null)
        {
            rep = rep + String.Format(", {0} Barrel(s)", Barrels.Count);
        }

        if (ChildAimer != null)
        {
            rep = rep + String.Format(", Child: {0}", ChildAimer.Node.name);
        }

        return rep + ")";
    }   
}
