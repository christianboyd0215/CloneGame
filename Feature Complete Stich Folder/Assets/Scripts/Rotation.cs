using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

    public float rSpeed;
    private Vector3 rDirection;
	// Use this for initialization
	void Start () {
        rDirection.z = rSpeed;
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(rDirection);
	}
}
