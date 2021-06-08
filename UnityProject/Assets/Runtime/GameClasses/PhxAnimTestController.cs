using System.Collections.Generic;
using UnityEngine;

public class PhxAnimTestController : PhxPawnController
{
    float ReloadTimer;

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
            ReloadTimer = Random.Range(0.5f, 5f);
            Reload = true;
        }
        else
        {
            Reload = false;
        }

        MoveDirection.x = Mathf.Sin(Time.timeSinceLevelLoad);
        MoveDirection.y = Mathf.Sin(Time.timeSinceLevelLoad);
        ViewDirection = Vector3.forward;

        ShootPrimary = true;
    }
}
