using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPickupScript : MonoBehaviour {

    // Use this for initialization
    public GameObject door;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject newDoor = Instantiate(door, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;
    }

}
