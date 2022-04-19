using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxPlayerController : PhxPawnController
{
    PhxPlayerInput PlayerInput => PhxGame.GetPlayerInput();
    PhxCamera Camera => PhxGame.GetCamera();


    public PhxPlayerController()
    {
        Team = 1;
    }

    public override Vector3 GetAimPosition()
    {
        //return TargetPos.HasValue ? TargetPos.Value : Camera.transform.position + Data.ViewDelta * 1000f;
        return Vector3.zero;
    }

    public override void Tick(float deltaTime)
    {
        //base.Tick(deltaTime);

        // UI Controls, are always checked

        // Nothing to control if either there's no pawn
        // to control or we're currently in some menu
        if (Pawn == null || Cursor.lockState != CursorLockMode.Locked)
        {
            Data.Events = new PhxButtonEvents();
            Data.Move = Vector2.zero;
            Data.ViewDelta = Vector2.zero;
            return;
        }

        Data.Events = PlayerInput.GetButtonEvents();

        PhxInputAxesGroup axes = PlayerInput.GetSoldierAxesDelta();
        Data.Move = axes.Thrust.GetValues();
        Data.ViewDelta = axes.View.GetValues();

        //Vector2 rotConstraints = Pawn.GetViewConstraint();
        //Vector2 maxTurnSpeed = Pawn.GetMaxTurnSpeed();

        //// max turn degrees per frame
        //Vector2 maxTurn = maxTurnSpeed * deltaTime;
        //float turnX = Mathf.Clamp(axes.View.Y.Value * 2f, -maxTurn.x, maxTurn.x);
        //float turnY = Mathf.Clamp(axes.View.X.Value * 2f, -maxTurn.y, maxTurn.y);

        //Quaternion rot = Quaternion.LookRotation(Data.ViewDelta);
        //Vector3 euler = rot.eulerAngles;
        //PhxUtils.SanitizeEuler(ref euler);
        //euler.x = Mathf.Clamp(euler.x + turnX, -rotConstraints.x, rotConstraints.x);
        //euler.y = Mathf.Clamp(euler.y + turnY, -rotConstraints.y, rotConstraints.y);

        //rot = Quaternion.Euler(euler);
        //Data.ViewDelta = rot * Vector3.forward;
        //Debug.DrawRay(Camera.transform.position, Data.ViewDelta * 1000f, Color.blue);

        // ignore vehicle colliders
        int layerMask = 7;
        if (Physics.Raycast(Camera.transform.position, Data.ViewDelta, out RaycastHit hit, 1000f, layerMask))
        {
            PhxInstance GetInstance(Transform t)
            {
                PhxInstance inst = t.gameObject.GetComponent<PhxInstance>();
                if (inst == null && t.parent != null)
                {
                    return GetInstance(t.parent);
                }
                return inst;
            }

            Target = GetInstance(hit.collider.gameObject.transform);
            //TargetPos = hit.point;
        }
        else
        {
            ////TargetPos = null;
        }
    }
}
