using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearBackCounter : MonoBehaviour {
    private GameObject Center;
    private GameObject Bar;
    private float stage = 2f;
    private bool trigger = false;
    private bool stage1 = false;
    private bool stage2 = false;
    public bool stage3 = false;
    void Start()
    {
        Center = GameObject.Find("Center");
    }
    void Update()
    {
        Bar = GameObject.FindWithTag("Bar");
        
        if (Bar != null)
        {
            if (Bar.transform.localScale.x >= 1 && !trigger)
            {
                stage -= 1f;
            }
        }
        if (stage == 1f && !trigger)
        {
            trigger = true;
            stage1 = true;
        }

        if (stage == 0 && !trigger)
        {
            trigger = true;
            stage2 = true;
        }
        if (stage3)
        {
            Destroy(Center);
        }
    }
    void FixedUpdate () {


        if (stage1)
        {
            if (gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount >= 0.51f)
            {
                gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount -= 0.01f;
            }
            else
            {
                stage1 = false;
                trigger = false;
            }
        }

        if (stage2)
        {
            if (gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount >= 0.01)
            {
                gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount -= 0.01f;
            }
            else
            {
                stage2 = false;
                trigger = false;
                stage3 = true;
            }
        }
    }
}
