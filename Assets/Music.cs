using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{

    AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic()
    {
        audioSource.Play();
    }
    public void StopMusic()
    {
        audioSource.Stop();
    }
}
