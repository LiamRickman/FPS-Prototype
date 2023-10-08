using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script controls the doors the player passes through.
 * This is done by checking if the player is in range and then playing an animation that moves the door up.
 * It was kept simple as it is a minor mechanic I added to seperate the practice course level into different sections.
*/

public class DoorController : MonoBehaviour
{
    //Declaring Variables
    [SerializeField] GameObject door;
    private Animator animator;

    private void Start()
    {
        //Getting Components
        animator = door.GetComponent<Animator>();
    }

    //Checks if the player has collided with the collider and if they have, the door open animation will play.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            animator.SetBool("Open", true);
        }
    }

    //Checks if the player has left the door collision check and if so, will play the door closing animation.
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            animator.SetBool("Open", false);
        }
    }
}
