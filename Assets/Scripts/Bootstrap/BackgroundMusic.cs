using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BackgroundMusic : MonoBehaviour
{ 
    [SerializeField] private AudioSource backgroundMusic;

    private void Awake()
    {
        if (backgroundMusic)
        {
            backgroundMusic.loop = true;
            backgroundMusic.volume = 0.2F;
        }
        else
        {
            Debug.Log("No music source");
        }
    }
}
