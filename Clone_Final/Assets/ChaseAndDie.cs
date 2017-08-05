using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class ChaseAndDie : MonoBehaviour {

    public float chaseSpeed;
    public float sprintSpeed;
    public float sprintTime;
    Transform player;               // Reference to the player's position.
    float counter;
    float xDistance;
    float yDistance;
    float Distance;
    public GameObject wayBack;
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
        if (counter <= sprintTime)
        {
            xDistance = -transform.position.x + player.position.x;
            yDistance = -transform.position.y + player.position.y;
            Distance = (float)(Math.Sqrt(xDistance * xDistance + yDistance * yDistance));
            unitxDistance = xDistance / Distance;
            unityDistance = yDistance / Distance;
            transform.Translate(unitxDistance * chaseSpeed, unityDistance * chaseSpeed, 0);
        }

        else if (counter <= sprintTime + 0.5)
        {

        }

        else if (counter <= sprintTime + 1.5)
        {
            transform.Translate(unitxDistance * sprintSpeed, unityDistance * sprintSpeed, 0);
        }

        else if (counter >= sprintTime + 2.5)
            counter = 0;

        counter += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Destroy(GameObject.FindGameObjectWithTag("BossWall"));
            player.GetComponent<PlatformerCharacter2D>().KillBoss("Spin");
            Instantiate(wayBack, new Vector3(70.16448f, 217.7765f, 0), Quaternion.Euler(0, 0, 0));
            Destroy(gameObject);
        }
    }
}
