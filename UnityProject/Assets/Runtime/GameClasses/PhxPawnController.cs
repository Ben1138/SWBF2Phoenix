using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhxPawnController
{
    public IPhxControlableInstance Pawn { get; private set; }

    public float mouseX;
    public float mouseY;

    public bool SwitchSeat;

    public bool ShootPrimary;
    public bool ShootSecondary;
    public bool Crouch;
    public bool Jump;
    public bool Sprint;
    public bool Roll;
    public bool Reload;
    public bool NextPrimaryWeapon;
    public bool NextSecondaryWeapon;

    public bool Enter;

    public Vector2 MoveDirection;
    public Vector3 ViewDirection;
    protected PhxInstance Target;

    public PhxCommandpost CapturePost;
    public int Team = 0;



    public bool IsIdle => !ShootPrimary && !Crouch && MoveDirection == Vector2.zero;
    public float IdleTime { get; private set; }


    public abstract Vector3 GetAimPosition();

    public PhxInstance GetAimObject()
    {
        return Target;
    }

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

    public virtual void Tick(float deltaTime)
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
}
