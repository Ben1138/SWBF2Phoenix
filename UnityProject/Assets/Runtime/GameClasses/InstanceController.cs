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

        public override bool Equals(object obj)
        {
            State other = (State)obj;
            return
                ShootPrimary == other.ShootPrimary &&
                Crouch == other.Crouch &&
                WalkDirection == other.WalkDirection &&
                ViewDirection == other.ViewDirection;
        }
    }

    public State ControlState { get; protected set; }

    public float IdleTime { get; private set; }

    State OldState;


    public void ResetIdleTime()
    {
        IdleTime = 0f;
    }

    public virtual void Update(float deltaTime)
    {
        if (ControlState.Equals(OldState))
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
