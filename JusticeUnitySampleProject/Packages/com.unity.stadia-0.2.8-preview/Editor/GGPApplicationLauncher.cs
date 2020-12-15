using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace UnityEditor.GGP
{
    public static class GGPApplicationLauncher
    {
        public static void Run(string installPath, string buildConfiguration)
        {
            // msbuild doesn't play nicely with solutions in network paths.
            if (installPath.StartsWith("\\\\") || installPath.StartsWith("//"))
            {
                throw new ArgumentException("Can not use Build and Run with a network share.", "installPath");
            }

            var launcher = new ApplicationLauncherImpl(
                Path.GetFullPath(installPath.Replace("/", @"\")),
                buildConfiguration);

            launcher.Deploy();

            if (!GGPUserBuildSettings.deployOnly)
            {
                EditorUtility.DisplayProgressBar("Launching Player", "Running", 0.9f);
                launcher.Run();
            }
        }
    }

    internal class ApplicationLauncherImpl
    {
        private readonly string installPath;
        private readonly string configuration;

        private const string gameletBaseDir = "/mnt/developer";
        private string layoutBaseDir => Path.Combine(installPath, "layout");
        private string layoutFileDir => Path.Combine(layoutBaseDir, "files");
        private string layoutManifestDir => Path.Combine(layoutBaseDir, "manifests");

        private readonly string remoteHashFileFullPath;
        private readonly string gameletFileListFullPath;

        // The original thought was to calculate the maximum allowable size for the arguments dynamically. But given that it will be uncommon for
        // a command to actually grow past a few thousand characters, we're going to save cycles and put a lower cap on the allowable argument size
        // that is well below the possible ggp command size.
        private const int kMaxWinCommandLength = 8191;
        private const int kMaxCompoundGgpArgLength = kMaxWinCommandLength - 1000;

        private Dictionary<string, string> localHashes;
        private List<string> gameletHashes;
        private HashSet<string> gameletFolders;
        private List<string> foldersToAdd;
        private Dictionary<string, List<string>> filesByPath;
        private float uploadSize;

        public ApplicationLauncherImpl(string installPath, string configuration)
        {
            this.installPath = installPath;
            this.configuration = configuration;

            remoteHashFileFullPath = FileUtil.GetUniqueTempPathInProject();
            gameletFileListFullPath = FileUtil.GetUniqueTempPathInProject();
        }

        public void Deploy()
        {
            using(new ProfilerBlockSPS("Deploy project to instance"))
            {
                var instanceName = GGPUserBuildSettings.gamelet;
                EnumerateFilesInLayout();

                if (!GGPUserBuildSettings.incrementalDeployment)
                {
                    CopyAllFilesToInstance(instanceName);
                    return;
                }

                EditorUtility.DisplayCancelableProgressBar("Deploying Player", "Getting existing layout from instance", 0.7f);
                var hashFileInfo = HashGameletAssets(instanceName);

                // Upload everything if we don't have full, non-zero hash information for both the local and the remote gamelet.
                if (hashFileInfo == null || hashFileInfo.Length == 0)
                {
                    // If either hash file is completely missing, log a warning. Otherwise the remote hash file has zero size, which means no files are on the remote yet. Not a problem.
                    if (hashFileInfo == null)
                    {
                        Debug.LogWarning("The asset hash file from the instance does not exist. Check that your instance is available with 'ggp instance list'.");
                    }

                    CopyAllFilesToInstance(instanceName);
                    return;
                }

                CopyAllChangedFilesToInstance(instanceName);
            }
        }

        private void CopyAllFilesToInstance(string instanceName)
        {
            using(new ProfilerBlockSPS("Copy all files to instance"))
            {
                using (var processResult = GGPRunner.PlaceFileOrFolderAsync(instanceName, Path.Combine(layoutFileDir, "*"), true))
                {
                    processResult.RunProcessAsyncWithProgressBarAndGetOutputOrThrow("Deploying Player", "Copying all files to instance", 0.7f);
                }

                // Set executable bit on executable via ggp ssh -- chmod 711 /mnt/developer/<exeName>
                GGPRunner.ChangePermissions(instanceName, $"/mnt/developer/{GGPUserBuildSettings.kExecutableName}", "711");
            }
        }

        private void CopyAllChangedFilesToInstance(string instanceName)
        {
            EditorUtility.DisplayProgressBar("Deploying Player", "Determining changed assets", 0.7f);
            HashLocalAssets();
            EnumerateGameletHashes();
            RemoveUnchangedAssetsFromLocalHashes();

            // If anything remains in the local dictionary then we have files that are either new or have changed. Upload them.
            if (localHashes.Count > 0)
            {
                const int kMebibyte = 1048576;
                string copyInfo = $"Copying {uploadSize / kMebibyte:0.0} MB of changed assets to instance";
                Console.WriteLine("Deploy: " + copyInfo);
                EditorUtility.DisplayProgressBar("Deploying Player", copyInfo, 0.7f);
                AddNewFoldersToGamelet();
                UploadChangedAssets(instanceName, copyInfo);
            }
            else
            {
                Debug.Log("No files have changed since the last deployment. No upload to the instance will occur.");
            }

            // Set executable bit on executable via ggp ssh -- chmod 711 /mnt/developer/<exeName>
            GGPRunner.ChangePermissions(instanceName, $"/mnt/developer/{GGPUserBuildSettings.kExecutableName}", "711");
        }

        public void Run()
        {
            // Build 'ggp run' command here and execute. Don't pop chrome if the the Stadia "headless" setting is on.
            GGPRunner.RunGamelet(GGPUserBuildSettings.application, GGPUserBuildSettings.gamelet, GGPUserBuildSettings.kExecutableName, GGPUserBuildSettings.headless);
        }

        private void EnumerateFilesInLayout()
        {
            using(new ProfilerBlockSPS("Enumerate files in layout"))
            {
                // Create a list of all files in the layout
                using (var fileList = new StreamWriter(File.OpenWrite(gameletFileListFullPath)))
                {
                    fileList.NewLine = "\n";

                    foreach (string file in Directory.GetFiles(layoutFileDir, "*", SearchOption.AllDirectories))
                    {
                        fileList.WriteLine(file.Replace(layoutFileDir + @"\", "").Replace(@"\", "/"));
                    }
                }
            }
        }

        private FileInfo HashGameletAssets(string instanceName)
        {
            using(new ProfilerBlockSPS("Calculate MD5 hashes on instance"))
            {
                // Place the 'gamelet_list' manifest file on the remote gamelet. Then run a script on the gamelet to collect the hashes of existing files. Pull the resulting file from the remote for later comparison.
                GGPRunner.PlaceFileOrFolder(instanceName, gameletFileListFullPath, "/mnt/developer/gamelet_files");
                GGPRunner.ExecuteCommand($"ssh --instance \"{instanceName}\" shell", "\"while read line; do if [ -f \\\"/mnt/developer/$line\\\" ]; then md5sum \\\"/mnt/developer/$line\\\"; fi; done < /mnt/developer/gamelet_files > /mnt/developer/gamelet_md5\"", false);
                GGPRunner.ExecuteCommand($"ssh --instance \"{instanceName}\" get", $"/mnt/developer/gamelet_md5 \"{remoteHashFileFullPath}\"", false);
            }

            FileInfo info = null;

            if (File.Exists(remoteHashFileFullPath))
            {
                info = new FileInfo(remoteHashFileFullPath);
            }

            return info;
        }

        private void HashLocalAssets()
        {
            localHashes = new Dictionary<string, string>();

            using(new ProfilerBlockSPS("Calculate MD5 hashes on local layout"))
            {
                var hasher = MD5.Create();

                foreach (var content in Directory.GetFiles(layoutFileDir, "*", SearchOption.AllDirectories))
                {
                    using (var stream = File.OpenRead(content))
                    {
                        // Key of the local and gamelet hash dictionaries is the MD5 hash + the relative path of the file.
                        localHashes[BitConverter.ToString(hasher.ComputeHash(stream)).Replace("-", "").ToLower() + content.Replace(layoutFileDir, gameletBaseDir).Replace("\\", "/")] = content;
                    }
                }
            }
        }

        private void EnumerateGameletHashes()
        {
            gameletHashes = new List<string>();
            gameletFolders = new HashSet<string>();

            // Populate a dictionary containing the hash information for the remote gamelet. Key = relative path (unix) Value = hash
            using (var gameletHashStream = new StreamReader(File.OpenRead(remoteHashFileFullPath)))
            {
                string line = string.Empty;
                while ((line = gameletHashStream.ReadLine()) != null)
                {
                    var tokens = line.Split(new char[] { ' ' }, 2);
                    var filename = tokens[1].Trim();
                    gameletHashes.Add(tokens[0] + filename);
                    gameletFolders.Add(filename.Substring(0, filename.LastIndexOf('/')));
                }
            }
        }

        private void RemoveUnchangedAssetsFromLocalHashes()
        {
            using(new ProfilerBlockSPS("Determine changed files for upload to instance"))
            {
                // Go through the dictionary for the remote gamelet. If it has hash values that match an entry in the local dictionary, there is a file that hasn't changed.
                // Remove the entry from the local dictionary so we don't upload the file later.
                foreach (var key in gameletHashes)
                {
                    if (localHashes.ContainsKey(key))
                    {
                        localHashes.Remove(key);
                    }
                }

                foldersToAdd = new List<string>();
                filesByPath = new Dictionary<string, List<string>>();

                var changed = new List<string>();
                foreach (var value in localHashes.Values)
                {
                    changed.Add(value);
                }

                Console.WriteLine("Deploy: The following files have diverged from the instance version and will be uploaded again:\n" + string.Join("\n", changed.ToArray()));

                foreach (var pair in localHashes)
                {
                    string folder = Path.Combine(gameletBaseDir, Path.GetDirectoryName(pair.Value.Replace(layoutFileDir + "\\", ""))).Replace("\\", "/");
                    if (!filesByPath.ContainsKey(folder))
                    {
                        filesByPath[folder] = new List<string>();
                    }

                    filesByPath[folder].Add(pair.Value);
                    uploadSize += new FileInfo(pair.Value).Length;
                }
            }
        }

        private void AddNewFoldersToGamelet()
        {
            using(new ProfilerBlockSPS("Create folders on instance"))
            {
                string leafFolder = string.Empty;

                foreach (var pair in filesByPath.OrderByDescending(k => k.Key))
                {
                    if (!gameletFolders.Contains(pair.Key) && !leafFolder.StartsWith(pair.Key) && leafFolder != gameletBaseDir)
                    {
                        leafFolder = pair.Key;
                        foldersToAdd.Add(leafFolder);
                    }
                }

                int currentArgLength = 0;
                string currentArgs = string.Empty;

                if (foldersToAdd.Count > 0)
                {
                    Action mkdirAction = () =>
                    {
                        GGPRunner.ExecuteCommand("ssh", $"shell --instance \"{GGPUserBuildSettings.gamelet}\" -- mkdir -p{currentArgs}", false);
                    };

                    foreach (var folder in foldersToAdd)
                    {
                        if (currentArgLength + folder.Length > kMaxCompoundGgpArgLength)
                        {
                            mkdirAction();
                            currentArgLength = 0;
                            currentArgs = string.Empty;
                        }

                        currentArgLength += folder.Length;
                        currentArgs += $" '{folder}'";
                    }

                    if (currentArgLength > 0)
                    {
                        mkdirAction();
                    }
                }
            }
        }

        private void UploadChangedAssets(string instanceName, string copyInfo)
        {
            using(new ProfilerBlockSPS("Upload changed files to instance"))
            {
                int currentArgLength = 0;
                var currentFiles = new List<string>();

                string destinationFolder = string.Empty;

                Action placeAction = () =>
                {
                    using (var processResult = GGPRunner.PlaceFilesAsync(instanceName, currentFiles, destinationFolder))
                    {
                        processResult.RunProcessAsyncWithProgressBarAndGetOutputOrThrow("Deploying Player", copyInfo, 0.7f);
                    }
                };

                foreach (var pair in filesByPath)
                {
                    destinationFolder = $"'{pair.Key}'";

                    currentArgLength = 0;
                    currentFiles.Clear();

                    foreach (var file in pair.Value)
                    {
                        if (currentArgLength + file.Length > kMaxCompoundGgpArgLength)
                        {
                            placeAction();
                            currentArgLength = 0;
                            currentFiles.Clear();
                        }

                        currentArgLength += file.Length;
                        currentFiles.Add(file);
                    }

                    if (currentArgLength > 0)
                    {
                        placeAction();
                    }
                }
            }
        }
    }

    internal struct ProfilerBlockSPS : IDisposable
    {
        public ProfilerBlockSPS(string name)
        {
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
