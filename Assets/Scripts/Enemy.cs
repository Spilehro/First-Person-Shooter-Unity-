using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    public GameObject player;
    public GameObject[] targets;

    public GameObject bulletHole;
    public GameObject muzzlePrefab;
    public GameObject head;
    public GameObject rifle, rifleStart, rifleEnd;
    public GameObject shotSound;

    public float fireInaccuracy = 0.1f;
    public float fireRateDelay = 1.0f;
    public float fovDegrees = 60.0f;
    public float minPlayerDistance = 10.0f;
    public float rotationSpeed = 1.0f;

    private Animator animator;

    private int currentTargetIndex;
    private float health;
    private bool isDead;
    private bool isPlayerDetected;
    private float fireRateTimer;
    private float rotationSpeedAlert;

    // Start is called before the first frame update.
    void Start()
    {
        animator = GetComponent<Animator>();

        currentTargetIndex = 0;
        health = 100;

        isDead = false;
        isPlayerDetected = false;

        fireRateTimer = 0.0f;
        rotationSpeedAlert = rotationSpeed * 5.0f;
    }

    // Update is called once per frame.
    void Update()
    {
        if (!isDead)
        {
            rotateToTarget();
            updateState();
        }
    }

    public void GetShot(float damageValue)
    {
        health -= damageValue;

        if (health <= 0)
        {
            isDead = true;
            GetComponent<CharacterController>().enabled = false;

            rifle.transform.parent = null;
            rifle.GetComponent<BoxCollider>().enabled = true;
            rifle.GetComponent<Rigidbody>().isKinematic = false;

            animator.SetTrigger("Die");
        }
    }

    private void addRifleFireEffects()
    {
        Destroy(Instantiate(shotSound, transform.position, transform.rotation), 2.0f);

        GameObject tempMuzzle = Instantiate(muzzlePrefab, rifleEnd.transform.position, rifleEnd.transform.rotation);
        tempMuzzle.GetComponent<ParticleSystem>().Play();
        Destroy(tempMuzzle, 2.0f);
    }

    private void attackPlayer()
    {
        fireRateTimer += Time.deltaTime;

        if (fireRateTimer >= fireRateDelay && !isDead)
        {
            animator.SetTrigger("AttackPlayer");
            fireRateTimer = 0.0f;

            addRifleFireEffects();
            fireRifle();
        }
    }
    private void fireRifle()
    {
        RaycastHit rayHit;
        Vector3 rifleEndPos = rifleEnd.transform.position
            + rifleEnd.transform.up * Random.Range(-fireInaccuracy, fireInaccuracy)
            + rifleEnd.transform.right * Random.Range(-fireInaccuracy, fireInaccuracy);

        if (Physics.Raycast(rifleEnd.transform.position, (rifleEndPos - rifleStart.transform.position), out rayHit, 100.0f))
        {
            if (rayHit.transform.tag == "Player")
            {
                float damageValue = 20.0f;
                player.GetComponent<GunVR>().GetShot(damageValue);
            }
            else
            {
                Destroy(Instantiate(bulletHole, (rayHit.point + rayHit.transform.up * 0.01f), rayHit.transform.rotation), 10.0f);
            }
        }
    }

    private void followPlayer()
    {
        animator.SetTrigger("FollowPlayer");
    }

    private void rotateToTarget()
    {
        if (Vector3.Distance(transform.position, targets[currentTargetIndex].transform.position) < 2.0f)
        {
            currentTargetIndex++;
            currentTargetIndex %= targets.Length;
        }

        GameObject currentTarget = isPlayerDetected && !player.GetComponent<GunVR>().isDead ? player : targets[currentTargetIndex];

        Vector3 targetPosition = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        float targetRotationSpeed = isPlayerDetected ? rotationSpeedAlert : rotationSpeed;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, targetRotationSpeed * Time.deltaTime);
    }

    private void updateState()
    {
        Vector3 eyeView = head.transform.forward;
        Vector3 playerView = player.GetComponent<GunVR>().spine.transform.position - head.transform.position;
        float viewAngleDifference = Vector3.Angle(eyeView, playerView);

        RaycastHit viewHit;
        string targetTag = "";

        //Debug.DrawRay(head.transform.position, playerView, Color.red, 0.1f);
        //Debug.DrawRay(head.transform.position, eyeView, Color.blue, 0.1f);

        if (Physics.Raycast(head.transform.position, playerView, out viewHit, 50.0f))
        {
            targetTag = viewHit.transform.tag;
        }
        isPlayerDetected = (viewAngleDifference < fovDegrees / 2) && targetTag == "Player" || health < 100.0f;

        if (player.GetComponent<GunVR>().isDead)
        {
            animator.SetTrigger("PatrolArea");
        }
        else if (isPlayerDetected)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < minPlayerDistance)
            {
                attackPlayer();
            }
            else
            {
                followPlayer();
            }
        }
    }
}
