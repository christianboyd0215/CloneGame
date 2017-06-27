using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearBackBar : MonoBehaviour {
    private GameObject theSpear;
    bool click = false;
    
	void FixedUpdate () {
        
        click = Input.GetKey(KeyCode.X);
        if (click)
        {
            if (gameObject.transform.localScale.x < 1)
            {
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x + 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            }
        }
        else
        {   if (gameObject.transform.localScale.x >=-0.01)
            {
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - 0.01f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            }
        }
        click = false;
	}


}
