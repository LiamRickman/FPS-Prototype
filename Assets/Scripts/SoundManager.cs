using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This sound manager is responsible for playing the sounds I have in the game. 
 * 
*/
public class SoundManager : MonoBehaviour
{
    //Audio clips are loaded by dragging them into the component
    public AudioClip rifleShot, pistolShot, shotgunShot, grapple;

    private AudioSource audioSrc;

    private void Start()
    {
        //Getting Components
        audioSrc = GetComponent<AudioSource>();
    }

    //This is called whenever a sound is needed to be played. 
    //It checks what sound is wanted to be played and then plays the appropriate sound once.
    public void PlaySound(string clip)
    {
        switch (clip)
        {
            case "RifleShot":
                audioSrc.PlayOneShot(rifleShot);
                break;
            case "PistolShot":
                audioSrc.PlayOneShot(pistolShot);
                break;
            case "ShotgunShot":
                audioSrc.PlayOneShot(shotgunShot);
                break;
            case "Grapple":
                audioSrc.PlayOneShot(grapple);
                break;
        }
    }

    //This can adjust the volume of the audio source if needed as some effects are too loud/too quiet.
    public void SetVolume(float _volume)
    {
        audioSrc.volume = _volume;
    }
}
