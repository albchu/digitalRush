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
    public float widthPadding = 0.1f;
    public float heightPadding = 0.1f;
    public float lengthPadding = 0.1f;

    private int numSent = 0;
    private CubeNode lastCube;
    private float nextSpawnDistance;
    private float firstPrefabMaxDistance = 0;
    private CubeNode[][,] map;
    private Stack<CubeNode> cubePool;
    //private bool doneSpawning;
    private Vector3 cubeDimensions;
    private Vector3 halfCubeDimensions;
    private int numSlicesGenerated;

    // Use this for initialization
    void Start()
    {
        numSlicesGenerated = 0;
        //print("hello");
        //doneSpawning = false;
        print(Vector3.Scale(new Vector3(1, 2, 3), new Vector3(2, 3, 4)));
        launchedPrefabs = new List<GameObject>();
        map = new CubeNode[mapLength][,];
        // Only generate a pool of cubes enough to fill the screen up to the player's distance. Theres no way ever that we will fill the screen with more blocks than that
        float totalCubesNeeded = mapWidth * mapHeight * mapLength;
        cubePool = new Stack<CubeNode>();
        print("To fill a space that is " + mapWidth + " by " + mapHeight + " by " + mapLength + " , we need to generate " + totalCubesNeeded + " cubes");
        float timeStarted = Time.time;
        for (int i = 0; i < totalCubesNeeded; i++)
        {
            cubePool.Push(new CubeNode(Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject));
        }
        float timeEnded = Time.time - timeStarted;
        print("Took " + timeEnded + " seconds to generate this pool of " + cubePool.Count + " cubes");

        cubeDimensions = cubePool.Peek().getCubeDimensions();   // Use first cube as template of dimensions for all other cubes. Respect this assumption.
        halfCubeDimensions = cubeDimensions/2;   // We use this value a lot. Just saving it for performance optimization
    }
	
	void Update () {
        //bool readyForNextSpawn = (lastSpawn == null || Vector3.Distance(transform.position, lastSpawn.transform.position) > nextSpawnDistance);
        //bool hasFirstPrefabHasRespawned = (launchedPrefabs.Count == 0) ? false : launchedPrefabs[0].GetComponent<CubeController>().hasRespawned;
        //if (readyForNextSpawn && !hasFirstPrefabHasRespawned)
        //{
        //    print("Sending Object!");
        //    nextSpawnDistance = prefabToSpawn.GetComponent<BoxCollider>().size.z + distanceBetweenSpawns;
        //    lastSpawn = Instantiate(prefabToSpawn, transform.position, Quaternion.identity) as GameObject;
        //    CubeController cubeController = lastSpawn.GetComponent<CubeController>();
        //    //cubeController.spawnLights(numLightsPerCube);
        //    cubeController.despawnDistance = spawnUntilDistance;
        //    launchedPrefabs.Add(lastSpawn);
        //    numSent++;
        //    print("Sending next object at " + nextSpawnDistance);
        //print("First element has traveled " + Vector3.Distance(launchedPrefabs[0].transform.position, transform.position));
        //}



        if (numSlicesGenerated < mapLength)
        {
            // Figure out whether it's time to spawn another one
            float lengthThreshold = cubeDimensions.z + lengthPadding;
            if(lastCube == null || (transform.position.z - lastCube.getPosition().z) >= lengthThreshold)
            {
                map[numSlicesGenerated] = spawnSlice(mapWidth, mapHeight);
                numSlicesGenerated++;
            }

        }

    }

    /**
     * Generates a 2d list by [width][height] of cubenodes
     */
    private CubeNode[,] spawnSlice(int width, int height)
    {
        CubeNode[,] slice = new CubeNode[width, height];
        for (int row = 0; row < width; row++)
        {
            for (int column = 0; column < height; column++)
            {
                Vector3 position = new Vector3();
                position.x = transform.position.x * ((float)mapWidth * halfCubeDimensions.x) + row * cubeDimensions.x + widthPadding * row;
                position.y = transform.position.y * ((float)mapHeight * halfCubeDimensions.y) + column * cubeDimensions.y + heightPadding * column;
                position.z = transform.position.z;

                CubeNode cube = cubePool.Pop();
                cube.initializeAt(position);
                lastCube = cube;
                slice[row, column] = cube;
                //print("initialized cube at row " + row + " and column " + column);
            }
        }
        return slice;
    }

    private class CubeNode
    {
        private GameObject cubeObj;
        private CubeController cubeController;

        public CubeNode(GameObject gameObj)
        {
            cubeObj = gameObj;
            cubeController = cubeObj.GetComponent<CubeController>();

            // On instantiation, we dont want to do anything with it.
            cubeObj.SetActive(false);
        }

        /**
         * Initializes the cube at row and column of the current gameObject's z position
         */
        public void initializeAt(Vector3 pos)
        {
            cubeObj.transform.position = pos;
            renderCube();
            cubeController.movementSpeed = 2f;   // Debug, i dont wanna see it move until we're ready
        }

        public Vector3 getPosition()
        {
            return cubeObj.transform.position;
        }

        /**
         * Will render the cube based on a random chance (between 0-1) where 1 is 100% chance of render
         */
        public void renderCube(float chancePercentage=1f)
        {
            print("There is a " + chancePercentage + " that this cube will render");
            chancePercentage = Mathf.Clamp(chancePercentage, 0, 1); // Incase someone is being a dumb bum
            if (Random.value > 1f - chancePercentage)   // Calculating the chance in reverse. ie: if chance percentage is .8, then the random value must be between 0 and 0.2 aka 1 - 0.8
            {
                cubeObj.SetActive(true);
            }
            else
            {
                cubeObj.SetActive(false);
            }
        }

        public Vector3 getCubeDimensions()
        {
            return Vector3.Scale(cubeObj.GetComponent<BoxCollider>().size, cubeObj.transform.localScale);   // Not true size of cube without respect to local scale of it
        }

        // todo: pass speed in so we can controll it from the datacore
    }
}
