using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is a very simple enemy script so I could test out the weapons damage mechanics.
 * It is used to store the enemy health and destroy the object if its health reaches 0.
 * In a full game this would be much more complex, however it worked for my prototype.
*/

public class Enemy : MonoBehaviour
{
    //Declaring Variables
    [SerializeField] float health = 50f;
    private float currentHealth;
    private PlayerController player;

    private void Start()
    {
        //Getting Components
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //Setting Default Values
        currentHealth = health;    
    }

    private void Update()
    {
        CheckHealth();
    }

    private void CheckHealth()
    {
        //If the enemies health reaches 0, the enemy is destroyed and the players kill count increases.
        if (currentHealth <= 0)
        {
            player.SetEnemiesKilled(1);
            Destroy(gameObject);
        }
    }

    //This damages the enemy and is called from the weapon scripts.
    public void TakeDamage(float _damage)
    {
        currentHealth -= _damage;
    }

}
