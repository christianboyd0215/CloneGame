using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour {

    public GameObject SpinBoss;
    // Use this for initialization
    void Start () {
        //GetComponentInParent<BoxCollider2D>().enabled = false;
        //GetComponentInParent<SpriteRenderer>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Instantiate(SpinBoss, new Vector3(transform.position.x + 10f, transform.position.y + 3f, transform.position.z), transform.rotation);
            transform.parent.GetComponent<BoxCollider2D>().enabled = true;
            transform.parent.GetComponent<SpriteRenderer>().enabled = true;
            Destroy(gameObject);
        }
    }
}
