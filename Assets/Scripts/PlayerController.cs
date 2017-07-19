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
    public float maxRotateDegreesYaw = 30f;      // This is the boundary value for how far the ship can rotate when it goes to the left and right
    public float maxRotateDegreesPitch = 15f;      // This is the boundary value for how far the ship can rotate when it goes to the left and right
    public float maxTurnDegrees = 25f;      // This is the boundary value for how far the ship can turn its nose when it goes to the left and right. Value is maximum degrees away from middle
    public float maxTurnIncrements = 1f;      // This is the maximum amount of increment steps to turn the ships nose from left or right 

    public float rotationIncrements = 50f;
    public float rotationRestoreIncrements = 2f;     // Time until ship returns to original position after a rotation

    public Transform playerCamera;

    private float nextDashTime = 0f;
    private float pressedDashTime = 0f;
    private bool restoringRotation;       // This value is for whether the restoration process (which can be disrupted) should occur
    private Quaternion originalRotation;

    void Start()
    {
        restoringRotation = false;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        movement();
    }

    void movement()
    {
        float dashModifier = dash();

        Vector3 movementDuringFrame = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.W))    // Forward
        {
            movementDuringFrame = Vector3.up * Time.deltaTime * movementSpeed * dashModifier;
            restoringRotation = false;
            bankDown();
        }
        if (Input.GetKey(KeyCode.S))   //Back
        {
            movementDuringFrame -= Vector3.up * Time.deltaTime * movementSpeed * dashModifier;
            restoringRotation = false;
            bankUp();
        }
        if (Input.GetKey(KeyCode.D))   // Right
        {
            Vector3 right_angle = Vector3.Cross(Vector3.forward, Vector3.up);
            movementDuringFrame -= right_angle * Time.deltaTime * movementSpeed * dashModifier;
            restoringRotation = false;
            bankRight();
            turnRight();
        }
        if (Input.GetKey(KeyCode.A))   // Left
        {
            Vector3 right_angle = Vector3.Cross(Vector3.forward, Vector3.up);
            movementDuringFrame += right_angle * Time.deltaTime * movementSpeed * dashModifier;
            restoringRotation = false;
            bankLeft();
            turnLeft();
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
            Quaternion differenceFromOriginal = Quaternion.Inverse(originalRotation) * transform.rotation;
            bool closeEnoughToOriginal = differenceFromOriginal == originalRotation || differenceFromOriginal.Equals(originalRotation);
            if (!closeEnoughToOriginal)
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationRestoreIncrements * Time.deltaTime);
        }
    }

    // The logic for the dash movement. returns the modifier value to apply dash;
    float dash()
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
        return dashModifier;
   }

    // Rotation Logics
    void turnRight()
    {
        float sign = (transform.forward.x < 0) ? -1.0f : 1.0f;
        float rotateForwardAngle = Vector3.Angle(Vector3.forward, transform.forward) * sign;
        float angleNormPercent = 1 - (rotateForwardAngle + maxTurnDegrees) / (maxTurnDegrees * 2);
        float rotateInc = maxTurnIncrements * angleNormPercent;     // Rotation done with this percentage style to have animation fluidity when holding both left and right down at same time
        conditionallyRotate(angleNormPercent > 0, Vector3.up, rotateInc);
    }

    void turnLeft()
    {
        float sign = (transform.forward.x < 0) ? -1.0f : 1.0f;
        float rotateForwardAngle = Vector3.Angle(Vector3.forward, transform.forward) * sign;
        float angleNormPercent = (rotateForwardAngle + maxTurnDegrees) / (maxTurnDegrees * 2);
        float rotateInc = maxTurnIncrements * angleNormPercent;     // Rotation done with this percentage style to have animation fluidity when holding both left and right down at same time
        conditionallyRotate(angleNormPercent > 0, Vector3.down, rotateInc);
    }

    void bankDown()
    {
        float rotateBudget = (transform.eulerAngles.x + maxRotateDegreesPitch) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
        bool canStillRotate = Mathf.Clamp(rotateBudget, 0, 10) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
        conditionallyRotate(canStillRotate, Vector3.left);
    }

    void bankUp()
    {
        float rotateBudget = (transform.eulerAngles.x + maxRotateDegreesPitch) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
        bool canStillRotate = Mathf.Clamp(rotateBudget, maxRotateDegreesPitch * 2 - 10, maxRotateDegreesPitch * 2) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
        conditionallyRotate(canStillRotate, Vector3.right);
    }

    void bankRight()
    {
        float rotateBudget = (transform.eulerAngles.z + maxRotateDegreesYaw) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
        bool canStillRotate = Mathf.Clamp(rotateBudget, 0, 10) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
        conditionallyRotate(canStillRotate, Vector3.back);
    }

    void bankLeft()
    {
        float rotateBudget = (transform.eulerAngles.z + maxRotateDegreesYaw) % 360f;   // Normalizes rotation and skews scale so that resting position is at MaxRotateDegrees
        bool canStillRotate = Mathf.Clamp(rotateBudget, maxRotateDegreesYaw * 2 - 10, maxRotateDegreesYaw * 2) != rotateBudget;     // Value can be missed if we wait for an exact value. Limiting rotation when the value hits a range close to the limit
        conditionallyRotate(canStillRotate, Vector3.forward);
    }

    /**
     * Will only rotate by the vector and increment if the first boolean value is true
     */
    void conditionallyRotate(bool canStillRotate, Vector3 rotVec, float rotIncs)
    {
        if (canStillRotate)     // Rotation clamping
            transform.RotateAround(transform.position, rotVec, rotIncs);
    }

    void conditionallyRotate(bool canStillRotate, Vector3 rotVec)
    {
        conditionallyRotate(canStillRotate, rotVec, rotationIncrements * Time.deltaTime);
    }
}
