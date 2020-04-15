using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using UnityEngine;
using UnityEngine.UI;

public class GameVersionLogic : MonoBehaviour
{
    [SerializeField] private Text gameVersion;
    [SerializeField] private Text sdkVersion;
    [SerializeField] private Text environment;
    
    void Start()
    {
        MainThreadTaskRunner.Instance.Run(delegate
        {
            gameVersion.text = "Game Version: " + LightFantasticConfig.GAME_VERSION;
            sdkVersion.text = "SDK Version: " + LightFantasticConfig.SDK_VERSION;
            environment.text = "Environment: " + AccelBytePlugin.Config.BaseUrl;
        });
    }
}
