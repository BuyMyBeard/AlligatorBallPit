using UnityEngine;
using UnityEngine.SceneManagement;

static class LevelManager
{
    public static int currentLevel = 0;
    public static int unlockedLevels = PlayerPrefs.GetInt("unlockedLevels", 1);
    public const int LevelCount = 3;


    public static bool LoadNextLevel()
    {
        if (currentLevel == LevelCount)
            return false;
        currentLevel++;
        if (currentLevel > unlockedLevels)
        {
            unlockedLevels = currentLevel;
            PlayerPrefs.SetInt("unlockedLevels", unlockedLevels);
        }
        SceneManager.LoadScene("Level " +  currentLevel);
        return true;
    }

    public static void LoadLevel(int level)
    {
        if (unlockedLevels < level)
            Debug.Log("Shouldn't be able to play this level");
        else
        {
            currentLevel = level;
            SceneManager.LoadScene("Level " + level);
        }
    }
}