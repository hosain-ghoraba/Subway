using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resume : MonoBehaviour
{
    [SerializeField] GameObject pauseScreen;
    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        AudioManager.Instance.PlayGameSound();
    }
}
