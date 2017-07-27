using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenTheDamnDoor : MonoBehaviour {

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FlyingSpear"))
        {
            Destroy(gameObject);
        }
    }
}
