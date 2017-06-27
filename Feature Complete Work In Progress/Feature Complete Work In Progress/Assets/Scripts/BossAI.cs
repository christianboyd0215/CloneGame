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
    float counter;
    Rigidbody2D boss_Rigidbody2D;
    BoxCollider2D HitBox;
    // Use this for initialization
    void Start()
    {
        
    }
    void Awake () {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // playerHealth = player.GetComponent<PlayerHealth>();
        // enemyHealth = GetComponent<EnemyHealth>();
        jump = true;
        randGen = new System.Random();
        boss_Rigidbody2D = GetComponent<Rigidbody2D>();
        HitBox = GetComponent<BoxCollider2D>();
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
        counter += Time.deltaTime;
        if (counter>=3)
        {
            boss_Rigidbody2D.AddForce(new Vector2(0, 1000));
            counter = 0;
        }


    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("should be jumping.");
        if (collision.gameObject.CompareTag("Ground"))
        {
            /*
            randNum = randGen.Next(0, 10);
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
            }*/
        }
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
                Destroy(gameObject);
                //kills enemy
                Debug.Log("Kill Enemy");

        }
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("should be jumping.");
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("should be jumping.");
            boss_Rigidbody2D.AddForce(new Vector2(0, 150));
        }
    }
}



