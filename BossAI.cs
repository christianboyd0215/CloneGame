using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour {

    Transform player;               // Reference to the player's position.
    //PlayerHealth playerHealth;      // Reference to the player's health.
    //EnemyHealth enemyHealth;        // Reference to this enemy's health.
    //NavMeshAgent nav;               // Reference to the nav mesh agent.////change to rigidbody
    bool jump;
    System.Random randGen;
    int randNum;
    Rigidbody2D boss_Rigidbody2D;
    // Use this for initialization
    void Awake () {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // playerHealth = player.GetComponent<PlayerHealth>();
        // enemyHealth = GetComponent<EnemyHealth>();
        jump = true;
        randGen = new System.Random();
        boss_Rigidbody2D = GetComponent<Rigidbody2D>();
    }
	

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        // If the enemy and the player have health left...
        if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            // ... set the destination of the nav mesh agent to the player.
            nav.SetDestination(player.position);
        }
        // Otherwise...
        else
        {
            // ... disable the nav mesh agent.
            nav.enabled = false;
        }
        */
        randNum = randGen.Next(0, 10);
        if (jump)
        {
            
            if ((4 <= randNum) && (randNum <= 6))
            {
                //jump up
                boss_Rigidbody2D.AddForce(new Vector2(0, 150));
                jump = false;
            }
            else if (4 < randNum)
            {
                //jump left
                boss_Rigidbody2D.AddForce(new Vector2(100, 100));
                jump = false;
            }
            else
            {
                // jump right
                boss_Rigidbody2D.AddForce(new Vector2(-100, 100));
                jump = false;
            }
        }


    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jump = true;
        }
        if (collision.gameObject.CompareTag("Spear"))
        {
            //kills enemy
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jump = true;
        }
    }
}



