using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallKillZone : MonoBehaviour {

    // Use this for initialization
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("FireBall"))
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("FireBall"))
        {
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerStay2D(Collision2D other)
    {
        //Destroy(gameObject);
    }
}

