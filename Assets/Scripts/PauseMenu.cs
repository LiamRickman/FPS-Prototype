using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * I created a simple pause menu script so players can move between the practice course and the open area. 
 * They can also quit the game here, instead of forcing the game to close. 
*/

public class PauseMenu : MonoBehaviour
{
    //Declaring Variables
    [SerializeField] GameObject ui;
    private PlayerController player;
    public bool paused = false;

    private void Start()
    {
        //Resetting timescale
        Time.timeScale = 1f;

        //Getting Components
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //Hiding UI by default
        ui.SetActive(false);
    }

    private void Update()
    {
        //Checks if the user is trying to activate or deactivate the pause menu.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    //Checks if the menu is active or not, and sets it to the opposite state.
    private void ToggleMenu()
    {
        if (ui.activeSelf)
        {
            Time.timeScale = 1f;
            paused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ui.SetActive(false);

        }
        else
        {
            Time.timeScale = 0f;
            paused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ui.SetActive(true);
        }
    }

    //Button Functions
    public void PressPractice()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadScene("Practice Course");
    }

    public void PressOpenArea()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadScene("Open Area");
    }

    public void PressQuit()
    {
        Application.Quit();
    }
}
