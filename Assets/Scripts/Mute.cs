using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mute : MonoBehaviour
{
    [SerializeField] Toggle muteCheckMark;
    void Start()
    {
        muteCheckMark = GetComponent<Toggle>();

        // set toggle state from playerref
        ApplyMuteSettings();

        // add listener
        muteCheckMark.onValueChanged.AddListener(OnToggleChanged);

    }

    public void ApplyMuteSettings()
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
