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
        return TestAim != null ? TestAim.position : Data.ViewDirection * 1000f;
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
            Data.Events.Pressed |= PhxInput.Soldier_Reload;
        }
        else
        {
            Data.Events.Pressed &= ~PhxInput.Soldier_Reload;
        }

        float sin = Mathf.Sin(ForwardOffset + Time.timeSinceLevelLoad);

        Data.MoveDirection.x = sin;
        Data.MoveDirection.y = sin;

        if (sin > -0.5f)
        {
            Data.Events.Down |= PhxInput.Flyer_FirePrimary;
        }
        else
        {
            Data.Events.Down &= ~PhxInput.Flyer_FirePrimary;
        }

        if (TestAim != null)
        {
            Data.ViewDirection = (TestAim.transform.position - Pawn.GetInstance().transform.position).normalized;
        }
    }
}
