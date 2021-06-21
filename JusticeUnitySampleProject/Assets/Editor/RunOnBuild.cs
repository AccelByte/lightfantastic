using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class RunOnBuild 
{
    public static void ServerBuild()
    {
        DevelopmentBuild();
    }

    private static string[] EnabledLevels()
    {
        return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
    }

    private static String FindLinuxPathArg ()
    {
        var args = Environment.GetCommandLineArgs();
        if(!args.Contains("-linuxBuildPath"))
        {
            throw new Exception("no linux path was supplied to build server");
        }
        for (int i = 0; i < args.Length; i++)
        {
            if(args[i] == "-linuxBuildPath" && i < args.Length - 1)
            {
                return args[i + 1];
            }
        }
        throw new Exception("linux -linuxBuildPath was supplied but no argument was passed");
    }
    public static void DevelopmentBuild()
    {
        BuildPipeline.BuildPlayer(EnabledLevels(), FindLinuxPathArg(), BuildTarget.StandaloneLinux64, BuildOptions.Development | BuildOptions.EnableHeadlessMode );
    }


}
