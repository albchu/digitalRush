using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float transitionSpeed = 5f;

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {

            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, transitionSpeed/5 * Time.deltaTime);

        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, transitionSpeed * Time.deltaTime);

        }
    }
}