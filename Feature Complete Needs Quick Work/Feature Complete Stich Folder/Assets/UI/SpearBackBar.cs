using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearBackBar : MonoBehaviour {
    private GameObject FlyingSpear;
    private GameObject Character;
    private float RecycleRange;
    private bool inRecycleRange = false;
    private bool done = false; 
    private bool click = false;
    void Update()
    { 
        if (!done)
        {
            done = GameObject.Find("Counter").GetComponent<SpearBackCounter>().stage3;
            FlyingSpear = GameObject.FindWithTag("FlyingSpear");
            Character = GameObject.FindWithTag("Player");
            if (FlyingSpear != null)
            {
                RecycleRange = FlyingSpear.GetComponent<FlyingSpear>().RecycleRange;
                if (Mathf.Abs(Character.transform.position.x - FlyingSpear.transform.position.x) <= RecycleRange && Mathf.Abs(Character.transform.position.y - FlyingSpear.transform.position.y) <= RecycleRange)
                {
                    inRecycleRange = true;
                }
                else
                {
                    inRecycleRange = false;
                }
            }
            else
            {
                inRecycleRange = false;
            }
        }
    }
    void FixedUpdate () {
        
        if (!done&& inRecycleRange)
        {
            click = Input.GetKey(KeyCode.X);
            if (click)
            {
                if (gameObject.transform.localScale.x < 1 && inRecycleRange)
                {
                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
                }
            }
            else
            {
                if (gameObject.transform.localScale.x >= -0.01)
                {
                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
                }
            }
            click = false;
        }
        else
        {
            if (done)
            {
                if (gameObject.transform.localScale.x > -0.01)
                {
                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (gameObject.transform.localScale.x > -0.01)
                {
                    gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
                }
            }
        }
	}


}
