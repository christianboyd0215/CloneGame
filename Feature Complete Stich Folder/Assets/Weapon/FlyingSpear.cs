using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSpear : MonoBehaviour
{
    public Vector2 Speed = new Vector2(15f, 0f);
    private GameObject Character;
    private Rigidbody2D rb;
    public GameObject BackBar;
    private GameObject HeldSpear;
    private Vector3 MoveSpeed;
    private Vector3 GritySpeed = Vector3.zero;
    private Vector3 currentAngle;
    public float RecycleRange = 20f;
    private float BackX;
    private float BackY;
    private float bTime;
    public float Power = 30;
    public float Angle = 5;
    public float Gravity = -10;
    public float Ground;
    private float dTime;
    private float direction;
    private bool done = false;
    private bool Shoot = false;
    private bool Flying = true;
    public bool Back = false;

    void Start()
    {
        HeldSpear = GameObject.FindWithTag("HeldSpear");
        Character = GameObject.FindWithTag("Player");
        BackBar = GameObject.FindWithTag("Bar");
        direction = Character.transform.localScale.x / Mathf.Abs(Character.transform.localScale.x);
        MoveSpeed = Quaternion.Euler(new Vector3(0, 0, Angle)) * Vector3.right * Power;
        MoveSpeed = new Vector3(MoveSpeed.x * direction, 1, 1);
        currentAngle = Vector3.zero;
        rb = gameObject.GetComponent<Rigidbody2D>();
        Ground = Character.GetComponent<GroundHeight>().Ground;

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.GetComponent<Rigidbody2D>().simulated = false;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        if (!collision.gameObject.CompareTag("Player") && !Back)
        {
            Speed = Vector2.zero;
            Flying = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;

        }
        else
        {
            if (collision.gameObject.CompareTag("Player") && Back)
            {
                Speed = Vector2.zero;
                Flying = false;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
    
    void Update()
    {
        BackBar = GameObject.FindWithTag("Bar");
        Character = GameObject.FindWithTag("Player");

        Shoot = Input.GetKeyDown(KeyCode.LeftControl);
        if (!HeldSpear.GetComponent<HeldSpear>().WasShot)
        {
            Destroy(gameObject);
        }
        if (!done)
        {
            done = GameObject.Find("Counter").GetComponent<SpearBackCounter>().stage3;
        }

        if (gameObject.transform.position.y <= Ground)
        {
            gameObject.GetComponent<Rigidbody2D>().simulated = false;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        if (BackBar != null)
        {
            if (BackBar.transform.localScale.x >= 1)
            {
                if (!done)
                {
                    _Back();
                }
            }
        }

        if (!Flying)
        {
            if (Shoot && Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 2 && Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y) <= 2)
            {
                Destroy(gameObject, 0.1f);
            }
        }

        if (Back)
        {
            rb.velocity = Speed;
            if (Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= 1)
            {
                rb.velocity = Vector2.zero;
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                Flying = false;
            }
        }

    }

    private void FixedUpdate()
    {
        if (Flying && !Back)
        {
            Fly();
        }

    }

    void _Back()
    {
        if (Mathf.Abs(Character.transform.position.x - gameObject.transform.position.x) <= RecycleRange && Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y) <= RecycleRange)
        {
            gameObject.GetComponent<Rigidbody2D>().simulated = true;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            Flying = true;
            Back = true;
            BackX = 15f * (Character.transform.position.x - gameObject.transform.position.x);
            bTime = Mathf.Abs((Character.transform.position.x - gameObject.transform.position.x)) / 15f;
            if (Character.transform.position.y >= gameObject.transform.position.y)
            {
                BackY = Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y) / bTime * 2f;
            }
            else
            {
                BackY = -Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y) / bTime * 2f;
            }
            Speed = new Vector2(BackX, BackY);

        }
    }

    void Fly()
    {
        GritySpeed.y = Gravity * (dTime += Time.fixedDeltaTime);
        transform.position += (MoveSpeed + GritySpeed) * Time.fixedDeltaTime;
        currentAngle.z = Mathf.Atan((MoveSpeed.y + GritySpeed.y) / MoveSpeed.x) * Mathf.Rad2Deg;
        transform.eulerAngles = currentAngle * 2;
    }
}