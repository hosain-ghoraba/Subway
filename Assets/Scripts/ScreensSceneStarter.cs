using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreensSceneStarter : MonoBehaviour
{
    [SerializeField] Toggle muteCheckMark;
    void Start()
    {
        // play pause screen sound
        AudioManager.Instance.PlayPauseScreenSound();

        muteCheckMark = muteCheckMark.GetComponent<Toggle>();

        // set toggle state from playerref
        ApplyMuteSettings(muteCheckMark);

        // add listener
        muteCheckMark.onValueChanged.AddListener(OnToggleChanged);

    }

    public void ApplyMuteSettings(Toggle muteCheckMark)
    {
        if (PlayerPrefs.HasKey("playSound"))
            muteCheckMark.isOn = PlayerPrefs.GetInt("playSound") == 1;
        AudioListener.pause = !muteCheckMark.isOn;
    }
    void OnToggleChanged(bool playSound)
    {
        PlayerPrefs.SetInt("playSound", playSound ? 1 : 0);
        PlayerPrefs.Save();
        AudioListener.pause = !playSound;
    }

}
