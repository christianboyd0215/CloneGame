using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class preFinalFight : MonoBehaviour {

    public GameObject Phase2;
    bool flyAway = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear") && flyAway)
        {
            flyAway = false;
            Instantiate(Phase2, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
        }
    }

    void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
