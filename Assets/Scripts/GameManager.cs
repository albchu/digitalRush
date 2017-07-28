using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameState.State currentState = GameState.State.Play;

	// Use this for initialization
	void Start () {
        currentState = GameState.State.Play;

    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(currentState == GameState.State.Paused)
            {
                currentState = GameState.State.Play;
            }
            else if (currentState == GameState.State.Play)
            {
                currentState = GameState.State.Paused;
            }
            print("GameState is now: " + currentState);
        }
	}
}
