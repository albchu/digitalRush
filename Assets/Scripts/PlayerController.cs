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
    public float rotationRestoreSeconds = 0.5f;     // Time until ship returns to original position after a rotation
    public float rotationMax = 50f;      // This is the boundary value for how far the ship can rotate when it goes to the left and right

    private float leftRotationAmount;
    private float rightRotationAmount;
    private float nextDashTime = 0f;
    private float pressedDashTime = 0f;
    private float rotationStartTime;
    private bool restoringRotation;       // This value is for whether the restoration process (which can be disrupted) should occur
    private bool atRestingRotation;       // This value is only for whether the rotation has been completely restored
    private Quaternion originalRotation;
    void Start()
    {
        leftRotationAmount = rotationMax;
        rightRotationAmount = -rotationMax;
        restoringRotation = false;
        atRestingRotation = true;
        originalRotation = transform.rotation;
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
        if (pressedDashTime >= dashPressedMaxTime)      // We dont want to let the player just hold dash and zoom around like a madman.
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

            // Rotation logic
            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            transform.RotateAround(transform.position, Vector3.forward, rightRotationAmount * Time.deltaTime);
            if (rightRotationAmount < 0)
            {
                rightRotationAmount++;
            }
        }
        else if (Input.GetKey(KeyCode.A))   // Left
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            transform.position += right_angle * Time.deltaTime * movementSpeed * dashModifier;

            // Rotation logic
            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            transform.RotateAround(transform.position, Vector3.forward, leftRotationAmount * Time.deltaTime);
            if (leftRotationAmount > 0)
            {
                leftRotationAmount--;
            }
        }

        // Rotations while going left
        if (Input.GetKeyDown(KeyCode.A) && atRestingRotation)
        {
            originalRotation = transform.rotation;  // Save the original rotation
            rotationStartTime = Time.time;
            print("recorded original rotation");
        }
        
        if (Input.GetKeyUp(KeyCode.A))
        {
            //print("Begin restoring rotation");
            restoringRotation = true;   // Start restoring process
            leftRotationAmount = rotationMax;
        }

        // Rotations while going right
        if (Input.GetKeyDown(KeyCode.D) && atRestingRotation)
        {
            originalRotation = transform.rotation;  // Save the original rotation
            rotationStartTime = Time.time;
            //print("recorded original rotation");
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            //print("Begin restoring rotation");
            restoringRotation = true;   // Start restoring process
            rightRotationAmount = -rotationMax;
        }

        if (restoringRotation)
        {
            float timeSinceStarted = Time.time - rotationStartTime;
            float percentageComplete = timeSinceStarted / rotationRestoreSeconds;   // Proper way of using lerp/slerp to ensure that full original rotation is achieved
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, percentageComplete);
            if (percentageComplete >= 1f || originalRotation == transform.rotation)
            {
                //print("Restored original rotation!");
                restoringRotation = false;
                atRestingRotation = true;
            }
        }
    }

}
