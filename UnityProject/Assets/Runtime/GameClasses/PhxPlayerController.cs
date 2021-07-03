using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxPlayerController : PhxPawnController
{
    PhxRuntimeMatch Match => PhxGameRuntime.GetMatch();
    public bool CancelPressed { get; private set; }


    float VehicleEnterTimer = 1.0f;


    public PhxPlayerController()
    {
        Team = 1;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

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
        SanitizeEuler(ref euler);
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

        SwitchSeat = Input.GetKeyDown(KeyCode.G);

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Pressed E!");
            TryEnterVehicle = true;
        }

        if (TryEnterVehicle)
        {
            if (VehicleEnterTimer <= 0.0f)
            {
                VehicleEnterTimer = 1.0f;
            }
            else 
            {
                TryEnterVehicle = false;
            }
        }

        VehicleEnterTimer = Mathf.Clamp(VehicleEnterTimer - Time.deltaTime, -0.01f, 1.0f);
    }
}
