using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Wizard_AI : MonoBehaviour {
    public GameObject Location1;
    public GameObject Location2;
    public GameObject Location3;
    public GameObject Location4;
    public GameObject Location5;
    public GameObject Location6;
    public GameObject Location7;
    public GameObject Location8;
    public GameObject Location9;
    public GameObject Location10;
    public GameObject Location11;
    public GameObject FinalAttackPosition;
    public GameObject Player;
    public float lowerrange;
    public float upperrange;
    public float speedreduction;
    private float timer;
    private float counter;
    private GameObject newFireball;
    private RigidBody2D speedFireBall;
    private GameObject CurrentLocation;


    // Use this for initialization
    void Start () {
        counter = Random.Range(lowerrange, upperrange);
        gameObject.transform.position = Location1;
        CurrentLocation = Location1;
	}
	
	// Update is called once per frame
	void Update () {
		if(timer>=counter)
        {
            newFireBall = Instantiate(fireball, new Vector2(xpos, ypos), Quaternion.identity);
            speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
            speedFireBall.velocity = new Vector2((Player.transform.position.x - gameObject.transform.position.x)/speedreduction, velocityOfShotsY);
            timer = 0; 
            counter = Random.Range(lowerrange, upperrange);
            if (Random.Range(0, 1) < 0.1)
            {
                Teleport();
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
        while(needsToTeleport)
        {
            int x = Random.Range(0, 12);
            if (x<1&&(CurrentLocation!=Location1))
            {
                gameObject.transform.position = Location1.transform.position;
                CurrentLocation = Location1;
            }
            else if(x<2 && (CurrentLocation != Location2))
            {
                gameObject.tranform.position = Location2.transform.position;
                CurrentLocation = Location2; ;
            }

        }
        

    }
}
