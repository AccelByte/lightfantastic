// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

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
