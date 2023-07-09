using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{

    [SerializeField] Slider sfxSlider, musicSlider;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] UnityEvent settingsChanged = new();

    void Awake()
    {
        sfxSlider.value = GameSettings.SFXVolume;
        musicSlider.value = GameSettings.MusicVolume;
        UpdateMusic();
        UpdateSFX();
    }
    void UpdateSFX() => audioMixer.SetFloat("SFXVolume", Mathf.Log10(GameSettings.SFXVolume) * 20);
    void UpdateMusic() => audioMixer.SetFloat("MusicVolume", Mathf.Log10(GameSettings.MusicVolume) * 20);

    public void SFXChanged()
    {
        GameSettings.SFXVolume = sfxSlider.value;
        UpdateSFX();
    }
    public void MusicChanged()
    {
        GameSettings.MusicVolume = musicSlider.value;
        UpdateMusic();
    }
}
