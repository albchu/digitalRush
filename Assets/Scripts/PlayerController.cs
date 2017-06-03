using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;
    public float movementSpeed = 1f;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))    // Forward
        {
            transform.position += transform.forward * Time.deltaTime * movementSpeed;
        }
        else if (Input.GetKey(KeyCode.S))   //Back
        {
            transform.position -= transform.forward * Time.deltaTime * movementSpeed;
        }
        else if (Input.GetKey(KeyCode.D))   // Right
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            transform.position -= right_angle * Time.deltaTime * movementSpeed;
        }
        else if (Input.GetKey(KeyCode.A))   // Left
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            transform.position += right_angle * Time.deltaTime * movementSpeed;
        }


        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotX += mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

}
