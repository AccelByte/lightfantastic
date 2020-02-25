﻿using System;
using UnityEngine;

[Serializable]
public struct MainHUD_Hint_Info
{
    public GameObject gameObject;
    public LightFantasticConfig.Platform platform;
}

public class MainHUD_Hint : MonoBehaviour
{
    [SerializeField] private MainHUD_Hint_Info[] hintInfos;

    private void Awake()
    {
        // Disable all first and then enable the hint for the correct platform
        foreach (var hint in hintInfos) { hint.gameObject.SetActive(false); }
        foreach (var hint in hintInfos)
        {
            if (hint.platform == LightFantasticConfig.GetPlatform())
            {
                hint.gameObject.SetActive(true);
            }
        }
    }
}
