using System.Collections.Generic;
using UnityEngine;

public class PhxAnimTestController : PhxPawnController
{
    float ReloadTimer;
    float ForwardOffset;


    public PhxAnimTestController()
    {
        ForwardOffset = Random.Range(0f, 3.1416f * 2f);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // nothing to do
        if (Pawn == null)
        {
            return;
        }

        ReloadTimer -= deltaTime;
        if (ReloadTimer < 0f)
        {
            ReloadTimer = Random.Range(1f, 10f);
            Reload = true;
        }
        else
        {
            Reload = false;
        }

        MoveDirection.x = Mathf.Sin(ForwardOffset + Time.timeSinceLevelLoad);
        MoveDirection.y = Mathf.Sin(ForwardOffset + Time.timeSinceLevelLoad);
        ViewDirection = Vector3.forward;

        //ShootPrimary = true;
    }
}
