using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour {

    float stoppos;
    public GameObject Spear;
    GameObject Camera;

	// Use this for initialization
	void Start () {
        stoppos = transform.position.y + 15;
        Camera = GameObject.FindGameObjectWithTag("MainCamera");
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (transform.position.y < stoppos)
        {
            transform.Translate(0, 0.08f, 0);
            if (transform.position.y > stoppos - 8)
                Camera.transform.Translate(0, 0.08f, 0);
        }

        else
        {
            Instantiate(Spear, new Vector3(transform.position.x, transform.position.y + 4, transform.position.z), Quaternion.Euler(0, 0, 90));
            GetComponent<FinalFight>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            Destroy(this);
        }
    }
}
