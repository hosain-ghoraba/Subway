using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarlyLoader : MonoBehaviour
{
    [SerializeField] GameObject startScreen;
    void Start()
    {
        LoadAllImages();
        startScreen.SetActive(true);
    }

    private void LoadAllImages()
    {
        string path = "Images";
        Object[] allAssets = Resources.LoadAll(path, typeof(Texture2D));
        Debug.Log("Number of assets loaded: " + allAssets.Length);
        foreach (Object asset in allAssets)
        {
            Debug.Log("Loaded asset: " + asset.name);
        }
    }
}
