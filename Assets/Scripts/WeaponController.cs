using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script controls which weapon the player has equipped via a state machine.
*/

public class WeaponController : MonoBehaviour
{
    //Declaring Variables
    private enum State
    {
        Rifle,
        Pistol,
        Shotgun,
        Holster
    }

    [Header("Weapons")]
    [SerializeField] private State state;
    [SerializeField] Rifle rifle;
    [SerializeField] Pistol pistol;
    [SerializeField] Shotgun shotgun;

    [Header("UI")]
    [SerializeField] Text ammoCountText;

    private void Update()
    {
        GetInputs();
        StateController();
    }

    private void GetInputs()
    {
        //Updating Weapon States
        if (Input.GetKeyDown(KeyCode.Alpha1))
            state = State.Rifle;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            state = State.Pistol;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            state = State.Shotgun;
        else if (Input.GetKeyDown(KeyCode.H))
            state = State.Holster;
    }

    private void StateController()
    {
        //Checks which state is active and enabled/disables the correct weapon for the state.
        switch (state)
        {
            case State.Rifle:
                rifle.gameObject.SetActive(true);
                pistol.gameObject.SetActive(false);
                shotgun.gameObject.SetActive(false);
                ammoCountText.enabled = true;
                break;
            case State.Pistol:
                rifle.gameObject.SetActive(false);
                pistol.gameObject.SetActive(true);
                shotgun.gameObject.SetActive(false);
                ammoCountText.enabled = true;
                break;
            case State.Shotgun:
                rifle.gameObject.SetActive(false);
                pistol.gameObject.SetActive(false);
                shotgun.gameObject.SetActive(true);
                ammoCountText.enabled = true;
                break;
            case State.Holster:
                rifle.gameObject.SetActive(false);
                pistol.gameObject.SetActive(false);
                shotgun.gameObject.SetActive(false);
                ammoCountText.enabled = false;
                break;
        }
    }
}

