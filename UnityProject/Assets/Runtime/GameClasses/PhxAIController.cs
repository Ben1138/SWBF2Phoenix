using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhxAIController : PhxPawnController
{
    public override void Tick(float deltaTime)
    {
        //base.Tick(deltaTime);

        // nothing to do
        if (Pawn == null)
        {
            return;
        }

        // TODO: find a target nearby

        if (Target != null)
        {
            Data.ViewDirection = (Target.transform.position - Pawn.GetInstance().transform.position).normalized;
        }
    }
}


public class PhxSoldierAIController : PhxAIController
{
    public override Vector3 GetAimPosition()
    {
        if (Target != null)
        {
            // TODO:
            // - if soldier, aim at head
            // - if vehicle, aim at weak spot
            return Target.transform.position;
        }
        return Data.ViewDirection * 1000f;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        // nothing to do
        if (Pawn == null)
        {
            return;
        }

        // TODO: follow a path to next objective, e.g. capture a CP

        if (Target != null)
        {
            // TODO: when to throw grenade? when to fire? etc.
        }
    }
}
