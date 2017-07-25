using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChange : MonoBehaviour {

    public GameObject TargetGround;
    public GameObject Character;
    private float GroundHeight;
	void Start () {
        GroundHeight = TargetGround.transform.position.y + 1.1f;
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Character.GetComponent<GroundHeight>().Ground = GroundHeight;
        
        }
    }
    void Update () {
        Character = GameObject.FindWithTag("Player");
	}
}
