using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour {

    public float movementSpeed = 30f;    // All objects are moving in the z direction towards the camera
    public float spawnDistance = 75f;   // Units that the object will spawn at
    public float lightPadding = 1f;     // The offset that lights will spawn over the cube
    public float despawnDistance = 30f;
    public float lightOrbitSpeed = 50f;
    public float lightIntensity = 3f;
    public float lightSpawnRadius = 2f;
    public bool hasRespawned = false;
    public string lightOrientation = "top";
    public List<GameObject> lights;
    public bool enableRespawn = true;
    // Y jitter
    public float jitterMaxSpeed = 10f;
    public float jitterBoundY = 0.5f;   // The max distance that the object can move away from its original Y position
    public float jitterIncrementY = 0.01f;      // The number of incremental units that the object can translate during a jitter
    public Vector3 originalPos;

    public enum Orientation
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    };

    // Use this for initialization
    void Start () {
        lights = new List<GameObject>();
        //spawnLights(4, (Orientation)System.Enum.Parse(typeof(Orientation), lightOrientation, true));
        originalPos = transform.position;
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
        //Vector3 halfPos = Vector3.Lerp(lightA.transform.position, lightB.transform.position, 0.5f);
        //lightA.transform.RotateAround(halfPos, Vector3.up, lightOrbitSpeed * Time.deltaTime);
        //lightB.transform.RotateAround(halfPos, Vector3.up, lightOrbitSpeed * Time.deltaTime);
    }

    void moveCube()
    {
        transform.position -= transform.forward * Time.deltaTime * movementSpeed;   // Move environment blocks towards camera

        // Vertical Jitter
        float jitterSpeed = Random.Range(1, jitterMaxSpeed);      // Random speed variable to make jitter more irratic;
        if (transform.position.y > originalPos.y + jitterBoundY)   // Object is above jitter boundary
        {
            jitterIncrementY *= -1; // Toggle jitter increment
            jitterSpeed = Random.Range(1, jitterMaxSpeed);      // Random speed variable to make jitter more irratic
        }
       else if (transform.position.y < originalPos.y - jitterBoundY)   // Object is above jitter boundary
        {
            jitterIncrementY *= -1; // Toggle jitter increment
            jitterSpeed = Random.Range(1, jitterMaxSpeed);      // Random speed variable to make jitter more irratic
        }
        transform.position += Vector3.up * Time.deltaTime * jitterIncrementY * jitterSpeed; // Vertical jitter movement

        if (enableRespawn && Vector3.Distance(originalPos, transform.position) > despawnDistance)   // If cube has passed the despawn threshold, reset to original position
        {
            transform.position = originalPos;
            hasRespawned = true;
      print("I HAVE RESPAWNED!");
        }
        else if (!enableRespawn)
        {
            this.gameObject.SetActive(enableRespawn);
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
     * side = the side of the cube that will have the lights
     */
    public void spawnLights(int numLights, Orientation side)
    {
        Vector3 cubeDimensions = GetComponent<BoxCollider>().size;
        List<Color> colors = getGoldenRatioColors(numLights);
        Vector3 centerPos = transform.position; // Initialize value
        float perpPoint1 = 0f;
        float perpPoint2 = 0f;
        float perpPoint3 = 0f;
        if (side == Orientation.Top)
        {
            centerPos.y += cubeDimensions.y * transform.localScale.y / 2 + lightPadding;
            perpPoint1 = centerPos.x;
            perpPoint2 = centerPos.z;
            perpPoint3 = centerPos.y;
        }
        else if (side == Orientation.Bottom)
        {
            centerPos.y -= cubeDimensions.y * transform.localScale.y / 2 + lightPadding;
            perpPoint1 = centerPos.x;
            perpPoint2 = centerPos.z;
            perpPoint3 = centerPos.y;
        }
        else if (side == Orientation.Right)
        {
            centerPos.x += cubeDimensions.x * transform.localScale.x / 2 + lightPadding;
            perpPoint1 = centerPos.y;
            perpPoint2 = centerPos.z;
            perpPoint3 = centerPos.x;
        }
        else if (side == Orientation.Left)
        {
            centerPos.x -= cubeDimensions.x * transform.localScale.x / 2 + lightPadding;
            perpPoint1 = centerPos.y;
            perpPoint2 = centerPos.z;
            perpPoint3 = centerPos.x;
        }
        else if (side == Orientation.Back)
        {
            centerPos.z += cubeDimensions.z * transform.localScale.z / 2 + lightPadding;
            perpPoint1 = centerPos.y;
            perpPoint2 = centerPos.x;
            perpPoint3 = centerPos.z;
        }
        else if (side == Orientation.Front)
        {
            centerPos.z -= cubeDimensions.z * transform.localScale.z / 2 + lightPadding;
            perpPoint1 = centerPos.y;
            perpPoint2 = centerPos.x;
        }
        else
        {
            print("No valid orientation given. Spawning no lights");
            return;
        }

        // Base case: 1 light to spawn
        if (numLights == 1)
        {
            lights.Add(spawnLightAt(centerPos, colors[0]));
        }
        else
        {
            for (int i = 0; i < numLights; i++)
            {
                Color color = colors[i];
                float pointA = perpPoint1 + lightSpawnRadius * Mathf.Cos(2 * Mathf.PI * i / numLights);
                float pointB = perpPoint2 + lightSpawnRadius * Mathf.Sin(2 * Mathf.PI * i / numLights);

                if (side == Orientation.Top || side == Orientation.Bottom)
                    lights.Add(spawnLightAt(new Vector3(pointA, centerPos.y, pointB), color, "Light " + i.ToString()));
                else if (side == Orientation.Right || side == Orientation.Left)
                    lights.Add(spawnLightAt(new Vector3(centerPos.x, pointA, pointB), color, "Light " + i.ToString()));
                else if (side == Orientation.Front ||side == Orientation.Back)
                    lights.Add(spawnLightAt(new Vector3(pointB, pointA, centerPos.z), color, "Light " + i.ToString()));
            }
        }
    }

    public void spawnLights(int numLights)        // DEPRECATED. Use function with orientation
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
