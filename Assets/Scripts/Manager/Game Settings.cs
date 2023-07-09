using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    static private float musicVolume, sfxVolume;
    static public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }
    static public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = value;
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }
    static GameSettings()
    {
        LoadPlayerPrefs();
    }
    static void LoadPlayerPrefs()
    {
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        LoadPlayerPrefs();
    }
}
