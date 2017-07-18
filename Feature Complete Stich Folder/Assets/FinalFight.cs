using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalFight : MonoBehaviour {

    GameObject player;
    public GameObject spear;
    float lowerrange = 5f;
    float upperrange = 8f;
    float counter;
    float period;
    bool spearRain = false;
    float gapTime = 0.8f;
    int numOfSpear = 30;
    float chargeTime;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        period = Random.Range(lowerrange, upperrange);
    }

    void ShootSpear()
    {
        Instantiate(spear, new Vector3(player.transform.position.x, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 90));
        Instantiate(spear, new Vector3(player.transform.position.x - 1f, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 90));
        Instantiate(spear, new Vector3(player.transform.position.x + 1f, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 90));
    }

    void GiveUSomeChance()
    {

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
            numOfSpear = 30;
        }

        if (spearRain)
        {
            chargeTime += Time.deltaTime;
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
        {
            if (counter > 0.8)
            {
                ShootSpear();
                counter = 0;
            }
        }
    }
}
