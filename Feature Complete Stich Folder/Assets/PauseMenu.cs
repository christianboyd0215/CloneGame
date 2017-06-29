using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    public GameObject PauseUI;
    bool paused = false;

	// Use this for initialization
	void Start () {

        PauseUI.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        if (paused)
        {
            PauseUI.SetActive(true);
            Time.timeScale = 0;
        }

        else
        {
            PauseUI.SetActive(false);
            Time.timeScale = 1;
        }
	}
}
