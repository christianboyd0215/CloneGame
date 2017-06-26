using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSpawner : MonoBehaviour {

    public float timeBetweenShots;
    public float velocityOfShotsX;
    public float velocityOfShotsY;
    private float counter;
    private float xpos;
    private float ypos;
    public GameObject fireball;
    private GameObject newFireBall;
    private Rigidbody2D speedFireBall;

    // Use this for initialization
    void Start()
    {
        counter = 0;
        xpos = this.gameObject.transform.position.x;
        ypos = this.gameObject.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if (counter >= timeBetweenShots)
        {
            newFireBall = Instantiate(fireball, new Vector2(xpos, ypos), Quaternion.identity);
            speedFireBall = newFireBall.GetComponent<Rigidbody2D>();
            speedFireBall.velocity = new Vector2(velocityOfShotsX, velocityOfShotsY);
            counter = 0;
        }
    }
}
