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
    public float maxRotateDegreesYaw = 30f;      // This is the boundary value for how far the ship can rotate when it goes to the left and right
    public float maxRotateDegreesPitch = 15f;      // This is the boundary value for how far the ship can rotate when it goes to the left and right
    public float rotationIncrements = 50f;
    public Transform playerCamera; 

    private float nextDashTime = 0f;
    private float pressedDashTime = 0f;
    private float rotationStartTime;
    private bool restoringRotation;       // This value is for whether the restoration process (which can be disrupted) should occur
    private bool atRestingRotation;       // This value is only for whether the rotation has been completely restored
    private Quaternion originalRotation;

    void Start()
    {
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

        Vector3 movementDuringFrame = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.W))    // Forward
        {
            movementDuringFrame = transform.up * Time.deltaTime * movementSpeed * dashModifier;
            
            // Rotation logic
            if (Input.GetKeyDown(KeyCode.W) && atRestingRotation)
            {
                //print("recorded original rotation going right");
                originalRotation = transform.rotation;  // Save the original rotation
                rotationStartTime = Time.time;
            }

            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            float rotateBudget = (transform.eulerAngles.x + maxRotateDegreesPitch) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
            bool canStillRotate = Mathf.Clamp(rotateBudget, 0, 10) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
            if (canStillRotate)     // Rotation clamping
                transform.RotateAround(transform.position, Vector3.left, rotationIncrements * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))   //Back
        {
            movementDuringFrame -= transform.up * Time.deltaTime * movementSpeed * dashModifier;


            // Rotation logic
            if (Input.GetKeyDown(KeyCode.S) && atRestingRotation)
            {
                //print("recorded original rotation going right");
                originalRotation = transform.rotation;  // Save the original rotation
                rotationStartTime = Time.time;
            }

            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            float rotateBudget = (transform.eulerAngles.x + maxRotateDegreesPitch) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
            bool canStillRotate = Mathf.Clamp(rotateBudget, maxRotateDegreesPitch * 2 - 10, maxRotateDegreesPitch * 2) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
            print("Euler" + transform.eulerAngles);
            print("W's rotate budget" + rotateBudget);
            if (canStillRotate)     // Rotation clamping
                transform.RotateAround(transform.position, Vector3.left, -rotationIncrements * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))   // Right
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            movementDuringFrame -= right_angle * Time.deltaTime * movementSpeed * dashModifier;

            // Rotation logic
            if (Input.GetKeyDown(KeyCode.D) && atRestingRotation)
            {
                //print("recorded original rotation going right");
                originalRotation = transform.rotation;  // Save the original rotation
                rotationStartTime = Time.time;
            }

            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            float rotateBudget = (transform.eulerAngles.z + maxRotateDegreesYaw) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
            bool canStillRotate = Mathf.Clamp(rotateBudget, 0, 10) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
            if (canStillRotate)     // Rotation clamping
                transform.RotateAround(transform.position, Vector3.forward, -rotationIncrements * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))   // Left
        {
            Vector3 right_angle = Vector3.Cross(transform.forward, Vector3.up);
            movementDuringFrame += right_angle * Time.deltaTime * movementSpeed * dashModifier;

            // Rotation logic
            if (Input.GetKeyDown(KeyCode.A) && atRestingRotation)
            {
                //print("recorded original rotation going left");
                originalRotation = transform.rotation;  // Save the original rotation
                rotationStartTime = Time.time;
            }
            atRestingRotation = false;
            restoringRotation = false;      // Disable restore so that it does not conflict with this movement
            float rotateBudget = (transform.eulerAngles.z + maxRotateDegreesYaw) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
            bool canStillRotate = Mathf.Clamp(rotateBudget, maxRotateDegreesYaw * 2 - 10, maxRotateDegreesYaw * 2) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
            if (canStillRotate)     // Rotation clamping
                transform.RotateAround(transform.position, Vector3.forward, rotationIncrements * Time.deltaTime);
        }
        playerCamera.position += movementDuringFrame; // Move the camera the same as the player
        transform.position += movementDuringFrame;      // Move the player
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            //print("Begin restoring rotation");
            restoringRotation = true;   // Start restoring processa
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
