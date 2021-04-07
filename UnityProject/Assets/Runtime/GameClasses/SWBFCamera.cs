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
        Follow,
        Control
    }
                     
    public CamMode    Mode               = CamMode.Free;
    public float      FreeMoveSpeed      = 100.0f;
    public float      FreeRotationSpeed  = 5.0f;
    public Vector3    PositionOffset     = new Vector3(0f, 2f, -2f);
    public float      FollowSpeed        = 10.0f;
    public float      MouseSensitivity   = 5f;

    const float RotVertMin = -45f;
    const float RotVertMax = 45f;

    Rigidbody     Body;
    ISWBFInstance FollowInstance;
    Vector3       CamTargetPos;
    Quaternion    CamTargetRot;


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
        CamTargetPos = transform.position;
        CamTargetRot = transform.rotation;
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

    }

    void FixedUpdate()
    {
        if (Mode == CamMode.Follow)
        {
            Vector3 rotPoint = FollowInstance.transform.position;
            rotPoint.y += PositionOffset.y;

            Vector3 viewDir = (Player.LookingAt - rotPoint).normalized;
            CamTargetPos = rotPoint + viewDir * PositionOffset.z;
            CamTargetRot = Quaternion.LookRotation(viewDir);
            CamTargetPos += CamTargetRot * new Vector3(PositionOffset.x, 0f, 0f);
        }
        else if (Mode == CamMode.Control)
        {
            Vector3 rotPoint = Player.Pawn.transform.position;
            rotPoint.y += PositionOffset.y;

            Vector3 viewRight = CamTargetRot * Vector3.right;
            //Quaternion targetRot = CamTargetRot * Quaternion.AngleAxis(Player.MouseDiff.y * -10f, viewRight) * Quaternion.AngleAxis(Player.MouseDiff.x * 10f, Vector3.up);

            Vector3 viewDir = CamTargetRot * Vector3.forward;
            viewDir = Quaternion.AngleAxis(-Player.MouseDiff.y * MouseSensitivity, viewRight) * viewDir;
            viewDir = Quaternion.AngleAxis(Player.MouseDiff.x * MouseSensitivity, Vector3.up) * viewDir;

            // TODO: this is a dumb way of rotating on the local X axis
            Quaternion targetRot = Quaternion.LookRotation(viewDir);

            // clamp vertical rotation
            Vector3 targetRotEuler = targetRot.eulerAngles;
            SanitizeEuler(ref targetRotEuler);
            targetRotEuler.x = Mathf.Clamp(targetRotEuler.x, RotVertMin, RotVertMax);
            targetRotEuler.z = Mathf.Clamp(targetRotEuler.z, RotVertMin, RotVertMax);
            targetRot = Quaternion.Euler(targetRotEuler);

            viewDir = targetRot * Vector3.forward;
            CamTargetPos = rotPoint + viewDir * PositionOffset.z;
            CamTargetRot = Quaternion.LookRotation(viewDir);
            CamTargetPos += CamTargetRot * new Vector3(PositionOffset.x, 0f, 0f);
        }

        if (Mode == CamMode.Free)
        {
            transform.position += transform.forward * Input.GetAxis("Vertical")   * Time.fixedDeltaTime * FreeMoveSpeed;
            transform.position += transform.right   * Input.GetAxis("Horizontal") * Time.fixedDeltaTime * FreeMoveSpeed;
            transform.position += transform.up      * Input.GetAxis("UpDown")     * Time.fixedDeltaTime * FreeMoveSpeed;

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeRotationSpeed;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeRotationSpeed;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }
        else if (Mode == CamMode.Follow || Mode == CamMode.Control)
        {
            Body.MovePosition(Vector3.Lerp(Body.position, CamTargetPos, Time.fixedDeltaTime * FollowSpeed));
            Body.MoveRotation(Quaternion.Slerp(Body.rotation, CamTargetRot, Time.fixedDeltaTime * FollowSpeed));
        }
    }
}
