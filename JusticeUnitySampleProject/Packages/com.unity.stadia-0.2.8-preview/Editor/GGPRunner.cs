using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;

namespace UnityEditor.GGP
{
    public static class GGPRunner
    {
        private const int kDefaultTimeoutMs = 30000;

        private static string ggpPath;
        private static string ggpExe;
        private static bool? sdkInitialized;

        public static void InitializePaths()
        {
            ggpPath = Environment.GetEnvironmentVariable("GGP_SDK_PATH");

            if (!string.IsNullOrEmpty(ggpPath))
                ggpExe = Path.Combine(ggpPath, @"dev\bin\ggp.exe");
        }

        public static bool SdkInPath => !string.IsNullOrEmpty(ggpPath);

        public static bool SdkInitialized
        {
            get
            {
                if (sdkInitialized != null)
                {
                    return (bool)sdkInitialized;
                }

                sdkInitialized = false;

                // According to Google, an initialized GGP SDK must be associated with an organization.
                IEnumerable<object> result = ExecuteCommand("organization", "list", true, kDefaultTimeoutMs) as IEnumerable<object>;
                if (result?.Count() > 0)
                {
                    sdkInitialized = true;
                }

                return (bool)sdkInitialized;
            }
        }

        public static void ResetInitializationStatus()
        {
            sdkInitialized = null;
        }

        public static IEnumerable<string> Applications()
        {
            IEnumerable<object> json = ExecuteCommand("application", "list", true, kDefaultTimeoutMs) as IEnumerable<object>;
            return json?.Select(result => ((JObject) result)["displayName"]?.ToString()).ToList();
        }

        public static IEnumerable<string> ReservedGamelets()
        {
            IEnumerable<object> json = ExecuteCommand("instance", "list", true, kDefaultTimeoutMs) as IEnumerable<object>;
            return json?.Select(result => ((JObject) result)["displayName"]?.ToString()).ToList();
        }

        public static void PlaceFileOrFolder(string instanceName, string file, string destination, bool recursive = false)
        {
            ExecuteCommand("ssh", $"--instance \"{instanceName}\" put" + (recursive ? " -r " : " ") + "\"" + file + "\"" + ((destination != null) ? $" \"{destination}\"" : ""), false);
        }

        public static void ChangePermissions(string instanceName, string path, string newPermissions, bool recursive = false)
        {
            ExecuteCommand("ssh", $"--instance \"{instanceName}\" shell -- chmod " + (recursive ? "-r " : "") + newPermissions + $" '{path}'", false, kDefaultTimeoutMs);
        }

        public static object ExecuteCommand(string command, string args, bool structured, int timeoutInMs = int.MaxValue)
        {
            using (var processResult = new GGPProcess(ggpExe, command, args, structured))
            {
                return processResult.RunProcessAndGetOutputOrThrow(structured, timeoutInMs);
            }
        }

        public static GGPProcess PlaceFileOrFolderAsync(string instanceName, string file, bool recursive = false)
        {
            return PlaceFileOrFolderAsync(instanceName, file, null, recursive);
        }

        private static GGPProcess PlaceFileOrFolderAsync(string instanceName, string file, string destination, bool recursive = false)
        {
            return new GGPProcess(ggpExe, "ssh", $"--instance \"{instanceName}\" put" + (recursive ? " -r " : " ") + "\"" + file + "\"" + ((destination != null) ? $" \"{destination}\"" : ""), false);
        }

        public static GGPProcess PlaceFilesAsync(string instanceName, IEnumerable<string> files, string destination)
        {
            var commandString = $"--instance \"{instanceName}\" put {files.Select(f => $"\"{f}\"").Aggregate((x, y) => x + " " + y)} ";
            if (!string.IsNullOrEmpty(destination))
                commandString += $" \"{destination}\"";

            return new GGPProcess(ggpExe, "ssh", commandString, false);
        }

        public static void RunGamelet(string application, string gamelet, string cmd, bool headless)
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = true,
                    Arguments = $"run --application \"{application}\" --instance \"{gamelet}\" --cmd \"{cmd}\"" + (headless ? " --headless" : ""),
                    FileName = ggpExe,
                    CreateNoWindow = false
                }
            };

            string gameletCommand = $"{process.StartInfo.FileName} {process.StartInfo.Arguments}";

            if (!process.Start())
            {
                throw new Build.BuildFailedException($"Failed to launch instance with command '{gameletCommand}'");
            }

            Debug.Log($"Launching instance with command '{gameletCommand}'");
        }
    }
}
