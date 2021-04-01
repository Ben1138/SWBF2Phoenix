using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstanceController
{
    public struct State
    {
        public bool ShootPrimary;
        public bool Crouch;
        public Vector2 WalkDirection;
        public Vector3 ViewDirection;

        public bool IsActive => ShootPrimary || Crouch || WalkDirection != Vector2.zero || ViewDirection != Vector3.zero;
    }

    public State ControlState { get; protected set; }

    public bool IsIdle => !ControlState.IsActive;
    public float IdleTime { get; private set; }

    State OldState;


    public void ResetIdleTime()
    {
        IdleTime = 0f;
    }

    public virtual void Update(float deltaTime)
    {
        if (!ControlState.IsActive)
        {
            IdleTime += deltaTime;
        }
        else
        {
            IdleTime = 0f;
            OldState = ControlState;
        }
    }
}
