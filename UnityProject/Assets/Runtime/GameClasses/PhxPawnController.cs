using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhxPawnController
{
    public IPhxControlableInstance Pawn { get; private set; }

    public bool ShootPrimary;
    public bool ShootSecondary;
    public bool Crouch;
    public bool Jump;
    public bool Sprint;
    public bool Reload;
    public bool NextPrimaryWeapon;
    public bool NextSecondaryWeapon;
    public Vector2 MoveDirection;
    public Vector3 ViewDirection;

    public PhxCommandpost CapturePost;
    public int Team = 0;


    public bool IsIdle => !ShootPrimary && !Crouch && MoveDirection == Vector2.zero;
    public float IdleTime { get; private set; }


    // For assignment, use IPhxControlableInstance.Assign()!
    public void SetPawn(IPhxControlableInstance pawn)
    {
        Debug.Assert(pawn.GetController() == this);

        Pawn = pawn;
        ViewDirection = Pawn.GetInstance().transform.forward;
    }

    // For un-assignment, use IPhxControlableInstance.UnAssign()!
    public void RemovePawn()
    {
        Pawn = null;
        CapturePost = null;
    }

    public void ResetIdleTime()
    {
        IdleTime = 0f;
    }

    public virtual void Update(float deltaTime)
    {
        if (IsIdle)
        {
            IdleTime += deltaTime;
        }
        else
        {
            IdleTime = 0f;
        }
    }

    protected static void SanitizeEuler(ref Vector3 euler)
    {
        while (euler.x > 180f) euler.x -= 360f;
        while (euler.y > 180f) euler.y -= 360f;
        while (euler.z > 180f) euler.z -= 360f;
        while (euler.x < -180f) euler.x += 360f;
        while (euler.y < -180f) euler.y += 360f;
        while (euler.z < -180f) euler.z += 360f;
    }
}
