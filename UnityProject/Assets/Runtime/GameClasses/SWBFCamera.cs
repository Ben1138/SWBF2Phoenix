using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWBFCamera : MonoBehaviour
{
    public enum CamMode
    {
        Free,
        Follow,
        Snapshot
    }
                     
    public CamMode    Mode               = CamMode.Free;
    public float      FreeMoveSpeed      = 100.0f;
    public float      FreeRotationSpeed  = 5.0f;
    public Vector3    PositionOffset     = new Vector3(0f, 2f, -2f);
    public Vector3    RotationOffset     = Vector3.zero;
    public float      FollowSpeed        = 10.0f;

    Rigidbody Body;
    Transform FollowTransform;


    public void Follow(Transform follow)
    {
        Mode = CamMode.Follow;
        FollowTransform = follow;
    }

    public void ViewSnapshot(Vector3 pos, Quaternion rot)
    {
        Mode = CamMode.Snapshot;
        transform.position = pos;
        transform.rotation = rot;
    }


    void Start()
    {
        Body = GetComponent<Rigidbody>();
    }

    void Update()
    { 
        if (Mode == CamMode.Free)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical")   * Time.deltaTime * FreeMoveSpeed;
            transform.position += transform.right   * Input.GetAxis("Horizontal") * Time.deltaTime * FreeMoveSpeed;
            transform.position += transform.up      * Input.GetAxis("UpDown")     * Time.deltaTime * FreeMoveSpeed;

            if (Input.GetMouseButton(1))
            {
                float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeRotationSpeed;
                float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeRotationSpeed;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }
        }
        else if (Mode == CamMode.Follow)
        {
            Vector3 camPos = FollowTransform.position + FollowTransform.rotation * PositionOffset;
            Quaternion camRot = FollowTransform.rotation * Quaternion.Euler(RotationOffset);
            Body.MovePosition(Vector3.Lerp(Body.position, camPos, Time.deltaTime * FollowSpeed));
            Body.MoveRotation(Quaternion.Slerp(Body.rotation, camRot, Time.deltaTime * FollowSpeed));
        }
    }
}
