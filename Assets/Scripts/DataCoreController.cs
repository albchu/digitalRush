﻿using System.Collections;
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

    private int numSent = 0;
    private CubeNode lastCube;
    private float nextSpawnDistance;
    private float firstPrefabMaxDistance = 0;
    //private CubeNode[][,] map;
    private List<CubeNode[,]> activeSlices;
    private Stack<CubeNode> cubePool;
    //private bool doneSpawning;
    private Vector3 cubeDimensions;
    private Vector3 halfCubeDimensions;
    private int numSlicesGenerated;
    private float despawnDistance;

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
        despawnDistance = (mapLength + lengthPadding) * cubeDimensions.z;
        print("Despawn distance is " + despawnDistance);
    }

    void Update()
    {
        generateSliceAtBegining();
        despawnSlices();
        moveSlices();
    }


    private CubeNode[,] generateSliceAtBegining()
    {
        CubeNode[,] newSlice = null;
        if (numSlicesGenerated < mapLength)
        {
            // Figure out whether it's time to spawn another one
            float lengthThreshold = cubeDimensions.z + lengthPadding;
            if (lastCube == null || (transform.position.z - lastCube.getPosition().z) >= lengthThreshold)
            {
                newSlice = spawnSlice(mapWidth, mapHeight);
                activeSlices.Add(newSlice);
                numSlicesGenerated++;
            }
            print("Current number of slices generated" + numSlicesGenerated);

        }
        else if (respawnSlices)
        {
            // TODO: Add this logic
        }
        else
        {
            print("Out of new slices to generate and respawn is disabled. To enable respawning, check 'Respawn Slices'");
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
            }
        }
    }

    private void despawnSlice(CubeNode[,] slice, int width, int height)
    {
        mapOverSlice(slice, width, height, despawnNode);
    }

    /**
    * A generic function that iterates over a slice and runs a void callback on each of the cubeNodes 
    */
    private void mapOverSlice(CubeNode[,] slice, int width, int height, System.Action<CubeNode, int, int> callback)
    {
        for (int row = 0; row < width; row++)
        {
            for (int column = 0; column < height; column++)
            {
                CubeNode node = slice[row, column];
                callback(node, row, column);
            }
        }
    }

    private void moveSlices()
    {
        for (int i = 0; i < activeSlices.Count; i++)
        {
            CubeNode[,] slice = activeSlices[i];
            moveSlice(slice, mapWidth, mapHeight);
        }
    }

    /**
    * Moves the slice forward by the movementSpeed
    */
    private void moveSlice(CubeNode[,] slice, int width, int height)
    {
        mapOverSlice(slice, width, height, moveNode);
    }

    private void moveNode(CubeNode node, int row, int column)
    {
        node.getController().moveCube(movementSpeed);
    }

    private void despawnNode(CubeNode node, int row, int column)
    {
        node.despawn();
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
                cube.initializeAt(position, despawnDistance);
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

        //public float 

        public CubeController getController()
        {
            return cubeController;
        }

        /**
         * Initializes the cube at row and column of the current gameObject's z position
         */
        public void initializeAt(Vector3 pos, float despawnDistance)
        {
            cubeObj.transform.position = pos;
            renderCube();
            //cubeController.movementSpeed = 10f;   // Debug, i dont wanna see it move until we're ready
            //cubeController.despawnDistance = despawnDistance;
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

        // todo: pass speed in so we can controll it from the datacore
    }
}
