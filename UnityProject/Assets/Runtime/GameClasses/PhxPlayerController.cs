using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxPlayerController : PhxPawnController
{
    PhxMatch Match => PhxGame.GetMatch();
    PhxCamera Camera => PhxGame.GetCamera();

    public bool CancelPressed { get; private set; }
    Vector3? TargetPos;

    float VehicleEnterTimer = 1.0f;


    public PhxPlayerController()
    {
        Team = 1;
    }

    public override Vector3 GetAimPosition()
    {
        return TargetPos.HasValue ? TargetPos.Value : Camera.transform.position + ViewDirection * 1000f;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        // UI Controls, are always checked
        CancelPressed = Input.GetButtonDown("Cancel");

        // Nothing to control if either there's no pawn
        // to control or we're currently in some menu
        if (Pawn == null || Cursor.lockState != CursorLockMode.Locked)
        {
            MoveDirection = Vector2.zero;
            Jump = false;
            Sprint = false;
            Reload = false;
            NextPrimaryWeapon = false;
            NextSecondaryWeapon = false;
            ShootPrimary = false;
            ShootSecondary = false;
            Crouch = false;
            return;
        }

        MoveDirection.x = Input.GetAxis("Horizontal");
        MoveDirection.y = Input.GetAxis("Vertical");

        mouseX =  Input.GetAxis("Mouse X");
        mouseY = -Input.GetAxis("Mouse Y");

        Vector2 rotConstraints = Pawn.GetViewConstraint();
        Vector2 maxTurnSpeed = Pawn.GetMaxTurnSpeed();

        // max turn degrees per frame
        Vector2 maxTurn = maxTurnSpeed * deltaTime;
        float turnX = Mathf.Clamp(mouseY * 2f, -maxTurn.x, maxTurn.x);
        float turnY = Mathf.Clamp(mouseX * 2f, -maxTurn.y, maxTurn.y);

        Quaternion rot = Quaternion.LookRotation(ViewDirection);
        Vector3 euler = rot.eulerAngles;
        PhxUtils.SanitizeEuler(ref euler);
        euler.x = Mathf.Clamp(euler.x + turnX, -rotConstraints.x, rotConstraints.x);
        euler.y = Mathf.Clamp(euler.y + turnY, -rotConstraints.y, rotConstraints.y);

        rot = Quaternion.Euler(euler);
        ViewDirection = rot * Vector3.forward;

        Jump = Input.GetButtonDown("Jump");
        Sprint = Input.GetButton("Sprint");
        Reload = Input.GetButtonDown("Reload");
        NextPrimaryWeapon = Input.GetAxis("WeaponChange") < 0;
        NextSecondaryWeapon = Input.GetAxis("WeaponChange") > 0;
        ShootPrimary = Input.GetButton("Fire1");
        ShootSecondary = Input.GetButton("Fire2");

        if (Input.GetButtonDown("Crouch"))
        {
            Crouch = !Crouch;
        }

        Debug.DrawRay(Camera.transform.position, ViewDirection * 1000f, Color.blue);

        // ignore vehicle colliders
        int layerMask = 7;
        if (Physics.Raycast(Camera.transform.position, ViewDirection, out RaycastHit hit, 1000f, layerMask))
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
        
        SwitchSeat = Input.GetKeyDown(KeyCode.G);

        if (Input.GetKeyDown(KeyCode.E))
        {
            Enter = true;
        }

        if (Enter)
        {
            if (VehicleEnterTimer <= 0.0f)
            {
                VehicleEnterTimer = 1.0f;
            }
            else 
            {
                Enter = false;
            }
        }

        VehicleEnterTimer = Mathf.Clamp(VehicleEnterTimer - Time.deltaTime, -0.01f, 1.0f);
    }
}
