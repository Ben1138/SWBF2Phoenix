using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWBFCamera : MonoBehaviour
{
    PlayerController Player => GameRuntime.GetMatch().Player;

    public enum CamMode
    {
        Fixed,
        Free,
        Follow
    }
                     
    public CamMode    Mode               = CamMode.Free;
    public float      FreeMoveSpeed      = 100.0f;
    public float      FreeRotationSpeed  = 5.0f;
    public Vector3    PositionOffset     = new Vector3(0f, 2f, -2f);
    public Vector3    RotationOffset     = Vector3.zero;
    public float      FollowSpeed        = 10.0f;

    Rigidbody Body;
    ISWBFInstance FollowInstance;


    public void Follow(ISWBFInstance follow)
    {
        Mode = CamMode.Follow;
        FollowInstance = follow;
    }

    public void ViewSnapshot(Vector3 pos, Quaternion rot)
    {
        Mode = CamMode.Fixed;
        transform.position = pos;
        transform.rotation = rot;
    }


    void Start()
    {
        Body = GetComponent<Rigidbody>();
    }

    public void RotateRigidBodyAroundPointBy(Rigidbody rb, Vector3 origin, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rb.MovePosition(q * (rb.transform.position - origin) + origin);
        rb.MoveRotation(rb.transform.rotation * q);
    }

    void Update()
    { 
        if (Mode == CamMode.Free)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical")   * Time.deltaTime * FreeMoveSpeed;
            transform.position += transform.right   * Input.GetAxis("Horizontal") * Time.deltaTime * FreeMoveSpeed;
            transform.position += transform.up      * Input.GetAxis("UpDown")     * Time.deltaTime * FreeMoveSpeed;

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeRotationSpeed;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeRotationSpeed;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }
        else if (Mode == CamMode.Follow)
        {
            Vector3 rotPoint = FollowInstance.transform.position;
            rotPoint.y += PositionOffset.y;

            Vector3 viewDir = (Player.LookingAt - rotPoint).normalized;
            Vector3 camPos = rotPoint + viewDir * PositionOffset.z;
            Quaternion camRot = Quaternion.LookRotation(viewDir);

            Debug.Log("Casm pos diff: " + (Body.position - camPos));

            Body.MovePosition(Vector3.Lerp(Body.position, camPos, Time.deltaTime * FollowSpeed));
            Body.MoveRotation(Quaternion.Slerp(Body.rotation, camRot, Time.deltaTime * FollowSpeed));
        }
    }
}
