using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script controls the shotgun and its launch mechanic.
 * It uses a raycast and adjusts the players rigidbody to launch the player the desired amount.
*/

public class Shotgun : MonoBehaviour
{
    //Declaring Variables
    [Header("Weapon Values")]
    [SerializeField] int ammo = 2;
    private int currentAmmo;
    [SerializeField] float damage = 10f;
    [SerializeField] int projectiles = 6;
    [SerializeField] float range = 15f;
    [SerializeField] float hitForce = 50f;
    [SerializeField] float launchForce = 15f;
    [SerializeField] float firerate = 0.25f;
    private float currentFirerate;
    private bool isReloading = false;

    [Header("References")]
    [SerializeField] Text ammoCount;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject bulletImpact;
    private PlayerController player;
    private Rigidbody playerRB;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    private SoundManager soundManager;

    private void Start()
    {
        //Get Components
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRB = player.GetComponent<Rigidbody>();
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

        //Set Default Ammo
        currentAmmo = ammo;
    }

    //General Updates
    private void Update()
    {
        UpdateUI();
        GetInputs();

        //Counts down the firerate
        if (currentFirerate > 0)
            currentFirerate -= Time.deltaTime;
    }

    private void GetInputs()
    {
        //Shoots the gun only if it has ammo, isnt reloading and the firerate has reset.
        if (Input.GetMouseButtonDown(0) && currentFirerate <= 0 && currentAmmo > 0 && !isReloading)
        {
            //Resets firerate
            currentFirerate = firerate;

            //Lowers ammo count
            currentAmmo -= 1;

            LaunchPlayer();
            Shoot();
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo < ammo || Input.GetMouseButtonDown(0) && currentAmmo == 0 && !isReloading)
            Reload();
    }

    private void LaunchPlayer()
    {
        //Ensures the player doesnt get launched if they are not grounded
        if (!player.GetIsGrounded())
        {
            //Calculates the launch direction and multiplies it by the launch force.
            Vector3 launchDirection = cam.transform.forward * launchForce;

            //playerRB.velocity = -launchDirection;

            //Resets player velocity and move direction values so the shotgun can launch correctly.
            playerRB.velocity = Vector3.zero;
            player.SetMoveDirection(Vector3.zero);

            //Adds force to the players rigidbody instantly.
            playerRB.AddForce(-launchDirection, ForceMode.Impulse);

            //Sets max air speed for the players in-air movement.
            player.SetMaxAirSpeed(15f);
        }
    }

    private void Shoot()
    {
        //Creates a muzzle flash
        muzzleFlash.Play();

        //Sound Effect
        soundManager.PlaySound("ShotgunShot");

        //Creates an array of raycast hits
        RaycastHit[] hitArray = new RaycastHit[projectiles];

        //Loops through the hit array
        for (int i = 0; i < hitArray.Length; i++)
        {
            //Weapon Spread Values to make a shotgun effect
            Vector3 minPos = new Vector3(-0.2f, -0.2f, 0);
            Vector3 maxPos = new Vector3(0.2f, 0.2f, 0);

            //Creates a random vector position within these positions above.
            Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), 0);

            //Checks if the raycast hit an object in the weapons range
            if (Physics.Raycast(cam.transform.position, cam.transform.forward + randomPos, out hitArray[i], range))
            {
                //Creates an impact effect at each hit position
                Instantiate(bulletImpact, hitArray[i].point, Quaternion.LookRotation(hitArray[i].normal));

                //Gets the gameobject the raycast hit.
                GameObject objectHit = hitArray[i].transform.gameObject;

                //If the object hit has a rigidbody and isnt an enemy, it will be launched. 
                if (objectHit.GetComponent<Rigidbody>() != null && !objectHit.gameObject.CompareTag("Enemy"))
                {
                    Rigidbody objectRB = objectHit.GetComponent<Rigidbody>();
                    objectRB.AddForce(cam.transform.forward * hitForce, ForceMode.Impulse);
                }

                //Damages Enemies if Hit
                if (objectHit.CompareTag("Enemy"))
                {
                    Enemy enemy = objectHit.GetComponent<Enemy>();
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    //Called when the weapon starts reloading.
    private void Reload()
    {
        //Updates the animation to start animating.
        animator.SetBool("Reloading", true);

        //Local variable to keep track if the weapon is being reloaded or not.
        isReloading = true;
    }

    //This is called when the animation has finished via an animation event.
    //This will reset the reload boolean and ammo count, allowing the gun to be fired again.
    public void Reloaded()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        currentAmmo = ammo;
    }

    //This keeps the ammo counter up to date on the UI.
    private void UpdateUI()
    {
        ammoCount.text = (currentAmmo + " / " + ammo).ToString();
    }
}
