using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets._2D;

public class NewSlimeAI : MonoBehaviour {
	
	//public float maxSpeed = 6f;
	//public float jumpForce = 1000f;
	public Transform groundCheck;
	public LayerMask whatIsGround;
	public float verticalSpeed = 20;
    /// <summary>
    /// slime variables
    /// </summary>
    private GameObject player;               // Reference to the player's position.
    float counter;
    private Rigidbody2D boss_Rigidbody2D;
    Vector2 Direction;
    public float jumpheight;
    public float jumpwidth;
    public float minSize; // The slime won't split if it is under the minSize
    public float jumptime;
    private static List<GameObject> slimeList = new List<GameObject>();
    public bool bossSlime;
    private bool isFacingRight;
    public GameObject babySlime;
    [HideInInspector]
	public bool lookingRight = true;
	bool doubleJump = false;
    public GameObject wayBack;
	
	private Animator cloudanim;
	public GameObject Cloud;


	//private Rigidbody2D rb2d;
	private Animator anim;
	private bool isGrounded = false;


	// Use this for initialization
	void Start () {
		boss_Rigidbody2D = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		cloudanim = GetComponent<Animator>();

		Cloud = GameObject.Find("Cloud");
       // cloudanim = GameObject.Find("Cloud(Clone)").GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        isFacingRight = true;
        if (player.GetComponent<PlatformerCharacter2D>().BossIsDead("Slime"))
        {
            Destroy(gameObject);
        }
        if (!bossSlime)
        {
            slimeList.Add(gameObject);
        }
    }
    void Awake()
    {
        boss_Rigidbody2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cloudanim = GetComponent<Animator>();

        Cloud = GameObject.Find("Cloud");
       // cloudanim = GameObject.Find("Cloud(Clone)").GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        
    }
    
    void OnCollisionStay2D(Collision2D collision2D)
    {
        if(collision2D.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if (collision2D.gameObject.CompareTag("Slow Ground"))
        {
            isGrounded = true;
        }
    }
    void OnCollisionExit2D(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
        if (collision2D.gameObject.CompareTag("Slow Ground"))
        {
            isGrounded = false;
        }
    }
	void OnCollisionEnter2D(Collision2D collision2D) {
		
		//	cloudanim.Play("cloud");	

		
        if (collision2D.gameObject.CompareTag("FlyingSpear"))
        {
            if (bossSlime)
            {
                if (transform.localScale.y > minSize)
                {
                    if (UnityEngine.Random.Range(0, 2) < 1)
                    {
                        // split
                        GameObject clone1 = Instantiate(gameObject, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;
                        GameObject clone2 = Instantiate(babySlime, new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;

                        clone1.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                        clone2.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                    }
                    else
                    {
                        GameObject clone1 = Instantiate(babySlime, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;
                        GameObject clone2 = Instantiate(gameObject, new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;

                        clone1.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                        clone2.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                    }
                }
                else
                {
                    foreach (GameObject slime in slimeList)
                    {
                        Destroy(slime);
                    }
                    player.GetComponent<PlatformerCharacter2D>().KillBoss("Slime");
                    Instantiate(wayBack, transform.position, transform.rotation);
                    Destroy(gameObject);
                }

            }
            else {
                if (transform.localScale.y > minSize)
                {
                    // split
                    GameObject clone1 = Instantiate(babySlime, new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;
                    GameObject clone2 = Instantiate(babySlime, new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z), transform.rotation) as GameObject;

                    clone1.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);
                    clone2.transform.localScale = new Vector3(transform.localScale.x * 0.5f, transform.localScale.y * 0.5f, transform.localScale.z);

                }
            }
            Destroy(gameObject);
        }
    }


	
	// Update is called once per frame
	//void Update () {
        /*

	if (Input.GetButtonDown("Jump") && (isGrounded || !doubleJump))
		{
			rb2d.AddForce(new Vector2(0,jumpForce));

			if (!doubleJump && !isGrounded)
			{
				doubleJump = true;
				Boost = Instantiate(Resources.Load("Prefabs/Cloud"), transform.position, transform.rotation) as GameObject;
			//	cloudanim.Play("cloud");		
			}
		}


	if (Input.GetButtonDown("Vertical") && !isGrounded)
		{
			rb2d.AddForce(new Vector2(0,-jumpForce));
			Boost = Instantiate(Resources.Load("Prefabs/Cloud"), transform.position, transform.rotation) as GameObject;
			//cloudanim.Play("cloud");
		}
        */
	//}


	void FixedUpdate()
	{


		//float hor = Input.GetAxis ("Horizontal");

		//anim.SetFloat ("Speed", Mathf.Abs (hor));

		//rb2d.velocity = new Vector2 (hor * maxSpeed, rb2d.velocity.y);
		  
		//isGrounded = Physics2D.OverlapCircle (groundCheck.position, 0.15F, whatIsGround);

		anim.SetBool ("IsGrounded", isGrounded);

		//if ((hor > 0 && !lookingRight)||(hor < 0 && lookingRight))
		//	Flip ();
		 
		anim.SetFloat ("vSpeed", GetComponent<Rigidbody2D>().velocity.y);

        /////////////////////////////////////////////////
        // get direction
        if (player.transform.position.x - transform.position.x > 0)
        {
            Direction = new Vector2(jumpwidth, jumpheight);
            lookingRight = true;
        }
        else
        {
            Direction = new Vector2(-jumpwidth, jumpheight);
            lookingRight = false;
        }
        // chase
        if(lookingRight && !isFacingRight)
        {
            Flip();
            isFacingRight = true;
        }
        else if(!lookingRight && isFacingRight )
            {
            Flip();
            isFacingRight = false;
        }
        if(isGrounded)
        {
            counter += Time.deltaTime;
        }
        if (counter >= jumptime && Math.Abs(player.transform.position.x - boss_Rigidbody2D.position.x) < 100)
        {
            boss_Rigidbody2D.AddForce(Direction);
            //Boost = Instantiate(Resources.Load("Prefabs/Cloud"), transform.position, transform.rotation) as GameObject;
            //cloudanim.Play("cloud");
            counter = 0;
            gameObject.GetComponent<AudioSource>().Play();
        }
    }


	
	public void Flip()
	{
		//lookingRight = !lookingRight;
		Vector3 myScale = transform.localScale;
		myScale.x *= -1;
		transform.localScale = myScale;
	}
    
}
