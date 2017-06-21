using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will control where all the enemy nodes come from
public class DataCoreController : MonoBehaviour {
    public float distanceBetweenSpawns = 2f;    // The amount of padding space between each prefab spawn. Assumes box colider on prefab for size calculations
    public float spawnUntilDistance = 50f;      // Will continuously spawn objects until the first one hits this distance away from spawn point
    public GameObject prefabToSpawn;
    public GameObject playerObj;
    public List<GameObject> launchedPrefabs;
    public int numLightsPerCube = 2;
    public int mapWidth = 6;
    public int mapHeight = 6;
    public int mapLength = 6;



    private int numSent = 0;
    private GameObject lastSpawn;
    private float nextSpawnDistance;
    private float firstPrefabMaxDistance = 0;
    private GameObject[,,] map;
    private List<GameObject> cubePool;
    // Use this for initialization
    void Start () {
        print("hello");
        launchedPrefabs = new List<GameObject>();
        map = new GameObject[mapWidth, mapHeight, mapLength];
        cubePool = new List<GameObject>();
        // Only generate a pool of cubes enough to fill the screen up to the player's distance. Theres no way ever that we will fill the screen with more blocks than that
        float totalCubesNeeded = mapWidth * mapHeight * Vector3.Distance(transform.position, playerObj.transform.position);
        print("To fill a space that is " + mapWidth + " by " + mapHeight + " away from the player, we need to generate " + totalCubesNeeded + " cubes");
        float timeStarted = Time.time;
        for (int i = 0; i < totalCubesNeeded; i++)
        {
            GameObject generatedCube = Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject;
            cubePool.Add(generatedCube);
        }
        float timeEnded = Time.time - timeStarted;
        print("Took " + timeEnded + " seconds to generate this pool");
    }
	
	void FixedUpdate () {
        bool readyForNextSpawn = (lastSpawn == null || Vector3.Distance(transform.position, lastSpawn.transform.position) > nextSpawnDistance);
        bool hasFirstPrefabHasRespawned = (launchedPrefabs.Count == 0) ? false : launchedPrefabs[0].GetComponent<CubeController>().hasRespawned;
        if (readyForNextSpawn && !hasFirstPrefabHasRespawned)
        {
            print("Sending Object!");
            nextSpawnDistance = prefabToSpawn.GetComponent<BoxCollider>().size.z + distanceBetweenSpawns;
            lastSpawn = Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject;
            CubeController cubeController = lastSpawn.GetComponent<CubeController>();
            //cubeController.spawnLights(numLightsPerCube);
            cubeController.despawnDistance = spawnUntilDistance;
            launchedPrefabs.Add(lastSpawn);
            numSent++;
            print("Sending next object at " + nextSpawnDistance);
        print("First element has traveled " + Vector3.Distance(launchedPrefabs[0].transform.position, transform.position));
        }
    }
}
