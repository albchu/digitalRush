using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivityY = 200.0f;
    public float mouseSensitivityX = 100.0f;
    public float clampAngle = 80.0f;
    public Transform rotatePoint;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis
                               // Use this for initialization
    void Start()
    {
        Vector3 rot = transform.parent.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    // Update is called once per frame
    void Update()
    {

        // Camera Controls
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");
        float deltaY = mouseX * mouseSensitivityY ;
        rotY += deltaY;
        rotX += mouseY * mouseSensitivityX * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
        //transform.RotateAround(rotatePoint.position, Vector3.up, deltaY * Time.deltaTime);
    }
}