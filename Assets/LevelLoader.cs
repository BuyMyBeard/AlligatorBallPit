using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    int levelID;
    private void Awake()
    {
        levelID = int.Parse(GetComponentInChildren<TextMeshProUGUI>().text);
        if (levelID > LevelManager.LevelCount || levelID < 1)
            Debug.Log("Level doesn't exist");

        if (levelID > LevelManager.unlockedLevels)
            GetComponent<Button>().interactable = false;
    }
    public void LoadLevel() => LevelManager.LoadLevel(levelID);
}
