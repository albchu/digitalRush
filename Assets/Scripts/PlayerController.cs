using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 1f;
    public float cameraDeadzoneY = 5f;
    public float cameraDeadzoneX = 5f;


    void Start()
    {
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
        else if (Input.GetKey(KeyCode.Space))   // Left
        {
            transform.position += Vector3.up * Time.deltaTime * movementSpeed;
        }


    }

}
