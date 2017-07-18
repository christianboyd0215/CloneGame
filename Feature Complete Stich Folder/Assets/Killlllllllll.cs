using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killlllllllll : MonoBehaviour {

    float counter;
    bool added = true;

    // Use this for initialization
    private void FixedUpdate()
    {
        counter += Time.deltaTime;
        if (counter > 0.42 && added)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -70);
            added = false;
        }
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.CompareTag("FireBallKillzone"))
        {
            Destroy(gameObject);
        }
    }
}
