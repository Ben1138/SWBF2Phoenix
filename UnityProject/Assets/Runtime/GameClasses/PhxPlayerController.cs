using System;
using System.Collections.Generic;
using UnityEngine;

public class PhxPlayerController : PhxPawnController
{
    public bool CancelPressed { get; private set; }



    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // nothing to do
        if (Pawn == null)
        {
            return;
        }

        MoveDirection.x = Input.GetAxis("Horizontal");
        MoveDirection.y = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

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

        CancelPressed = Input.GetButtonDown("Cancel");
    }
}
