#if PLATFORM_STADIA_INSTALLED
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityEditor.GGP
{
    internal class GGPBuildWindowInUse : GGPBuildWindow
    {
        private readonly GUIContent application = EditorGUIUtility.TrTextContent("Application", "Stadia application name for debugging and deployment");
        private readonly GUIContent gamelet = EditorGUIUtility.TrTextContent("Instance", "Instance name for debugging and deployment");
        private readonly GUIContent buildConfiguration = EditorGUIUtility.TrTextContent("Build Configuration", "Player configuration to use for building.");
        private readonly GUIContent refresh = EditorGUIUtility.TrTextContent("Refresh", "Refresh the list of Stadia applications and instances.");
        private readonly GUIContent incrementalDeployment = EditorGUIUtility.TrTextContent("Incremental Deployment", "Only upload deltas of changes between local and instance. If the file tree has not changed significantly, this can save a significant amount of time.");
        private readonly GUIContent copyDebugSymbols = EditorGUIUtility.TrTextContent("Copy Debug Symbols", "Copy player debug symbols to the project's deployment location. Copying symbols will slightly increase build times so this setting is not recommended to use for iteration. However, it is required for submitting applications to Google.");
        private readonly GUIContent[] dataLoading = new GUIContent[1] { EditorGUIUtility.TrTextContent("Loading...") };
        private readonly GUIContent[] emptyList = new GUIContent[0];
        private readonly string[] emptyStrings = new string[0];

        private static readonly BuildType[] buildConfigurations =
        {
            BuildType.Debug,
            BuildType.Release,
            BuildType.Master,
        };

        private static readonly GUIContent[] buildConfigurationStrings =
        {
            EditorGUIUtility.TrTextContent("Debug"),
            EditorGUIUtility.TrTextContent("Release"),
            EditorGUIUtility.TrTextContent("Master"),
        };

        private GUIContent[] applications;
        private string[] applicationStrings;
        private GUIContent[] gamelets;
        private string[] gameletStrings;
        private bool loading;
        private BackgroundWorker worker;
        private string applicationExceptionMessage;
        private string gameletExceptionMessage;
        private List<string> pluginSymbolFiles;

        private ValidationStatus cachedSdkInitValidationStatus = ValidationStatus.NotChecked;
        private ValidationStatus cachedFullValidationStatus = ValidationStatus.NotChecked;

        private void ScanApplicationsAndGamelets()
        {
            worker = new BackgroundWorker();

            GGPRunner.ResetInitializationStatus();
            GGPRunner.InitializePaths();
            loading = true;

            applications = emptyList;
            gamelets = emptyList;
            applicationStrings = emptyStrings;
            gameletStrings = emptyStrings;

            // Run work of fetching the list of applications and gamelets on a background thread.
            worker.DoWork += (sender, e) =>
            {
                // Once we've verified ggp init has been run, don't check again.
                if (cachedSdkInitValidationStatus != ValidationStatus.OK)
                {
                    cachedSdkInitValidationStatus = ValidateSdkInit();
                }

                if (cachedSdkInitValidationStatus == ValidationStatus.OK)
                {
                    try
                    {
                        IEnumerable<string> detectedApplications = null;
                        detectedApplications = GGPRunner.Applications();
                        applications = detectedApplications?.Select(item => EditorGUIUtility.TrTextContent(item)).ToArray() ?? emptyList;
                        applicationStrings = detectedApplications?.ToArray() ?? emptyStrings;
                    }
                    catch (Exception ex)
                    {
                        // We hit an error running the ggp commands for fetching applications. If it was a ggp error, output was logged to the console. If it was a win32 error, we handle it here.
                        applicationExceptionMessage = $"Caught {ex.GetType()}. {ex.Message}";
                        applications = emptyList;
                        applicationStrings = emptyStrings;
                    }

                    try
                    {
                        IEnumerable<string> detectedGamelets = null;
                        detectedGamelets = GGPRunner.ReservedGamelets();
                        gamelets = detectedGamelets?.Select(item => EditorGUIUtility.TrTextContent(item)).ToArray() ?? emptyList;
                        gameletStrings = detectedGamelets?.ToArray() ?? emptyStrings;
                    }
                    catch (Exception ex)
                    {
                        // We hit an error running the ggp commands for fetching gamelets. If it was a ggp error, output was logged to the console. If it was a win32 error, we handle it here.
                        gameletExceptionMessage = $"Caught {ex.GetType()}. {ex.Message}";
                        gamelets = emptyList;
                        gameletStrings = emptyStrings;
                    }
                }
            };

            // Run repaint on main thread after work has completed. This is done to make sure the build window repaints if it's visible but the user is not interacting with it.
            worker.RunWorkerCompleted += (sender, e) =>
            {
                cachedFullValidationStatus = ValidateFull();

                loading = false;

                BuildPlayerWindow[] buildWindow = Resources.FindObjectsOfTypeAll<BuildPlayerWindow>();

                // It's possible the build window was closed while running the background thread. Verify the window still exists. There will only be one.
                if (buildWindow != null && buildWindow.Length > 0)
                {
                    // Repaint the build window once the background work is done to show the results if the user is not actively using the window.
                    buildWindow[0].Repaint();
                }
            };
            worker.RunWorkerAsync();
        }

        public override bool GetBuildWindowExtensionBool(string key)
        {
            switch (key)
            {
                case "EnabledBuildAndRunButton":
                    return cachedSdkInitValidationStatus == ValidationStatus.OK && !loading;
                default:
                    return true;
            }
        }

        public override void ShowPlatformBuildOptions()
        {
            if (worker == null)
            {
                ScanApplicationsAndGamelets();
            }

            GUIContent[] displayedApplications = null;
            string[] displayedApplicationStrings = null;

            GUIContent[] displayedGamelets = null;
            string[] displayedGameletStrings = null;

            // Check loaded once here to avoid any threading issues.
            if (!loading)
            {
                displayedApplications = applications;
                displayedApplicationStrings = applicationStrings;

                displayedGamelets = gamelets;
                displayedGameletStrings = gameletStrings;
            }
            else
            {
                displayedApplications = dataLoading;
                displayedApplicationStrings = emptyStrings;

                displayedGamelets = dataLoading;
                displayedGameletStrings = emptyStrings;
            }

            int selectedIndex = Math.Max(0, Array.IndexOf(buildConfigurations, UserBuildSettings.buildConfiguration));
            selectedIndex = EditorGUILayout.Popup(buildConfiguration, selectedIndex, buildConfigurationStrings);
            if (buildConfigurations[selectedIndex] != UserBuildSettings.buildConfiguration)
                UserBuildSettings.buildConfiguration = buildConfigurations[selectedIndex];

            using (new EditorGUI.DisabledScope(!(displayedApplicationStrings?.Length > 0)))
            {
                selectedIndex = Math.Max(0, Array.IndexOf(displayedApplicationStrings, GGPUserBuildSettings.application));
                selectedIndex = EditorGUILayout.Popup(application, selectedIndex, displayedApplications);
                if (displayedApplicationStrings?.Length > 0)
                {
                    GGPUserBuildSettings.application = displayedApplicationStrings[selectedIndex];
                }
            }

            using (new EditorGUI.DisabledScope(!(displayedGameletStrings?.Length > 0)))
            {
                selectedIndex = Math.Max(0, Array.IndexOf(displayedGameletStrings, GGPUserBuildSettings.gamelet));
                selectedIndex = EditorGUILayout.Popup(gamelet, selectedIndex, displayedGamelets);
                if (displayedGameletStrings?.Length > 0)
                {
                    GGPUserBuildSettings.gamelet = displayedGameletStrings[selectedIndex];
                }
            }

            bool reload = false;

            using (new EditorGUI.DisabledScope(loading))
            {
                reload = GUILayout.Button(refresh);
            }

            GGPUserBuildSettings.incrementalDeployment = EditorGUILayout.Toggle(incrementalDeployment, GGPUserBuildSettings.incrementalDeployment);
            UserBuildSettings.copyDebugSymbols = EditorGUILayout.Toggle(copyDebugSymbols, UserBuildSettings.copyDebugSymbols);

            if (!AcceptableValidationValues.Contains(cachedFullValidationStatus))
            {
                EditorGUILayout.HelpBox(GetRunSettingsValidationMessage(cachedFullValidationStatus), MessageType.Error);
            }

            if (reload)
            {
                ScanApplicationsAndGamelets();
            }
        }

        public override string PrepareForBuild(BuildOptions options, BuildTarget target)
        {
            // If this is build and run, make sure cached values are set. If the user has never opened the build window for a project, they won't be.
            if ((options & BuildOptions.AutoRunPlayer) != 0 && (string.IsNullOrEmpty(GGPUserBuildSettings.application) || string.IsNullOrEmpty(GGPUserBuildSettings.gamelet)))
            {
                return "GGP projects must have an instance and application selected from the build window before using 'Build and Run'.";
            }

            return null;
        }

        public override void PostProcessBeforeInstallStreamingAssets(string PlayerPackage, string StagingArea, string StagingAreaNativePlugins)
        {
            ShowStep("Copying Unity Resources", 0);
            using(new ProfilerBlock("PostProcessGGP.CopyUnityResources"))
            {
                const string kResources = @"Data/Resources/unity default resources";
                var source = Path.Combine(PlayerPackage, kResources);
                var target = Path.Combine(StagingArea, kResources);
                FileUtil.CopyFileOrDirectory(source, target);
            }

            pluginSymbolFiles = new List<string>();
            ShowStep("Copying plugins", 0.08f);
            using(new ProfilerBlock("PostProcessGGP.CopyPlugins"))
            {
                bool shouldCopyDebugSymbols = UserBuildSettings.copyDebugSymbols;

                Directory.CreateDirectory(StagingAreaNativePlugins);

                foreach (var importer in PluginImporter.GetImporters(BuildTarget.Stadia))
                {
                    var pluginPath = importer.assetPath;
                    var pluginExtension = Path.GetExtension(pluginPath).ToLower();

                    switch (pluginExtension)
                    {
                        case ".so":
                        {
                            FileUtil.CopyFileOrDirectory(pluginPath, Path.Combine(StagingAreaNativePlugins, Path.GetFileName(pluginPath)));

                            if (shouldCopyDebugSymbols)
                            {
                                bool copyPluginSymbols = true;
                                var debugFile = Path.ChangeExtension(pluginPath, ".debug");

                                if (!File.Exists(debugFile))
                                {
                                    debugFile = Path.ChangeExtension(debugFile, ".so.debug");
                                    copyPluginSymbols = File.Exists(debugFile);
                                }

                                if (copyPluginSymbols)
                                    pluginSymbolFiles.Add(debugFile);
                            }
                        }
                            break;
                    }
                }
            }

            ShowStep("Copying streaming assets", 0.16f);
        }

        public override void PostProcessAfterRunIl2Cpp(string stagingArea, string installPath)
        {
            ShowStep("CopyToLayout", 0.64f);
            using(new ProfilerBlock("PostProcessGGP.CopyToLayout"))
            {
                var layoutPath = Path.Combine(installPath, "layout");
                var filesPath = Path.Combine(layoutPath, "files");
                var sourcePath = Path.Combine(layoutPath, "source");
                var symbolsPath = Path.Combine(layoutPath, "symbols");

                // Create layout folder and children
                CreateOrCleanDirectory(layoutPath);
                CreateOrCleanDirectory(filesPath);
                CreateOrCleanDirectory(symbolsPath);

                FileUtil.DeleteFileOrDirectory(Path.Combine(stagingArea, "Data\\Managed"));
                FileUtil.MoveFileOrDirectory(Path.Combine(stagingArea, "Data\\Native"), Path.Combine(stagingArea, "Native"));

                // Move native plugins
                foreach (var file in Directory.GetFiles(Path.Combine(stagingArea, "NativePlugins")))
                {
                    var destDir = file.EndsWith(".debug", StringComparison.OrdinalIgnoreCase) ? symbolsPath : filesPath;
                    FileUtil.MoveFileOrDirectory(file, Path.Combine(destDir, Path.GetFileName(file)));
                }

                var il2cppOutputDirectory = Path.Combine(stagingArea, "Native");

                // Move debug symbols out of il2cpp output directory
                foreach (var il2cppDebugSymbol in Directory.GetFiles(il2cppOutputDirectory, "*.debug", SearchOption.TopDirectoryOnly))
                    FileUtil.MoveFileOrDirectory(il2cppDebugSymbol, Path.Combine(symbolsPath, Path.GetFileName(il2cppDebugSymbol)));

                // Move the rest of il2cpp output directory
                foreach (var file in Directory.GetFiles(il2cppOutputDirectory))
                    File.Move(file, Path.Combine(filesPath, Path.GetFileName(file)));

                // Move Data and all children
                FileUtil.MoveFileOrDirectory(Path.Combine(stagingArea, "Data"), Path.Combine(filesPath, "Data"));

                // Copy UnityPlayer
                var playerDirectory = $"{BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Stadia, BuildOptions.None)}\\Variations\\il2cpp\\{UserBuildSettings.buildConfiguration.ToString().ToLower()}";

                FileUtil.CopyFileOrDirectory(Path.Combine(playerDirectory, "UnityPlayer.so"), Path.Combine(filesPath, "UnityPlayer.so"));

                // Copy debug symbols
                if (UserBuildSettings.copyDebugSymbols)
                {
                    foreach (var symbolsFile in Directory.GetFiles(playerDirectory, "*.debug", SearchOption.TopDirectoryOnly))
                        FileUtil.CopyFileOrDirectory(symbolsFile, Path.Combine(symbolsPath, Path.GetFileName(symbolsFile)));

                    foreach (var symbolsFile in pluginSymbolFiles)
                        FileUtil.CopyFileOrDirectory(symbolsFile, Path.Combine(symbolsPath, Path.GetFileName(symbolsFile)));
                }

                // Copy executable
                FileUtil.CopyFileOrDirectory(Path.Combine(playerDirectory, GGPUserBuildSettings.kExecutableName), Path.Combine(filesPath, GGPUserBuildSettings.kExecutableName));

                // Move il2cpp source for generated binary
                FileUtil.MoveFileOrDirectory(Path.Combine(stagingArea, @"il2cpp_source\il2cppOutput"), sourcePath);
            }
        }

        public override void Run(string installPath)
        {
            GGPApplicationLauncher.Run(installPath, UserBuildSettings.buildConfiguration.ToString().ToLower());
        }

        private enum ValidationStatus
        {
            OK,
            NotChecked,
            SdkNotInPath,
            SdkNotInitialized,
            NoApplicationsAvailable,
            NoGameletReserved,
            ErrorFetchingApplications,
            ErrorFetchingGamelets,
        }

        private static readonly ValidationStatus[] AcceptableValidationValues = { ValidationStatus.OK, ValidationStatus.NotChecked };

        private static ValidationStatus ValidateSdkInit()
        {
            if (!GGPRunner.SdkInPath)
            {
                return ValidationStatus.SdkNotInPath;
            }

            if (!GGPRunner.SdkInitialized)
            {
                return ValidationStatus.SdkNotInitialized;
            }

            return ValidationStatus.OK;
        }

        private ValidationStatus ValidateFull()
        {
            var sdkInitResult = ValidateSdkInit();
            if (sdkInitResult != ValidationStatus.OK)
            {
                return sdkInitResult;
            }

            if (applications?.Length == 0)
            {
                return ValidationStatus.NoApplicationsAvailable;
            }

            if (gamelets?.Length == 0)
            {
                return ValidationStatus.NoGameletReserved;
            }

            if (!string.IsNullOrEmpty(applicationExceptionMessage))
            {
                return ValidationStatus.ErrorFetchingApplications;
            }

            if (!string.IsNullOrEmpty(gameletExceptionMessage))
            {
                return ValidationStatus.ErrorFetchingGamelets;
            }

            return ValidationStatus.OK;
        }

        private string GetRunSettingsValidationMessage(ValidationStatus runSettingsValidation)
        {
            const string disableText = "Build and Run will be disabled.";

            switch (runSettingsValidation)
            {
                case ValidationStatus.OK:
                case ValidationStatus.NotChecked:
                    return "";
                case ValidationStatus.SdkNotInPath:
                    return $"The GGP_SDK_PATH environment variable is not detected. Verify the Stadia SDK is installed. {disableText}";
                case ValidationStatus.SdkNotInitialized:
                    return $"The Stadia SDK is not initialized. Run 'ggp init' then press 'Refresh'. {disableText}";
                case ValidationStatus.NoGameletReserved:
                    return $"No Stadia instance is reserved. Run 'ggp instance reserve' then press 'Refresh'. {disableText}";
                case ValidationStatus.NoApplicationsAvailable:
                    return $"No Stadia Applications are available. Go to the Stadia web portal and add an application. Then press 'Refresh'. {disableText}";
                case ValidationStatus.ErrorFetchingApplications:
                    return $"System running 'ggp application list': {applicationExceptionMessage}";
                case ValidationStatus.ErrorFetchingGamelets:
                    return $"System running 'ggp application list': {gameletExceptionMessage}";
                default:
                    throw new ArgumentException("Invalid buildSettingsValidation");
            }
        }

        private static void ShowStep(string message, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Building Player", message, progress))
                throw new OperationCanceledException();
        }

        private static void CreateOrCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
        }
    }
}
#endif
