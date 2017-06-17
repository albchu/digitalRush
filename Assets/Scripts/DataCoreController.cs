using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will control where all the enemy nodes come from
public class DataCoreController : MonoBehaviour {

    public int numToSend = 5;
    public float distanceBetweenSpawns = 2f;    // The amount of padding space between each prefab spawn. Assumes box colider on prefab for size calculations
    public GameObject prefabToSpawn;
    public List<GameObject> launchedPrefabs;
    public int numLightsPerCube = 2;

    private int numSent = 0;
    private GameObject lastSpawn;
    private float nextSpawnDistance;
    // Use this for initialization
    void Start () {
        print("hello");
        launchedPrefabs = new List<GameObject>();
	}
	
	void FixedUpdate () {
        if ((lastSpawn == null || Vector3.Distance(transform.position, lastSpawn.transform.position) > nextSpawnDistance) && numSent < numToSend)
        {
            print("Sending Object!");
            nextSpawnDistance = prefabToSpawn.GetComponent<BoxCollider>().size.z + distanceBetweenSpawns;
            lastSpawn = Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject;
            CubeController cubeController = lastSpawn.GetComponent<CubeController>();
            cubeController.spawnLights(numLightsPerCube);
            launchedPrefabs.Add(lastSpawn);
            numSent++;
            print("Sending next object at " + nextSpawnDistance);
        }
    }
}
