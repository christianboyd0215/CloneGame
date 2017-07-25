using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour {

    public string startLevel;
    public string BossTrial;

    public void NewGame()
    {
        Application.LoadLevel(startLevel);
    }

    public void FinalBossTrial()
    {
        Application.LoadLevel(BossTrial);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
