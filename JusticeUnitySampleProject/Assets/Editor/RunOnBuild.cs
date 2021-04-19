using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RunOnBuild 
{
    public static void ServerBuild()
    {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "ServerBuild", "true");
    }

    public static void ClientBuild()
    {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "ServerBuild", "false");
    }
}
