using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    public static MusicPlayer Instance;
    public AudioSource audioSource;

    void Awake() {

        if (Instance != null) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
        audioSource = GetComponent<AudioSource>();
        
    }


}