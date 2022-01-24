using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhxCamera : MonoBehaviour
{
    public enum CamMode
    {
        Fixed,
        Free,
        Follow,
        Track,
    }
                     
    public CamMode    Mode { get; private set; } = CamMode.Free;
    public float      FreeMoveSpeed              = 100.0f;
    public float      FreeRotationSpeed          = 5.0f;
    public Vector3    PositionOffset             = new Vector3(0f, 2f, -2f);
    public float      FollowSpeed                = 100.0f;
    public float      MouseSensitivity           = 5f;


    IPhxControlableInstance FollowInstance;

    IPhxTrackable TrackableInstance;



    public void Track(IPhxTrackable Track)
    {
        Mode = CamMode.Track;
        TrackableInstance = Track;
    }


    public void Follow(IPhxControlableInstance follow)
    {
        Mode = CamMode.Follow;
        FollowInstance = follow;
    }

    public void Fixed()
    {
        Mode = CamMode.Fixed;
        FollowInstance = null;
    }

    public void Fixed(PhxTransform t)
    {
        Fixed();
        transform.position = t.Position;
        transform.rotation = t.Rotation;
    }

    public void Free()
    {
        Mode = CamMode.Free;
        FollowInstance = null;
    }

    public void RotateRigidBodyAroundPointBy(Rigidbody rb, Vector3 origin, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rb.MovePosition(q * (rb.transform.position - origin) + origin);
        rb.MoveRotation(rb.transform.rotation * q);
    }


    void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (Mode == CamMode.Free)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical") * deltaTime * FreeMoveSpeed;
            transform.position += transform.right * Input.GetAxis("Horizontal") * deltaTime * FreeMoveSpeed;
            transform.position += transform.up * Input.GetAxis("UpDown") * deltaTime * FreeMoveSpeed;

            if (Input.GetMouseButton(1))
            {
                float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeRotationSpeed;
                float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeRotationSpeed;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }
        }
        else if (Mode == CamMode.Follow)
        {
            Vector3 rotPoint = FollowInstance.GetInstance().transform.position;
            rotPoint.y += PositionOffset.y;

            //Vector3 viewDir = (FollowInstance.GetTargetPosition() - rotPoint).normalized;
            Vector3 viewDir = PhxGame.GetMatch().Player.ViewDirection;
            Vector3 camTargetPos = rotPoint + viewDir * PositionOffset.z;
            Quaternion camTargetRot = Quaternion.LookRotation(viewDir);
            camTargetPos += camTargetRot * new Vector3(PositionOffset.x, 0f, 0f);

            transform.position = camTargetPos;// Vector3.Lerp(transform.position, camTargetPos, deltaTime * FollowSpeed);
            transform.rotation = camTargetRot;// Quaternion.Slerp(transform.rotation, camTargetRot, deltaTime * FollowSpeed);
        }
        else if (Mode == CamMode.Track)
        {
            transform.rotation = TrackableInstance.GetCameraRotation();
            transform.position = TrackableInstance.GetCameraPosition();
        }
    }
}
