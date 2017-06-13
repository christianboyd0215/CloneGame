using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSpear : MonoBehaviour
{
    public Vector2 Speed = new Vector2(15f, 0f);
    private GameObject Character;
    private Rigidbody2D rb;
    private bool Shoot = false;

    void Awake()
    {
        Character = GameObject.FindWithTag("Player");
        Speed = new Vector2(Speed.x * Character.transform.localScale.x / Mathf.Abs(Character.transform.localScale.x), Speed.y * Mathf.Abs(Character.transform.localScale.y));
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            Speed = Vector2.zero;
        }
    }  


void Update()
    {
        Character = GameObject.FindWithTag("Player");
        Shoot = Input.GetKeyDown(KeyCode.LeftControl);
        rb.velocity = Speed;

        if (Shoot && Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 1)
        {
            Destroy(gameObject,0.1f);
        }

        Shoot = false;
    }
}
