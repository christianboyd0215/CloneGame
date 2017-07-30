using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearEffect : MonoBehaviour {

    private GameObject HeldSpear;
	void Start () {
        HeldSpear = GameObject.FindWithTag("HeldSpear");

    }
	
	void Update () {
        if (gameObject != null && HeldSpear != null)
        {
            if (gameObject != null && !HeldSpear.GetComponent<HeldSpear>().WasShot)
            {
                Destroy(gameObject, 2);
            }
        }
    }
}
