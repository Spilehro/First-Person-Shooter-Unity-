using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Door : MonoBehaviour
{
    public Text gameOverMsg;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<GunVR>().isImmortal = true;
            gameOverMsg.enabled = true;
            Invoke("restartGame", 10.0f);
        }
    }

    private void restartGame()
    {
        gameOverMsg.enabled = false;
        SceneManager.LoadScene(0);
    }
}

