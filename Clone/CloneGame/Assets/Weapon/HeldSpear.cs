using UnityEngine;

public class HeldSpear : MonoBehaviour {
    
    
    public GameObject FlyingSpear;
    private GameObject Character;
    private GameObject Spear;
    private Rigidbody2D rb;
    private Vector2 spear_position;
    private bool Shoot;
    private bool WasShot = false;

    void Update()
    {
        Character = GameObject.FindWithTag("Player");
        Spear = GameObject.FindWithTag("FlyingSpear");
        Shoot = Input.GetKeyDown(KeyCode.LeftControl);
        spear_position = gameObject.transform.position;
        
        if (Shoot && !WasShot)
        {

            WasShot = true;
            gameObject.GetComponent<Renderer>().enabled = false;
            Instantiate(FlyingSpear, spear_position, Quaternion.identity);

        }
        else
        {
            if (Shoot && WasShot && Mathf.Abs(Spear.transform.position.x - Character.transform.position.x) <= 1) 
            {
                WasShot = false;
                gameObject.GetComponent<Renderer>().enabled = true;
            }
        }

        Shoot = false;
    }
    
}
