using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]

public class RayGun : MonoBehaviour
{
    public Transform rayRenderOrigin;
    public Transform rayTargetOrigin;

    Vector2 mouse;
    RaycastHit hit;
    float range = 100.0f;
    LineRenderer line;
    public Material lineMaterial;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.SetVertexCount(2);
        line.GetComponent<Renderer>().material = lineMaterial;
        line.SetWidth(0.1f, 0.25f);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 fwd = rayTargetOrigin.transform.TransformDirection(Vector3.forward);
            Debug.DrawRay(rayTargetOrigin.transform.position, fwd * 50, Color.green);
            // Shoot raycast
            if (Physics.Raycast(rayTargetOrigin.position, rayTargetOrigin.forward, out hit, 50))
            {
                //Debug.Log("Raycast hitted to: " + objectHit.collider);
                //targetEnemy = hit.collider.gameObject;
                line.SetPosition(1, hit.point + hit.normal);

            }
            line.enabled = true;
            line.SetPosition(0, rayRenderOrigin.position);
        }
    }
}
