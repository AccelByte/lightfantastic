#if PLATFORM_STADIA_INSTALLED
using System.IO;

namespace UnityEditor.GGP
{
    public static class GGPBuildMethods
    {
        private const BuildTarget TargetStadia = BuildTarget.Stadia;
        private const BuildTargetGroup TargetGroupStadia = BuildTargetGroup.Stadia;

        public static void MakeBuildForCertification(bool showBuiltPlayer = true, string buildLocationPath = null)
        {
            // Save current state
            var buildConfigurationTemp = UserBuildSettings.buildConfiguration;
            var copyDebugSymbolsTemp = UserBuildSettings.copyDebugSymbols;
            var il2CppCompilerConfigurationTemp = PlayerSettings.GetIl2CppCompilerConfiguration(TargetGroupStadia);

            try
            {
                // Setup cert state
                UserBuildSettings.buildConfiguration = BuildType.Master;
                UserBuildSettings.copyDebugSymbols = true;
                PlayerSettings.SetIl2CppCompilerConfiguration(TargetGroupStadia, Il2CppCompilerConfiguration.Master);

                // Make the build
                var buildPlayerOptions = new BuildPlayerOptions();
                if (!string.IsNullOrEmpty(buildLocationPath))
                    buildPlayerOptions.locationPathName = buildLocationPath;
                else
                    buildPlayerOptions =
                        BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(buildPlayerOptions);

                if (showBuiltPlayer)
                    buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer;

                buildPlayerOptions.target = TargetStadia;
                buildPlayerOptions.targetGroup = TargetGroupStadia;
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);

                // Rearrange to Stadia package layout
                var buildPath = buildPlayerOptions.locationPathName;
                var layoutPath = Path.Combine(buildPath, "layout");
                var filesPath = Path.Combine(layoutPath, "files");
                var sourcePath = Path.Combine(layoutPath, "source");
                var symbolsPath = Path.Combine(layoutPath, "symbols");
                var debugSymbolsPath = Path.Combine(filesPath, "debug_symbols");
                var packagePath = Path.Combine(buildPath, "package_root");
                var newSourcePath = Path.Combine(buildPath, "source");

                FileUtil.CopyFileOrDirectory(symbolsPath, debugSymbolsPath);
                FileUtil.CopyFileOrDirectory(filesPath, packagePath);
                FileUtil.CopyFileOrDirectory(sourcePath, newSourcePath);
                FileUtil.DeleteFileOrDirectory(layoutPath);
            }
            finally
            {
                // Restore state
                UserBuildSettings.buildConfiguration = buildConfigurationTemp;
                UserBuildSettings.copyDebugSymbols = copyDebugSymbolsTemp;
                PlayerSettings.SetIl2CppCompilerConfiguration(TargetGroupStadia, il2CppCompilerConfigurationTemp);
            }
        }

        public static bool CertCheckBuildDebugSymbols(string packageDir)
        {
            GGPRunner.InitializePaths();
            string commandOutput = GGPRunner.ExecuteCommand("cert", $"check-debug-symbols --package-root {packageDir}", false) as string;
            return string.IsNullOrEmpty(commandOutput) || commandOutput.Contains("Passed");
        }

        public static bool CertCheckSdkVersion(string packageDir)
        {
            GGPRunner.InitializePaths();
            string versionOutput = GGPRunner.ExecuteCommand("version", string.Empty, false) as string;
            int firstDotLoc = versionOutput?.IndexOf('.') ?? 0;
            string version = versionOutput?.Substring(firstDotLoc + 1, 4);
            string commandOutput = GGPRunner.ExecuteCommand("cert", $"check-sdk-version --submission-version={version} --package-root {packageDir}", false) as string;
            return string.IsNullOrEmpty(commandOutput) || commandOutput.Contains("true");
        }
    }
}
#endif
