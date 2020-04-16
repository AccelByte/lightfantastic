// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using ABRuntimeLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLineArgs : MonoBehaviour
{

    // Start is called before the first frame update
    public bool ParseCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("ARG " + i + ": " + args[i]);
            if (args[i] == "-steamLaunch")
            {
                return true;
            }
        }
        return false;
    }
}
