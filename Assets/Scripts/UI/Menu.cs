using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    bool pauseMenuBlocked = false;
    public void BlockPauseMenu() => pauseMenuBlocked = true;
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    void Resume()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
    void Pause()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    public void Quit()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        if (pauseMenuBlocked)
            return;
        if (Time.timeScale == 1)
            Pause();
        else
            Resume();
    }
    public void NextLevel()
    {
        LevelManager.LoadNextLevel();
        Time.timeScale = 1;
    }

}
