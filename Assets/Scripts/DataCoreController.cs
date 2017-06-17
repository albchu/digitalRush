using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will control where all the enemy nodes come from
public class DataCoreController : MonoBehaviour {

    public int numToSend = 5;
    public float secBetweenSpawns = 2f;
    public GameObject prefabToSpawn;
    public List<GameObject> launchedPrefabs;
    public int numLightsPerCube = 2;
    private float nextSpawnTime = 0f;
    private int numSent = 0;

	// Use this for initialization
	void Start () {
        launchedPrefabs = new List<GameObject>();
	}
	
	void FixedUpdate () {
        print("Current time: " + Time.time);
        if (Time.time > nextSpawnTime && numSent < numToSend)
        {
            print("Sending Object!");
            nextSpawnTime = Time.time + secBetweenSpawns;
            GameObject cube = Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject;
            CubeController cubeController = cube.GetComponent<CubeController>();
            cubeController.spawnLights(numLightsPerCube);
            numSent++;
            print("Sending next object at " + nextSpawnTime);
        }
    }
}
