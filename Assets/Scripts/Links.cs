using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Links : MonoBehaviour
{
    private TMP_Text textMeshPro;
    private List<string> generalLinks = new List<string>
    {
        "https://discussions.unity.com/t/how-to-freeze-and-unfreeze-my-game/311091",
        "https://www.youtube.com/watch?v=HL9Swf45lZ4&ab_channel=DaveCarrigg",
        "https://discussions.unity.com/t/how-do-i-mute-all-audio-sound/16685",
    };
    private List<string> soundsLinks = new List<string>
    {
        "https://pixabay.com/sound-effects/",
    };

    void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
        SetText();
    }

    void Update()
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
        if (Input.GetMouseButtonDown(0) && linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            int linkNumber;
            if (linkID.StartsWith("general") && int.TryParse(linkID.Substring(7), out linkNumber) && linkNumber >= 0 && linkNumber < generalLinks.Count)
            {
                Application.OpenURL(generalLinks[linkNumber]);
            }
            else if (linkID.StartsWith("sound") && int.TryParse(linkID.Substring(5), out linkNumber) && linkNumber >= 0 && linkNumber < soundsLinks.Count)
            {
                Application.OpenURL(soundsLinks[linkNumber]);
            }
        }
    }

    void SetText()
    {
        string text = "General:\n";
        for (int i = 0; i < generalLinks.Count; i++)
        {
            text += $"<link=\"general{i}\"><color=#FFFFFF><u>Link {i + 1}</u></color></link>\n"; // Default color is white and underlined
        }

        text += "\nSounds:\n";
        for (int i = 0; i < soundsLinks.Count; i++)
        {
            text += $"<link=\"sound{i}\"><color=#FFFFFF><u>Link {i + 1}</u></color></link>\n"; // Default color is white and underlined
        }

        textMeshPro.text = text;
    }
}