using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Jump : MonoBehaviour {
    private bool IsJump;

	void Update () {
        IsJump = CrossPlatformInputManager.GetButtonDown("Jump");
        if (IsJump)
        { gameObject.GetComponent<AudioSource>().Play(); }
        IsJump = false;
    }
}
