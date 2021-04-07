using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PawnController
{
    SWBFCamera Cam => GameRuntime.GetCamera();

    public ISWBFInstance Pawn;
    public bool CancelPressed { get; private set; }
    public Vector2 MouseDiff;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        WalkDirection.x = Input.GetAxis("Horizontal");
        WalkDirection.y = Input.GetAxis("Vertical");

        MouseDiff.x = Input.GetAxis("Mouse X");
        MouseDiff.y = Input.GetAxis("Mouse Y");

        Vector3 camForward = Cam.transform.forward;
        //if (Physics.Raycast(Cam.transform.position, camForward, out RaycastHit hit, 1000f, 3))
        //{
        //    LookingAt = hit.point;
        //}
        //else
        //{
            LookingAt = camForward * 1000f;
        //}

        Jump = Input.GetButtonDown("Jump");
        Sprint = Input.GetButton("Sprint");
        Reload = Input.GetButtonDown("Reload");
        ShootPrimary = Input.GetButton("Fire1");
        ShootSecondary = Input.GetButton("Fire2");

        if (Input.GetButtonDown("Crouch"))
        {
            Crouch = !Crouch;
        }

        CancelPressed = Input.GetButtonDown("Cancel");
    }
}
