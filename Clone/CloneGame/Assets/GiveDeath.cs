using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets._2D
{
    public class GiveDeath : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
            }
        }
    }
}
