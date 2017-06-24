using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void ButtonMenu(Button button)
    {
        //print(button);
        if(button.name == "NewGame")
        {
            print("Started New Game");
        }
        if (button.name == "LoadGame")
        {
            print("Loaded Our Game");
        }
        if (button.name == "Exit")
        {
            print("Exit");
        }
    }
}
