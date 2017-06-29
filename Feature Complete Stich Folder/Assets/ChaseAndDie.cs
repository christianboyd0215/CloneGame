using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAndDie : MonoBehaviour {

    public float chaseSpeed;
    Transform player;               // Reference to the player's position.
    float counter;
    float xDistance;
    float yDistance;
    float Distance;
    float unitxDistance;
    float unityDistance;


    // Use this for initialization
    void Start()
    {

    }

    void Awake()
    {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        // get direction
        if (counter <= 4.8)
        {
            xDistance = -transform.position.x + player.position.x;
            yDistance = -transform.position.y + player.position.y;
            Distance = (float)(Math.Sqrt(xDistance * xDistance + yDistance * yDistance));
            unitxDistance = xDistance / Distance;
            unityDistance = yDistance / Distance;
            transform.Translate(unitxDistance * chaseSpeed, unityDistance * chaseSpeed, 0);
        }

        else if (counter <= 5.3)
        {

        }

        else if (counter <= 6.3)
        {
            transform.Translate(unitxDistance * chaseSpeed * 5, unityDistance * chaseSpeed * 5, 0);
        }

        else if (counter >= 7.3)
            counter = 0;

        counter += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Destroy(GameObject.FindGameObjectWithTag("BossWall"));
            Destroy(gameObject);
        }
    }
}
