// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsoleHelper : MonoBehaviour
{
    
    private static DeveloperConsoleHelper instance_;
    public static DeveloperConsoleHelper Instance
    {
        get
        {
            if (instance_ == null)
                CreateGameObject();
            return instance_;
        }
    }
    
    private void Awake()
    {
        if (instance_ != null)
        {
            Destroy(gameObject);
            return;
        }
        instance_ = this;
        DontDestroyOnLoad(gameObject);
    }

    private static void CreateGameObject()
    {
        if (instance_ == null)
        {
            new GameObject("Developer Console Helper").AddComponent<DeveloperConsoleHelper>();
        }
    }
    
    public void Refresh()
    {
        if (!LightFantasticConfig.DEVELOPER_CONSOLE_VISIBLE)
        {
            MainThreadTaskRunner.Instance.Run(()=>{StartCoroutine(ClearConsoleAsync());});
        }
    }

    IEnumerator ClearConsoleAsync()
    {
        while(true)
        {
            Debug.ClearDeveloperConsole();
            Debug.developerConsoleVisible = false;
            yield return null;
        }
    }
}
