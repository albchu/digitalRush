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
    public float movementSpeed = 5f;
    public bool respawnSlices = false;

    // If we only generate mapLength amount of new slices before starting to pop dormant slices, there wont be any to pop. 
    // This padding ensures that we always have some extra dormant slices to use when we need them
    public int numPaddingSlices = 2;

    private int numSent = 0;
    private float nextSpawnDistance;
    private float firstPrefabMaxDistance = 0;
    private List<CubeNode[,]> activeSlices;
    private Queue<CubeNode[,]> dormantSlices;
    private Stack<CubeNode> cubePool;
    private Vector3 cubeDimensions;
    private Vector3 halfCubeDimensions;
    private int numSlicesGenerated;
    private float despawnDistance;
    private float lengthThreshold;    // The distance that the last slice must have traveled before we generate a new slice

    // Use this for initialization
    void Start()
    {
        numSlicesGenerated = 0;
        //print("hello");
        //doneSpawning = false;
        print(Vector3.Scale(new Vector3(1, 2, 3), new Vector3(2, 3, 4)));
        launchedPrefabs = new List<GameObject>();
        //map = new CubeNode[mapLength][,];
        activeSlices = new List<CubeNode[,]>();
        dormantSlices = new Queue<CubeNode[,]>();
        // Only generate a pool of cubes enough to fill the screen up to the player's distance. Theres no way ever that we will fill the screen with more blocks than that
        float totalCubesNeeded = mapWidth * mapHeight * mapLength * numPaddingSlices;
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
        lengthThreshold = cubeDimensions.z + lengthPadding;   // The distance that the last slice must have traveled before we generate a new slice

        despawnDistance = (mapLength + lengthPadding) * cubeDimensions.z;
        print("Despawn distance is " + despawnDistance);
    }

    void Update()
    {
        if (readyForNextSlice())
        {
            CubeNode[,] slice = generateSliceAtBegining();
            applyBehaviorFilter(slice, mapWidth, mapHeight);
        }

        despawnSlices();
        moveSlices();
    }

    private void applyBehaviorFilter(CubeNode[,] slice, int width, int height)
    {
        // For now just do the one plain one. Im tired, just want something dirty to see if it even works.
        for (int row = 0; row < width; row++)
        {
            for (int column = 0; column < height; column++)
            {
                CubeNode node = slice[row, column];

                if (row == 0 || row == width-1 || column == 0 || column == height - 1)
                {
                    node.spawn();
                }
                else
                {
                    node.despawn();
                }
            }
        }
    }

    /**
    * Returns whether we can generate another slice
    */
    private bool readyForNextSlice()
    {
        return (activeSlices.Count == 0) || (activeSlices[activeSlices.Count - 1][0, 0].getDistanceFrom(transform.position).z >= lengthThreshold);
    }

    private CubeNode[,] generateSliceAtBegining()
    {
        CubeNode[,] newSlice = null;
        if (numSlicesGenerated < mapLength + numPaddingSlices)
        {
            newSlice = spawnSlice(mapWidth, mapHeight);
            activeSlices.Add(newSlice);
            numSlicesGenerated++;
        }
        else if (respawnSlices && dormantSlices.Count > 0)
        {
            print("Spawning from dormant slices");
            newSlice = dormantSlices.Dequeue();
            activeSlices.Add(newSlice);
            initializeSlice(newSlice, mapWidth, mapHeight);
        }
        else
        {
            //print("Out of new slices to generate and respawn is disabled. To enable respawning, check 'Respawn Slices'");
        }
        return newSlice;
    }

    private void despawnSlices()
    {
        if(activeSlices.Count > 0)
        {
            // Performance shortcut: Only evaluate the first slice for despawn. This is the one thats farthest along
            CubeNode[,] lastSlice = activeSlices[0];
            CubeNode cubeFromFirstSlice = lastSlice[0,0];
            bool reachedDespawnThreshold = cubeFromFirstSlice.getDistanceFrom(transform.position).z >= despawnDistance;
            if (reachedDespawnThreshold)
            {
                despawnSlice(lastSlice, mapWidth, mapHeight);
                activeSlices.RemoveAt(0);
                dormantSlices.Enqueue(lastSlice);
                print("Despawning slice! We now have " + dormantSlices.Count + " dormant slices");
            }
        }
    }

    /**
    * A generic function that iterates over a slice and runs a void callback on each of the cubeNodes 
    */
    private void mapOverSlice(CubeNode[,] slice, int width, int height, System.Action<CubeNode> callback)
    {
        for (int row = 0; row < width; row++)
        {
            for (int column = 0; column < height; column++)
            {
                CubeNode node = slice[row, column];
                callback(node);
            }
        }
    }

    /* 
    * Moves all active slices
    */
    private void moveSlices()
    {
        for (int i = 0; i < activeSlices.Count; i++)
        {
            CubeNode[,] slice = activeSlices[i];
            moveSlice(slice, mapWidth, mapHeight);
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
                slice[row, column] = cube;
                //print("initialized cube at row " + row + " and column " + column);
            }
        }
        return slice;
    }


    /****** SLICE MAP FUNCTIONS ******/

    /**
    * Moves the slice forward by the movementSpeed
    */
    private void moveSlice(CubeNode[,] slice, int width, int height)
    {
        mapOverSlice(slice, width, height, moveNode);
    }

    private void despawnSlice(CubeNode[,] slice, int width, int height)
    {
        mapOverSlice(slice, width, height, despawnNode);
    }

    private void initializeSlice(CubeNode[,] slice, int width, int height)
    {
        mapOverSlice(slice, width, height, initializeNode);
    }

    /****** NODE MAP FUNCTIONS ******/

    private void initializeNode(CubeNode node)
    {
        node.reinitialize();
    }

    private void moveNode(CubeNode node)
    {
        node.getController().moveCube(movementSpeed);
    }

    private void despawnNode(CubeNode node)
    {
        node.despawn();
    }

    private class CubeNode
    {
        private GameObject cubeObj;
        private CubeController cubeController;
        private Vector3 originalPos;
        public CubeNode(GameObject gameObj)
        {
            cubeObj = gameObj;
            cubeController = cubeObj.GetComponent<CubeController>();

            // On instantiation, we dont want to do anything with it.
            cubeObj.SetActive(false);
        }

        public CubeController getController()
        {
            return cubeController;
        }

        /**
         * Initializes the cube at row and column of the current gameObject's z position
         */
        public void initializeAt(Vector3 pos)
        {
            originalPos = pos;
            cubeObj.transform.position = pos;
            renderCube();
            cubeController.originalPos = pos; // We want it to respawn at this location and not where it was initalized in the pool
        }

        /**
        * Not quite the same as Vector3.Distance. This returns the distances on each axis
        */ 
        public Vector3 getDistanceFrom(Vector3 pos)
        {
            return new Vector3(Mathf.Abs(cubeObj.transform.position.x - pos.x), Mathf.Abs(cubeObj.transform.position.y - pos.y), Mathf.Abs(cubeObj.transform.position.z - pos.z));
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
            //print("There is a " + (chancePercentage * 100) + "% chance that this cube will render");
            chancePercentage = Mathf.Clamp(chancePercentage, 0, 1); // Incase someone is being a dumb bum
            if (Random.value > 1f - chancePercentage)   // Calculating the chance in reverse. ie: if chance percentage is .8, then the random value must be between 0 and 0.2 aka 1 - 0.8
            {
                spawn();
            }
            else
            {
                despawn();
            }
        }

        public void spawn()
        {
            cubeObj.SetActive(true);
        }

        public void despawn()
        {
            cubeObj.SetActive(false);
        }

        public Vector3 getCubeDimensions()
        {
            return Vector3.Scale(cubeObj.GetComponent<BoxCollider>().size, cubeObj.transform.localScale);   // Not true size of cube without respect to local scale of it
        }

        public void reinitialize()
        {
            this.initializeAt(originalPos);
        }
    }
}
