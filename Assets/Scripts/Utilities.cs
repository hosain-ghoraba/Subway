using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utilities : MonoBehaviour
{
    private const string MainMenuSceneName = "ScreensScene";
    private const string GameSceneName = "GameScene";

    public void StartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(GameSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(MainMenuSceneName);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

