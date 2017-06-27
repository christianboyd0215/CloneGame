using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSpear : MonoBehaviour
{
    public Vector2 Speed = new Vector2(15f, 0f);
    private GameObject Character;
    private Rigidbody2D rb;
    private GameObject BackBar;
    private float BackX;
    private float BackY;
    private bool Shoot = false;
    private bool Flying = true;
    public bool Back = false;
    void Awake()
    {
        BackBar = Character = GameObject.FindWithTag("Bar");
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
            Flying = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }


    void Update()
    {
        Character = GameObject.FindWithTag("Player");
        Shoot = Input.GetKeyDown(KeyCode.LeftControl);

        if (gameObject.transform.position.y <= -19)
        {
            gameObject.GetComponent<Rigidbody2D>().simulated = false;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        if (BackBar.transform.localScale.x >= 1)
        {
            _Back();
        }

        if (!Flying)
        {
            if (Shoot && Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 1)
            {
                Destroy(gameObject, 0.1f);
            }
        }
        else
        {
            if (Shoot && Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 5)
            {
                _Back();
            }
        }

        if (Back)
        {
            rb.velocity = Speed;
            if(Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 1)
            {
                rb.velocity = Vector2.zero;
                Flying = false;
            }
        }
        else
        {
            rb.velocity = Speed;
        }

    }

    void _Back()
    {
        gameObject.GetComponent<Rigidbody2D>().simulated = true;
        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        Flying = true;
        Back = true;
        BackX = 15f * (Character.transform.position.x - gameObject.transform.position.x);
        if (Character.transform.position.y >= gameObject.transform.position.y)
        {
            BackY = Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y);
        }
        else
        {
            BackY = -Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y);
        }
        Speed = new Vector2(BackX, BackY);
    }
}
