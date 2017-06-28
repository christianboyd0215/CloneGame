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
        xDistance = - transform.position.x + player.position.x;
        yDistance = - transform.position.y + player.position.y;
        Distance = (float)(Math.Sqrt(xDistance * xDistance + yDistance * yDistance));
        unitxDistance = xDistance / Distance;
        unityDistance = yDistance / Distance;

        transform.Translate(unitxDistance * chaseSpeed, unityDistance * chaseSpeed, 0);
        //position.x += unitxDistance;
        //position.y += unityDistance;

        // chase

        //position.x += unitxDistance;
        //position.y += unityDistance;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
            Destroy(gameObject);
    }
}
