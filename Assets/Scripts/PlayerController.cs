using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float movementSpeed = 1f;
    public float cameraDeadzoneY = 5f;
    public float cameraDeadzoneX = 5f;
    public float dashSpeed = 7f;
    public float dashRestSeconds = 2f;
    public float dashPressedMaxTime = 0.1f;
    private float nextDashTime = 0f;
    private float pressedDashTime = 0f;
    void Start()
    {
    }

    void Update()
    {
        movement();
    }

    void movement()
    {
        float dashModifier = 1f;        // Init if space not pushed
        if (Input.GetKey(KeyCode.Space) && Time.time > nextDashTime)
        {
            pressedDashTime += Time.deltaTime;
            dashModifier = dashSpeed;

        }
        //print("Time spent pressing dash key is " + pressedDashTime);
        if (pressedDashTime >= dashPressedMaxTime)
        {
            pressedDashTime = 0f;
            nextDashTime = Time.time + dashRestSeconds;
            //print("Current time is: " + Time.time + ". Next dash time at " + nextDashTime);
        }

        if (Input.GetKey(KeyCode.W))    // Forward
        {
            transform.position += transform.up * Time.deltaTime * movementSpeed * dashModifier;
        }
        else if (Input.GetKey(KeyCode.S))   //Back
        {
            transform.position -= transform.up * Time.deltaTime * movementSpeed * dashModifier;
        }
        else if (Input.GetKey(KeyCode.D))   // Right
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            transform.position -= right_angle * Time.deltaTime * movementSpeed * dashModifier;
        }
        else if (Input.GetKey(KeyCode.A))   // Left
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            transform.position += right_angle * Time.deltaTime * movementSpeed * dashModifier;
        }
    }

}
