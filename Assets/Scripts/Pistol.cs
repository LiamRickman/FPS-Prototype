using UnityEngine;
using UnityEngine.UI;

/*
 * This script is used for the semi-automatic pistol. 
 * It uses a raycast system to check what is in range and whether to deal damage.
 * This makes the weapon a "hitscan" weapon.
*/

public class Pistol : MonoBehaviour
{
    //Declaring Variables
    [Header("Weapon Values")]
    [SerializeField] int ammoCount = 15;
    private int currentAmmoCount;
    [SerializeField] float damage = 10f;
    [SerializeField] float range = 50f;
    [SerializeField] float hitForce = 0f;
    [SerializeField] float fireRate = 0.15f;
    private float currentFireRate;
    private bool isReloading = false;

    [Header("Game Objects/References")]
    [SerializeField] Text ammoText;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impact;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    private SoundManager soundManager;

    private void Start()
    {
        //Getting Components
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>(); 
        
        //Setting Default Ammo Count
        currentAmmoCount = ammoCount;
    }

    //Resets weapon after being swapped to
    private void OnEnable()
    {
        //Resets reloading values so the gun can fire. Caused a bug if player reloaded and swapped weapons they were unable to fire the weapon.
        isReloading = false;
        animator.SetBool("Reloading", false);
    }

    private void Update()
    {
        UpdateUI();

        //Counts down firerate
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;

        //Tries to shoot the weapon
        if (Input.GetMouseButtonDown(0) && currentFireRate <= 0 && currentAmmoCount > 0 && !isReloading)
        {
            //Resets Firerate
            currentFireRate = fireRate;

            //Lowers Ammo Count
            currentAmmoCount -= 1;

            //Calls Shoot Method
            Shoot();
        }

        //Checks Conditions for Reload and calls reload method if met.
        else if (Input.GetKeyDown(KeyCode.R) && currentAmmoCount < ammoCount || Input.GetMouseButtonDown(0) && currentAmmoCount == 0 && !isReloading)
            Reload();
    }

    private void Shoot()
    {
        //Plays muzzleflash effect and shot sound effect.
        muzzleFlash.Play();
        soundManager.PlaySound("PistolShot");
        
        //Casts a ray in front of the camera and checks if it has hit anything.
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
        {
            //Creates an impact particle effect at the hitpoint.
            Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));

            //Sets the object hit as a gameobject.
            GameObject objectHit = hit.transform.gameObject;

            //If the gameobject hits something with a rigidbody that isn't the enemies, it applies a force and moves the rigidbody.
            //Could be used in combination with a ragdoll effect to launch something.
            if (objectHit.GetComponent<Rigidbody>() != null && !objectHit.CompareTag("Enemy"))
            {
                Rigidbody objectRB = objectHit.GetComponent<Rigidbody>();
                objectRB.AddForce(cam.transform.forward * hitForce, ForceMode.Impulse);
            }

            //If the ray hits an enemy, it will deal damage to the enemy by calling the enemies take damage method.
            if (objectHit.CompareTag("Enemy"))
            {
                Enemy enemy = objectHit.GetComponent<Enemy>();
                enemy.TakeDamage(damage);
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
        currentAmmoCount = ammoCount;
    }

    //This keeps the ammo counter up to date on the UI.
    private void UpdateUI()
    {
        ammoText.text = (currentAmmoCount + " / " + ammoCount).ToString();
    }
}
