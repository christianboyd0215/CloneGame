using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalFight : MonoBehaviour {

    GameObject player;
    public GameObject box1;
    public GameObject box2;
    public GameObject box3;

    GameObject currentbox1;
    GameObject currentbox2;
    GameObject currentbox3;

    public GameObject spear;
    float lowerrange = 5f;
    float upperrange = 8f;
    float counter;
    float period;
    bool spearRain = false;
    float gapTime = 0.8f;
    int numOfSpear = 40;
    float chargeTime;
    bool box1NotThrowed = true;
    bool box2NotThrowed = true;
    bool box3NotThrowed = true;
    bool firstloop = true;
    bool whichmove;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        period = Random.Range(lowerrange, upperrange);
        whichmove = true;
    }

    void ShootSpear()
    {
        Instantiate(spear, new Vector3(player.transform.position.x, transform.position.y - 3, transform.position.z), Quaternion.Euler(0, 0, 90));
        Instantiate(spear, new Vector3(player.transform.position.x - 1f, transform.position.y - 3, transform.position.z), Quaternion.Euler(0, 0, 90));
        Instantiate(spear, new Vector3(player.transform.position.x + 1f, transform.position.y - 3, transform.position.z), Quaternion.Euler(0, 0, 90));
    }

    void GiveUSomeChance()
    {
        if (chargeTime > 1 && box1NotThrowed)
        {
            currentbox1 = Instantiate(box1, new Vector3(player.transform.position.x, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 0));
            box1NotThrowed = false;
        }

        else if (chargeTime > 1.8 && box2NotThrowed)
        {
            currentbox2 = Instantiate(box2, new Vector3(player.transform.position.x, transform.position.y + 8, transform.position.z), Quaternion.Euler(0, 0, 0));
            box2NotThrowed = false;
        }

        else if (chargeTime > 2.9 && box3NotThrowed)
        {
            currentbox3 = Instantiate(box3, new Vector3(player.transform.position.x, transform.position.y + 12, transform.position.z), Quaternion.Euler(0, 0, 0));
            box3NotThrowed = false;
        }

        else if (chargeTime > 5.3 && chargeTime < 7)
        {
            Destroy(currentbox1);
            Destroy(currentbox2);
            Destroy(currentbox3);
        }

        else if (chargeTime > 7)
        {
            period = Random.Range(lowerrange, upperrange);
            chargeTime = 0;
            box1NotThrowed = true;
            box2NotThrowed = true;
            box3NotThrowed = true;
            spearRain = false;
            whichmove = Random.Range(0, 5) > 1;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        period -= Time.deltaTime;
        counter += Time.deltaTime;

        if (period < 0)
            spearRain = true;

        if (numOfSpear < 0)
        {
            spearRain = false;
            period = Random.Range(lowerrange, upperrange);
            chargeTime = 0;
            numOfSpear = 40;
            whichmove = Random.Range(0, 5) > 2.5;
            if (firstloop)
            {
                firstloop = false;
                whichmove = false;
            }
        }

        if (spearRain)
        {
            chargeTime += Time.deltaTime;

            if (whichmove)
            {
                if (chargeTime > 1)
                {

                    if (counter > 0.2)
                    {
                        Instantiate(spear, new Vector3(player.transform.position.x, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 90));
                        counter = 0;
                        numOfSpear--;
                    }
                }
            }

            else
                GiveUSomeChance();
        }

        else
        {
            if (counter > 0.8)
            {
                ShootSpear();
                counter = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FlyingSpear"))
            Destroy(gameObject);
    }
}
