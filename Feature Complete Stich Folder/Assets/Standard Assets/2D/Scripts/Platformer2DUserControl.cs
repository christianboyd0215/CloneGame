using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof(PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;
        private bool m_Jump;
        private bool jump_Cancel;



        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
        }


        private void Update()
        {
            if (!m_Jump)
            {
                // Read the jump input in Update so button presses aren't missed.
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                jump_Cancel = CrossPlatformInputManager.GetButtonUp("Jump");
            }
        }


        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = (CrossPlatformInputManager.GetButton("Fire1")|| Input.GetKey(KeyCode.LeftControl));
            bool sprint = (Input.GetKey(KeyCode.LeftShift)||CrossPlatformInputManager.GetButton("Fire2"));//this is meant to be right trigger but somehow is right bumper.
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            m_Character.Move(h, crouch, m_Jump, jump_Cancel, sprint);
            m_Jump = false;
        }
    }
}
