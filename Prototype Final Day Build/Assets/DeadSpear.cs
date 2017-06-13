using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadSpear : MonoBehaviour {

    private GameObject Character;
    private Rigidbody2D rb;
    private bool Shoot = false;

    void Awake()
    {
        Character = GameObject.FindWithTag("Player");
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }


    void Update()
    {
        Character = GameObject.FindWithTag("Player");
        Shoot = Input.GetKeyDown(KeyCode.LeftControl);

        if (Shoot && Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 1)
        {
            Destroy(gameObject, 0.1f);
        }

        Shoot = false;
    }
}
