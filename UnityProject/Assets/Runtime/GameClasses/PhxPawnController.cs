using System;
using System.Collections.Generic;
using UnityEngine;


public struct PhxPawnControlData
{
    public PhxButtonEvents Events;
    public Vector2 Move;
    public Vector2 ViewDelta;           // Degrees this frame
}

public abstract class PhxPawnController
{
    public PhxCommandpost CapturePost;
    public int Team = 0;

    public IPhxControlableInstance Pawn { get; private set; }
    protected PhxPawnControlData Data;
    protected PhxInstance Target;


    public PhxPawnControlData GetControlData()
    {
        return Data;
    }

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
    }

    // For un-assignment, use IPhxControlableInstance.UnAssign()!
    public void RemovePawn()
    {
        Pawn = null;
        CapturePost = null;
    }

    public abstract void Tick(float deltaTime);
}
