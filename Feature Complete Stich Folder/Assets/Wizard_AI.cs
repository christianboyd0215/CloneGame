using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Wizard_AI : MonoBehaviour
{
    public GameObject Location1;
    public GameObject Location2;
    public GameObject Location3;
    public GameObject Location4;
    public GameObject Location5;
    public GameObject Location6;
    public GameObject Location7;
    public GameObject Location8;
    public GameObject FinalAttackPosition;
    public GameObject Player;
    public GameObject fireball;
    public GameObject finalAttackCollider;
    public float expansionTime;
    public float lowerrange;
    public float upperrange;
    public float speedreduction;
    public float scaleForFinalAttack;
    private float timer;
    private float counter;
    private bool finalAttack;
    private int finalAttackCounter;
    public float finalAttackTimer;
    private GameObject newFireBall;
    private Rigidbody2D speedFireBall;
    private GameObject CurrentLocation;
    private float wave1;
    private float wave2;
    private float wave3;
    private bool bigBang;


    // Use this for initialization
    void Start()
    {
        counter = Random.Range(lowerrange, upperrange);
        gameObject.transform.position = Location1.transform.position;
        CurrentLocation = Location1;
        finalAttackCounter = 0;
        finalAttack = false;
        timer = 0;
        wave1 = finalAttackTimer / 4;
        wave2 = (finalAttackTimer * 2) / 4;
        wave3 = (finalAttackTimer * 3) / 4;
        bigBang = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(finalAttack)
        {
            float wave1 = finalAttackTimer / 4;
            float wave2 = (finalAttackTimer * 2) / 4;
            float wave3 = (finalAttackTimer * 3) / 4;
            if(timer>= finalAttackTimer)
            {
                bigBang = true;
                finalAttack = false;
                StartCoroutine(BigBang(expansionTime));
            }
            else
            {

                timer += Time.deltaTime;
            }
        }
        else if (bigBang)
        {
            timer = 0;
        }
        else if ((timer >= counter) && !finalAttack && !bigBang)
        {
            newFireBall = Instantiate(fireball, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity);
            speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
            //float vectxsqr = (Player.transform.position.x - gameObject.transform.position.x) * (Player.transform.position.x - gameObject.transform.position.x);
            //float vectysqr = (Player.transform.position.y - gameObject.transform.position.y) * (Player.transform.position.y - gameObject.transform.position.y);
            speedFireBall.velocity = new Vector2((Player.transform.position.x - gameObject.transform.position.x) /
                speedreduction, (Player.transform.position.y - gameObject.transform.position.y) / speedreduction);
            timer = 0;
            counter = Random.Range(lowerrange, upperrange);
            finalAttackCounter += 1;
            if (Random.Range(0, 1) < 0.1)
            {
                Teleport();
            }
            if(finalAttackCounter>=6)
            {
                finalAttack = true;
                gameObject.transform.position = FinalAttackPosition.transform.position;
                timer = 0;
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    void Teleport()
    {
        bool needsToTeleport = true;
        Debug.Log("teleportation should occur");
        while (needsToTeleport)
        {
            float x = Random.Range(0, 11);
            if (x < 0 && (CurrentLocation != Location1))
            {
                gameObject.transform.position = Location1.transform.position;
                CurrentLocation = Location1;
                needsToTeleport = false;
            }
            else if (x < 1 && (CurrentLocation != Location2))
            {
                gameObject.transform.position = Location2.transform.position;
                CurrentLocation = Location2;
                needsToTeleport = false;
            }
            else if (x < 2 && (CurrentLocation != Location3))
            {
                gameObject.transform.position = Location3.transform.position;
                CurrentLocation = Location3;
                needsToTeleport = false;
            }
            else if (x < 3 && (CurrentLocation != Location4))
            {
                gameObject.transform.position = Location4.transform.position;
                CurrentLocation = Location4;
                needsToTeleport = false;
            }
            else if (x < 4 && (CurrentLocation != Location5))
            {
                gameObject.transform.position = Location5.transform.position;
                CurrentLocation = Location5;
                needsToTeleport = false;
            }
            else if (x < 5 && (CurrentLocation != Location6))
            {
                gameObject.transform.position = Location6.transform.position;
                CurrentLocation = Location6;
                needsToTeleport = false;
            }
            else if (x < 6 && (CurrentLocation != Location7))
            {
                gameObject.transform.position = Location7.transform.position;
                CurrentLocation = Location7;
                needsToTeleport = false;
            }
            else if (x < 7 && (CurrentLocation != Location8))
            {
                gameObject.transform.position = Location8.transform.position;
                CurrentLocation = Location8;
                needsToTeleport = false;
            }
            

        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!finalAttack)
        {
            if (other.gameObject.CompareTag("FlyingSpear"))
            {
                Teleport();
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator BigBang(float expansionTime)
    {
        float time = 0;
        Vector2 originalScale = finalAttackCollider.transform.localScale;
        Vector2 newScale = (finalAttackCollider.transform.localScale * scaleForFinalAttack);

        while(time<expansionTime)
        {
            finalAttackCollider.transform.localScale = Vector2.Lerp(originalScale, newScale, time / expansionTime);
            time += Time.deltaTime;
        }
        time = 0;
        while(time<1)
        {
            finalAttackCollider.transform.localScale = Vector2.Lerp(newScale, originalScale, time / expansionTime);
            time += Time.deltaTime;
        }
        bigBang = false;

        yield return null;
    }
}
