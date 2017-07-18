using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour {

    public string startLevel;

    public void NewGame()
    {
        Application.LoadLevel(startLevel);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
