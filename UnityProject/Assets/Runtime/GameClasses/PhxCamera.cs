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
        FollowTrackable,
    }
                     
    public CamMode    Mode { get; private set; } = CamMode.Free;
    public float      FreeMoveSpeed              = 100.0f;
    public float      FreeRotationSpeed          = 5.0f;
    public Vector3    PositionOffset             = new Vector3(0f, 2f, -2f);
    public float      FollowSpeed                = 10.0f;
    public float      MouseSensitivity           = 5f;


    IPhxControlableInstance FollowInstance;

    IPhxTrackable TrackableInstance;



    public void FollowTrackable(IPhxTrackable follow)
    {
        Mode = CamMode.FollowTrackable;
        TrackableInstance = follow;
    }


    public void Follow(IPhxControlableInstance follow)
    {
        Mode = CamMode.Follow;
        FollowInstance = follow;

        FreeMoveSpeed              = 100.0f;
        FreeRotationSpeed          = 5.0f;
        PositionOffset             = new Vector3(0f, 2f, -2f);
        FollowSpeed                = 10.0f;
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

    void SanitizeEuler(ref Vector3 euler)
    {
        while (euler.x > 180f) euler.x -= 360f;
        while (euler.y > 180f) euler.y -= 360f;
        while (euler.z > 180f) euler.z -= 360f;
        while (euler.x < -180f) euler.x += 360f;
        while (euler.y < -180f) euler.y += 360f;
        while (euler.z < -180f) euler.z += 360f;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 1000f, Color.red);
    }

    void FixedUpdate()
    {
        if (Mode == CamMode.Free)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical") * Time.fixedDeltaTime * FreeMoveSpeed;
            transform.position += transform.right * Input.GetAxis("Horizontal") * Time.fixedDeltaTime * FreeMoveSpeed;
            transform.position += transform.up * Input.GetAxis("UpDown") * Time.fixedDeltaTime * FreeMoveSpeed;

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


            Vector3 viewDir = (FollowInstance.GetTargetPosition() - rotPoint).normalized;
            //Vector3 viewDir = PhxGameRuntime.GetMatch().Player.ViewDirection;
            Vector3 camTargetPos = rotPoint + viewDir * PositionOffset.z;
            Quaternion camTargetRot = Quaternion.LookRotation(viewDir);
            camTargetPos += camTargetRot * new Vector3(PositionOffset.x, 0f, 0f);

            transform.position = Vector3.Lerp(transform.position, camTargetPos, Time.fixedDeltaTime * FollowSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, camTargetRot, Time.fixedDeltaTime * FollowSpeed);
        }
        else if (Mode == CamMode.FollowTrackable)
        {
            transform.rotation = TrackableInstance.GetCameraRotation();
            transform.position = TrackableInstance.GetCameraPosition();
        }
    }
}
