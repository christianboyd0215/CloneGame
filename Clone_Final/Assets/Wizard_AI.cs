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
    GameObject Player;
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
    private float wave;
    private float waveCounter;
    public int number_of_waves;
    public float speed_of_waves;
    private bool bigBang;


    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        counter = Random.Range(lowerrange, upperrange);
        gameObject.transform.position = Location1.transform.position;
        CurrentLocation = Location1;
        finalAttackCounter = 0;
        finalAttack = false;
        timer = 0;
        waveCounter = 0;
        wave = finalAttackTimer / number_of_waves;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(finalAttack)
        {
            float timePast;
            if(timer>= finalAttackTimer)
            {
                bigBang = true;
                finalAttack = false;
                StartCoroutine(BigBang(expansionTime));
            }
            else
            {
                if (waveCounter >= wave)
                {
                    newFireBall = Instantiate(fireball, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity);
                    gameObject.GetComponent<AudioSource>().Play();
                    speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
                    //float vectxsqr = (Player.transform.position.x - gameObject.transform.position.x) * (Player.transform.position.x - gameObject.transform.position.x);
                    //float vectysqr = (Player.transform.position.y - gameObject.transform.position.y) * (Player.transform.position.y - gameObject.transform.position.y);
                    speedFireBall.velocity = new Vector2(speed_of_waves, 0);
                    newFireBall = Instantiate(fireball, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity);
                    speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
                    //float vectxsqr = (Player.transform.position.x - gameObject.transform.position.x) * (Player.transform.position.x - gameObject.transform.position.x);
                    //float vectysqr = (Player.transform.position.y - gameObject.transform.position.y) * (Player.transform.position.y - gameObject.transform.position.y);
                    speedFireBall.velocity = new Vector2(-speed_of_waves, 0);
                    waveCounter = 0;
                }
                timePast = Time.deltaTime;
                timer += timePast;
                waveCounter += timePast;
            }
        }
        else if (bigBang)
        {
            timer = Time.deltaTime;
            timer = 0;

        }
        else if ((timer >= counter) && !finalAttack && !bigBang)
        {
            newFireBall = Instantiate(fireball, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), Quaternion.identity);
            gameObject.GetComponent<AudioSource>().Play();
            speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
            //float vectxsqr = (Player.transform.position.x - gameObject.transform.position.x) * (Player.transform.position.x - gameObject.transform.position.x);
            //float vectysqr = (Player.transform.position.y - gameObject.transform.position.y) * (Player.transform.position.y - gameObject.transform.position.y);
            speedFireBall.velocity = new Vector2((Player.transform.position.x - gameObject.transform.position.x) /
                speedreduction, (Player.transform.position.y - gameObject.transform.position.y) / speedreduction);
            timer = 0;
            counter = Random.Range(lowerrange, upperrange);
            finalAttackCounter += 1;
            if (Random.Range(0, 1) > 0.9)
            {
                Teleport();
            }
            if(finalAttackCounter>=6)
            {
                finalAttack = true;
                gameObject.transform.position = FinalAttackPosition.transform.position;
                //Rigidbody2D playerthrow = Player.GetComponent<Rigidbody2D>();
                //playerthrow.AddForce(new Vector2(100 * ((Player.transform.position.x - gameObject.transform.position.x) / (Player.transform.position.x - gameObject.transform.position.x)), 30));
                timer = 0;
                finalAttackCounter = 0;
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
            finalAttackCounter += 1;
            

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
            if (other.gameObject.CompareTag("Player"))
            {
                Teleport();
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Destroy(GameObject.FindGameObjectWithTag("BossWall2"));
            Destroy(gameObject);
        }
    }

    IEnumerator BigBang(float expansionTime)
    {
        float time = 0;
        Vector2 originalScale = finalAttackCollider.transform.localScale;
        Vector2 newScale = new Vector2(finalAttackCollider.transform.localScale.x * scaleForFinalAttack, finalAttackCollider.transform.localScale.y * scaleForFinalAttack);

        while(time<expansionTime)
        {
            //Debug.Log("Expanding");
            finalAttackCollider.transform.localScale = Vector2.Lerp(originalScale, newScale, (time / expansionTime));
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        time = 0;
        while(time<expansionTime)
        {
            finalAttackCollider.transform.localScale = Vector2.Lerp(newScale, originalScale, (time / expansionTime));
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        bigBang = false;
        finalAttack = false;
        timer = 0;

        yield return null;
    }
}
