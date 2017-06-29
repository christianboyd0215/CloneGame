using UnityEngine;

public class HeldSpear : MonoBehaviour {
    
    
    public GameObject FlyingSpear;
    public GameObject FireFlyingSpear;
    public GameObject IceFlyingSpear;
    public GameObject WindFlyingSpear;
    public GameObject ShieldFlyingSpear;
    public GameObject Fire;
    public GameObject Ice;
    public GameObject Wind;
    public GameObject Shield;
    private GameObject Character;
    private GameObject Spear;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 spear_position;
    private float distance = 2.0f;
    private string status = null;
    private bool Ele = false;
    private bool Shoot;
    public bool WasShot = false;

    private void Start()
    {
        Character = GameObject.FindWithTag("Player");
        sr = gameObject.GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        Ele = Shoot = Input.GetKeyDown(KeyCode.C);
        Character = GameObject.FindWithTag("Player");
        Spear = GameObject.FindWithTag("FlyingSpear");
        Shoot = Input.GetKeyDown(KeyCode.LeftControl);
        spear_position = gameObject.transform.position;
       
        if (Ele)
        {
            if (Mathf.Abs(Fire.transform.position.x - gameObject.transform.position.x) <= distance && Mathf.Abs(Fire.transform.position.y - gameObject.transform.position.y) <= distance)
            {
                status = "Fire";
                Ele = false;
            }
            else
            {
                if (Mathf.Abs(Ice.transform.position.x - gameObject.transform.position.x) <= distance && Mathf.Abs(Ice.transform.position.y - gameObject.transform.position.y) <= distance)
                {
                    status = "Ice";
                    Ele = false;
                }
                else
                {
                    if (Mathf.Abs(Wind.transform.position.x - gameObject.transform.position.x) <= distance && Mathf.Abs(Wind.transform.position.y - gameObject.transform.position.y) <= distance)
                    {
                        status = "Wind";
                        Ele = false;
                    }
                    else
                    {
                        if (Mathf.Abs(Shield.transform.position.x - gameObject.transform.position.x) <= distance && Mathf.Abs(Shield.transform.position.y - gameObject.transform.position.y) <= distance)
                        {
                            status = "Shield";
                            Ele = false;
                        }
                        else
                        {
                            Ele = false;
                        }
                    }
                }
            }
        }
        
        if (status != null)
        {
            if (status == "Fire")
            {
                sr.color = new Color(255, 0, 0, 255);
            }
            else
            {
                if (status == "Ice")
                {
                    sr.color = new Color(0, 0, 255, 255);
                }
                else
                {
                    if (status == "Wind")
                    {
                        sr.color = new Color(0, 255, 0, 255);
                    }
                    else
                    {
                        if (status == "Shield")
                        {
                            sr.color = new Color(255, 255, 0, 255);
                        }
                    }
                }
            }
        }
        
        if (Shoot && !WasShot)
        {

            WasShot = true;
            gameObject.GetComponent<Renderer>().enabled = false;

            if (status != null)
            {
                if (status == "Fire")
                {
                    Instantiate(FireFlyingSpear, spear_position, Quaternion.identity);
                }
                else
                {
                    if (status == "Ice")
                    {
                        Instantiate(IceFlyingSpear, spear_position, Quaternion.identity);
                    }
                    else
                    {
                        if (status == "Wind")
                        {
                            Instantiate(WindFlyingSpear, spear_position, Quaternion.identity);
                        }
                        else
                        {
                            if (status == "Shield")
                            {
                                Instantiate(ShieldFlyingSpear, spear_position, Quaternion.identity);
                            }
                        }
                    }
                }
            }
            else
            {
                Instantiate(FlyingSpear, spear_position, Quaternion.identity);
            }
        }
        else
        {
            if (Shoot && WasShot && Mathf.Abs(Spear.transform.position.x - Character.transform.position.x) <= distance&& Mathf.Abs(Character.transform.position.y - gameObject.transform.position.y) <= distance) 
            {
                WasShot = false;
                gameObject.GetComponent<Renderer>().enabled = true;
            }
        }

        Shoot = false;
    }
    
}
