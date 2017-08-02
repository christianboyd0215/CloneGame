using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        public float m_SprintSpeed;  // Amount of maxSpeed applied to sprinting movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
        public static Vector3 respawnPoint;

        public static bool SlimeBossKilled = false;
        public static bool SpinBossKilled = false;
        public static bool WizardBossKilled = false;

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        public bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        private float slowground;
        private float fastground;
        private float ground;
        public bool Jumped = false;

        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            ground = m_MaxSpeed;
            slowground = ground * .5f;
            fastground = ground * 1.5f;
            if (respawnPoint != default(Vector3))
                transform.position = respawnPoint;
        }


        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                    m_Grounded = true;
            }
            m_Anim.SetBool("Ground", m_Grounded);

            //if (!m_Grounded && m_Rigidbody2D.velocity.y < 0)
                //Jumped = true;

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
        }


        public void Move(float move, bool crouch, bool jump, bool sprint)
        {
            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move * m_CrouchSpeed : move);
                move = (sprint ? move * m_SprintSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                m_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, m_Rigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground"))//&& !jump_cancel
            {
                // Add a vertical force to the player.
                if (!Jumped)
                {
                    Jumped = true;
                    m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 18f);
                }
                m_Anim.SetBool("Ground", false);          
            }
            if (!jump && !m_Anim.GetBool("Ground") && m_Rigidbody2D.velocity.y > 0)
            {
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            }
        }


        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        private void Respawn()
        {
            if (respawnPoint == default(Vector3))
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }

            else
            {
                DontDestroyOnLoad(this);
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
                Destroy(gameObject);
            }
        }
        public void KillBoss(string bossName)
        {
            if(bossName == "Slime")
            {
                SlimeBossKilled = true;
            }
        }
        public bool BossIsDead(string bossName)
        {
            if(bossName == "Slime")
            {
                return SlimeBossKilled;
            }
            if(bossName == "Wizard")
            {
                return WizardBossKilled;
            }
            if(bossName == "Spin")
            {
                return SpinBossKilled;
            }
            if(bossName == "Final")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Triggers when a collision is initially detected
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                m_MaxSpeed = ground;
            }
            else if (collision.gameObject.CompareTag("Fast Ground"))
            {
                m_MaxSpeed = fastground;
            }
            else if (collision.gameObject.CompareTag("Slow Ground"))
            {
                m_MaxSpeed = slowground;
            }
            if (collision.gameObject.CompareTag("Moving Ground"))
            {
                m_Rigidbody2D.transform.parent = collision.transform;
            }
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Respawn();
            }
        }
        //Triggers every frame while an object is colliding with another object
        void OnCollisionStay2D(Collision2D collision)
        {

        }
        //Triggers on exiting a collision
        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Moving Ground"))
            {
                m_Rigidbody2D.transform.parent = null;
            }
        }
        //Triggers when object enters a Trigger
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "FireBall")
            {
                Respawn();
            }

            if (other.tag == "Spike")
            {
                Respawn();
            }

            if (other.tag == "Enemy")
            {
                Respawn();
            }

            if (other.tag == "CheckPoint")
            {
                respawnPoint = other.transform.position;
            }

            if (other.tag == "Door")
            {
                if(SlimeBossKilled)
                {
                    if(SpinBossKilled)
                    {
                        if(WizardBossKilled)
                        {
                            transform.position = new Vector3(-43.44f, -53.24f, 0);
                        }
                    }
                }
                
            }
            if(other.tag == "Secondary Door")
            {
                transform.position = new Vector3(62, 157, 0);
            }
        }
    }
}

