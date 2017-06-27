using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointControl : MonoBehaviour {

    public Sprite oFlag;
    public Sprite tFlag;
    private SpriteRenderer Renderer;
    public bool checkpointReached;

    // Use this for initialization
    void Start () {
        Renderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Touch Check
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Renderer.sprite = tFlag;
            checkpointReached = true;
        }
    }
}
