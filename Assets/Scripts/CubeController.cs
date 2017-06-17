﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour {

    public float movementSpeed = 30f;    // All objects are moving in the z direction towards the camera
    public float spawnDistance = 75f;   // Units that the object will spawn at
    public float lightPadding = 1f;     // The offset that lights will spawn over the cube
    public GameObject cameraObject;
    public float lightOrbitSpeed = 50f;
    public float lightIntensity = 3f;
    public float lightSpawnRadius = 2f;

    // Y jitter
    public float jitterBoundY = 0.5f;   // The max distance that the object can move away from its original Y position
    public float jitterIncrementY = 0.01f;      // The number of incremental units that the object can translate during a jitter
    float originalY;
    GameObject lightA;
    GameObject lightB;
    // Use this for initialization
    void Awake () {
        //spawnLights();
        originalY = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {

        moveCube();
        //orbitLights();
    }

    /**
     * Orbit lightA and lightB around each other
     */ 
    void orbitLights()
    {
        Vector3 halfPos = Vector3.Lerp(lightA.transform.position, lightB.transform.position, 0.5f);
        lightA.transform.RotateAround(halfPos, Vector3.up, lightOrbitSpeed * Time.deltaTime);
        lightB.transform.RotateAround(halfPos, Vector3.up, lightOrbitSpeed * Time.deltaTime);
    }

    void moveCube()
    {
        transform.position -= transform.forward * Time.deltaTime * movementSpeed;   // Move environment blocks towards camera

        // Vertical Jitter
        if (transform.position.y > originalY + jitterBoundY)   // Object is above jitter boundary
        {
            jitterIncrementY *= -1; // Toggle jitter increment
        }
       else if (transform.position.y < originalY - jitterBoundY)   // Object is above jitter boundary
        {
            jitterIncrementY *= -1; // Toggle jitter increment
        }
        //float jitterSpeed = Vector3.Distance(transform.position, new Vector3(transform.position.x, originalY, transform.position.z)) + 1; // Value should be larger 
        transform.position += Vector3.up * Time.deltaTime * jitterIncrementY;

        if (transform.position.z < cameraObject.transform.position.z)   // If cube has passed the camera, reset to original position
        {
            Vector3 spawnLocation = transform.position;
            spawnLocation.z = cameraObject.transform.position.z + spawnDistance;
            transform.position = spawnLocation;
        }
    }

    /**
     * Returns a list of colors that obey the golden ratio 
     * Follows algorithm from http://martin.ankerl.com/2009/12/09/how-to-create-random-colors-programmatically/
     */
    List<Color> getGoldenRatioColors(float numColors=2, float saturation = 1f, float value = 0.95f)
    {
        float goldenRatioConjugate = 0.618033988749895f;
        List<Color> colors = new List<Color>();
        float hue = Random.Range(0, 1f);
        for (int i = 0; i < numColors; i++)
        {
            hue += goldenRatioConjugate;
            hue %= 1;
            Color newColor = Color.HSVToRGB(hue, saturation, value);
            colors.Add(newColor);
        }
        return colors;
    }

    /**
     * Spawns light objects on top side of box collider
     */
    public void spawnLights(int numLights)
    {
        Vector3 lightPosA = transform.position;
        Vector3 lightPosB = transform.position;
        Vector3 cubeDimensions = GetComponent<BoxCollider>().size;
        float lightOffsetY = cubeDimensions.y * transform.localScale.y / 2 + lightPadding;

        List<Color> colors = getGoldenRatioColors(numLights);
        // Base case: 1 light to spawn
        if (numLights == 1)
        {
            spawnLightAt(new Vector3(transform.position.x, lightOffsetY, transform.position.z), colors[0]);
        }
        else
        {
            for (int i = 0; i < numLights; i++)
            {
                Color color = colors[i];
                float xCoord = transform.position.x + lightSpawnRadius * Mathf.Cos(2 * Mathf.PI * i / numLights);
                float ZCoord = transform.position.z + lightSpawnRadius * Mathf.Sin(2 * Mathf.PI * i / numLights);
                spawnLightAt(new Vector3(xCoord, lightOffsetY, ZCoord), color, "Light " + i.ToString());
            }
        }
    }

    GameObject spawnLightAt(Vector3 spawnPosition, Color color, string lightName="Light")
    {
        GameObject lightGameObject = new GameObject(lightName);
        //GameObject lightGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);  // Here for debug to see where lights spawn
        lightGameObject.transform.parent = transform;
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.intensity = lightIntensity;
        lightComp.color = color;
        lightGameObject.transform.position = spawnPosition;
        return lightGameObject;
    }
}
