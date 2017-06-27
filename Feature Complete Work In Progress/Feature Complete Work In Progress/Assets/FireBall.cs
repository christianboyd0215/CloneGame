using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour {
	// Use this for initialization
	void Start () {
        

	}
	
	void OnTriggerEnter2D(Collider2D trigger)
    {
        if(trigger.gameObject.CompareTag("FireBallKillzone"))
        {
            Destroy(gameObject);
        }
    }
}
