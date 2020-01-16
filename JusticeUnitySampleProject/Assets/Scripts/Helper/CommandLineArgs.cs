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
