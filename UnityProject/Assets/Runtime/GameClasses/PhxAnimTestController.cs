using System.Collections.Generic;
using UnityEngine;

public class PhxAnimTestController : PhxAIController
{
    public Transform TestAim;

    float ReloadTimer;
    float ForwardOffset;


    public PhxAnimTestController()
    {
        ForwardOffset = Random.Range(0f, 3.1416f * 2f);
    }

    public override Vector3 GetAimPosition()
    {
        return TestAim != null ? TestAim.position : ViewDirection * 1000f;
    }

    public override void Tick(float deltaTime)
    {
        //base.Tick(deltaTime);

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

        float sin = Mathf.Sin(ForwardOffset + Time.timeSinceLevelLoad);

        MoveDirection.x = sin;
        MoveDirection.y = sin;

        ShootPrimary = sin > -0.5f;

        if (TestAim != null)
        {
            ViewDirection = (TestAim.transform.position - Pawn.GetInstance().transform.position).normalized;
        }
    }
}
