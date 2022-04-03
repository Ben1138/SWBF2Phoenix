using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxPlayerController : PhxPawnController
{
    PhxPlayerInput PlayerInput => PhxGame.GetPlayerInput();
    PhxCamera Camera => PhxGame.GetCamera();

    Vector3? TargetPos;


    public PhxPlayerController()
    {
        LockedInputs = PhxInput.None;
        Team = 1;
    }

    public override Vector3 GetAimPosition()
    {
        return TargetPos.HasValue ? TargetPos.Value : Camera.transform.position + Data.ViewDirection * 1000f;
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
            Data.MoveDirection = Vector2.zero;
            Data.ViewDirection = Vector2.zero;
            return;
        }

        Data.Events = PlayerInput.GetButtonEvents();

        PhxInputAxesGroup axes = PlayerInput.GetSoldierAxes();
        Data.MoveDirection = axes.Thrust.Axis;

        Vector2 rotConstraints = Pawn.GetViewConstraint();
        Vector2 maxTurnSpeed = Pawn.GetMaxTurnSpeed();

        // max turn degrees per frame
        Vector2 maxTurn = maxTurnSpeed * deltaTime;
        float turnX;
        float turnY;

        if (axes.View.Type == PhxInputAxisType.Relative)
        {
            turnX = Mathf.Clamp(axes.View.Axis.y * 2f, -maxTurn.x, maxTurn.x);
            turnY = Mathf.Clamp(axes.View.Axis.x * 2f, -maxTurn.y, maxTurn.y);
        }
        else
        {
            Debug.Assert(axes.View.Type == PhxInputAxisType.Absolute);

            turnX = axes.View.Axis.y * maxTurn.x;
            turnY = axes.View.Axis.x * maxTurn.y;
        }

        Quaternion rot = Quaternion.LookRotation(Data.ViewDirection);
        Vector3 euler = rot.eulerAngles;
        PhxUtils.SanitizeEuler(ref euler);
        euler.x = Mathf.Clamp(euler.x + turnX, -rotConstraints.x, rotConstraints.x);
        euler.y = Mathf.Clamp(euler.y + turnY, -rotConstraints.y, rotConstraints.y);

        rot = Quaternion.Euler(euler);
        Data.ViewDirection = rot * Vector3.forward;
        Debug.DrawRay(Camera.transform.position, Data.ViewDirection * 1000f, Color.blue);

        // ignore vehicle colliders
        int layerMask = 7;
        if (Physics.Raycast(Camera.transform.position, Data.ViewDirection, out RaycastHit hit, 1000f, layerMask))
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
            TargetPos = hit.point;
        }
        else
        {
            TargetPos = null;
        }
    }
}
