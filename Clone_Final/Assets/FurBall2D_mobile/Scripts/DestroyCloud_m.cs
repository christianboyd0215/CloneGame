using UnityEngine;
using System.Collections;

public class DestroyCloud_m : MonoBehaviour {
	
	void Start(){
		Invoke("DestroyIt",0.5f);
	}

	void DestroyIt(){
		Destroy(gameObject);
	}
}