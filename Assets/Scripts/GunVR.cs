using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GunVR : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    public GameObject bulletHole;
    public GameObject muzzlePrefab;
    public GameObject shotSound;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;

    public float health = 100;
    public float maxHealth = 200;
    public bool isDead = false;
    public bool isImmortal = false;

    public Text gameOverMsg;
    public Text playerHealth;
    public Text magBullets;
    public Text remainingBullets;

    int magBulletsVal = 30;
    int maxBulletsVal = 90;
    int remainingBulletsVal = 90;
    int magSize = 30;

    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    // Use this for initialization
    void Start()
    {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update()
    {
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }

        OVRInput.Update();
        
        if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKey(KeyCode.Space))
            && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
        { 
            shotDetection(); // Should be completed
            addRifleFireEffects(); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.3f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal--;

            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }

        if ((OVRInput.GetDown(OVRInput.Button.Back) || OVRInput.Get(OVRInput.Button.Back) || OVRInput.GetDown(OVRInput.RawButton.Back) || OVRInput.Get(OVRInput.RawButton.Back) || Input.GetKey(KeyCode.R))
            && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead)
        {
            animator.SetBool("reload", true);
            gunReloadTime = 2.5f;
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }

        updateText();
    }
  

    public void GetShot(float damageValue) // getting hit from enemy
    {
        if (!isImmortal)
        {
            health -= damageValue;
        }

        if (health <= 0)
        {
            gameOverMsg.enabled = true;
            isDead = true;
            GetComponent<CharacterMovement>().isDead = true;
            GetComponent<CharacterController>().enabled = false;

            headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            animator.SetBool("dead", true);

            Invoke("restartGame", 10.0f);
        }
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (eventNumber == 1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else if (eventNumber == 2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    public void RestoreAmmo()
    {
        remainingBulletsVal = maxBulletsVal;
    }

    public void RestoreHealth()
    {
        health = maxHealth;
    }

    private void addRifleFireEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        Destroy(Instantiate(shotSound, transform.position, transform.rotation), 2.0f);

        GameObject tempMuzzle = Instantiate(muzzlePrefab, end.transform.position, end.transform.rotation);
        tempMuzzle.GetComponent<ParticleSystem>().Play();
        Destroy(tempMuzzle, 2.0f);
    }

    private void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    private void restartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void shotDetection() // Detecting the object which player shot 
    {
        RaycastHit rayHit;

        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position), out rayHit, 100.0f))
        {
            if (rayHit.transform.tag == "enemy")
            {
                float damageValue = 20.0f;
                rayHit.transform.GetComponent<Enemy>().GetShot(damageValue);
            }
            else
            {
                Destroy(Instantiate(bulletHole, (rayHit.point + rayHit.transform.up * 0.01f), rayHit.transform.rotation), 10.0f);
            }
        }
    }

    private void updateText()
    {
        playerHealth.text = isImmortal ? "--" : health.ToString() + " HP";
        magBullets.text = magBulletsVal.ToString();
        remainingBullets.text = remainingBulletsVal.ToString();
    }
}
