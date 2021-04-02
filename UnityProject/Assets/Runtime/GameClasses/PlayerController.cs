using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PawnController
{
    SWBFCamera Cam => GameRuntime.GetCamera();

    public ISWBFInstance Pawn;
    public bool CancelPressed { get; private set; }


    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        WalkDirection.x = Input.GetAxis("Horizontal");
        WalkDirection.y = Input.GetAxis("Vertical");

        Vector3 camForward = (Quaternion.Inverse(Cam.transform.rotation) * Quaternion.Euler(/*Input.GetAxis("Mouse Y")*10f*/0f, -Input.GetAxis("Mouse X")*10f, 0f)) * Cam.transform.forward;

        if (Physics.Raycast(Cam.transform.position, camForward, out RaycastHit hit, 1000f, 3))
        {
            LookingAt = hit.point;
        }
        else
        {
            LookingAt = camForward * 1000f;
        }

        Debug.Log("LookingAt: " + LookingAt);

        //if (Cam.Mode == SWBFCamera.CamMode.Player)
        //{
        //    Vector3 rotPoint = Pawn.transform.position;
        //    rotPoint.y += Cam.PositionOffset.y;

        //    float rotX = Input.GetAxis("Mouse X");
        //    float rotY = Input.GetAxis("Mouse Y");

        //    //Cam.transform.RotateAround(rotPoint, new Vector3(1f, 0f, 0f), rotY);
        //    Cam.transform.RotateAround(rotPoint, new Vector3(0f, 1f, 0f), rotX);

        //}            

        CancelPressed = Input.GetButtonDown("Cancel");
    }
}
