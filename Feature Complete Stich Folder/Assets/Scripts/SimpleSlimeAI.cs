using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSlimeAI : MonoBehaviour {

    Transform player;               // Reference to the player's position.
    float counter;
    Rigidbody2D boss_Rigidbody2D;
    Vector2 Direction;
    public float jumpheight;
    public float jumpwidth;
    public float minSize; // The slime won't split if it is under the minSize
    public float jumptime;

    // Use this for initialization
    void Start()
    {
        
    }
    void Awake()
    {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        boss_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // get direction
        if (player.position.x - boss_Rigidbody2D.position.x > 0)
            Direction = new Vector2(jumpwidth, jumpheight);
        else
            Direction = new Vector2(-jumpwidth, jumpheight);
        // chase
        counter += Time.deltaTime;
        if (counter >= 0.8 && Math.Abs(player.position.x - boss_Rigidbody2D.position.x) < 10)
        {
            boss_Rigidbody2D.AddForce(Direction);
            counter = 0;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            if (transform.localScale.y > minSize)
            {
                // split
                GameObject clone1 = Instantiate(gameObject, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;
                GameObject clone2 = Instantiate(gameObject, new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;

                clone1.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                clone2.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
            }
            Destroy(gameObject);
        }
    }
}
