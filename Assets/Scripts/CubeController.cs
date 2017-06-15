using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour {

    public float movementSpeed = 30f;    // All objects are moving in the z direction towards the camera
    public float spawnDistance = 75f;   // Units that the object will spawn at
    public float lightPadding = 1f;     // The offset that lights will spawn over the cube
    public GameObject cameraObject;
    public float lightOrbitSpeed = 50f;
    public float lightIntensity = 3f;
    public GameObject colliderObject;

    GameObject lightA;
    GameObject lightB;
    // Use this for initialization
    void Start () {
        BoxCollider boxCollider = colliderObject.GetComponent<BoxCollider>();
        spawnLights(boxCollider);
	}
	
	// Update is called once per frame
	void Update () {

        moveCube();
        orbitLights();
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
        this.gameObject.transform.position -= transform.forward * Time.deltaTime * movementSpeed;   // Move environment blocks

        if (this.gameObject.transform.position.z < cameraObject.transform.position.z)   // If cube has passed the camera, reset to original position
        {
            Vector3 spawnLocation = this.gameObject.transform.position;
            spawnLocation.z = cameraObject.transform.position.z + spawnDistance;
            this.gameObject.transform.position = spawnLocation;
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
    void spawnLights(BoxCollider boxCollider)
    {
        Vector3 lightPosA = this.gameObject.transform.position;
        Vector3 lightPosB = this.gameObject.transform.position;
        Vector3 cubeDimensions = boxCollider.size;
        float lightOffsetY = cubeDimensions.y / 2 + lightPadding;
        float lightOffsetX = cubeDimensions.x / 2 - lightPadding;

        lightPosA.y += lightOffsetY;
        lightPosB.y += lightOffsetY;

        lightPosA.x += lightOffsetX;
        lightPosB.x -= lightOffsetX;

        List<Color> colors = getGoldenRatioColors();
        Color colorA = colors[0];
        Color colorB = colors[1];
        lightA = spawnLightAt(lightPosA, colorA, "OrbitLightA");
        lightB = spawnLightAt(lightPosB, colorB, "OrbitLightB");
    }

    GameObject spawnLightAt(Vector3 spawnPosition, Color color, string lightName="someLight")
    {
        GameObject lightGameObject = new GameObject(lightName);
        lightGameObject.transform.parent = this.gameObject.transform;
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.intensity = lightIntensity;
        lightComp.color = color;
        lightGameObject.transform.position = spawnPosition;
        return lightGameObject;
    }
}
