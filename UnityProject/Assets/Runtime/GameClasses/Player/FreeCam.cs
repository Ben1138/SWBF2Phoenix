using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public float MoveSpeed = 100.0f;
    public float RotationSpeed = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        transform.position += transform.forward * Input.GetAxis("Vertical")   * Time.deltaTime * MoveSpeed;
        transform.position += transform.right   * Input.GetAxis("Horizontal") * Time.deltaTime * MoveSpeed;
        transform.position += transform.up      * Input.GetAxis("UpDown")     * Time.deltaTime * MoveSpeed;

        if (Input.GetMouseButton(1))
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * RotationSpeed;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * RotationSpeed;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }
    }
}
