using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhxPawnController
{
    public IPhxControlableInstance Pawn;

    public bool ShootPrimary;
    public bool ShootSecondary;
    public bool Crouch;
    public bool Jump;
    public bool Sprint;
    public bool Reload;
    public Vector2 MoveDirection;
    public Vector3 ViewDirection;

    public bool IsIdle => !ShootPrimary && !Crouch && MoveDirection == Vector2.zero;
    public float IdleTime { get; private set; }


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
}
