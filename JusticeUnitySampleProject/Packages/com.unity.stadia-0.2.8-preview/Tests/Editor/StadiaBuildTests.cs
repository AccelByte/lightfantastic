#if PLATFORM_STADIA_INSTALLED
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.GGP;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tests
{
    public class StadiaBuildTests
    {
        private const string scenePath = "Assets/StadiaBuildTestScene.unity";
        private readonly string[] buildFilenames = {
            "GameAssembly.so",
            "libunitystadia.so",
            "StadiaPlayer",
            "UnityPlayer.so"
        };
        private readonly string[] symbolFilenames = {
            "GameAssembly.so.debug",
            "libunitystadia.so.debug",
            "StadiaPlayer.debug",
            "UnityPlayer.so.debug"
        };
        private readonly string buildDir = Application.dataPath.Replace("/Assets", "/build");

        [Test]
        [TestCase(BuildType.Release)]
        [TestCase(BuildType.Debug)]
        public void BuildAndCopySymbolsTest(BuildType buildConfiguration)
        {
            string outputDir = MakeABuild(buildConfiguration);
            string filesDir = $"{outputDir}/layout/files";
            string symbolsDir = $"{outputDir}/layout/symbols";

            CheckBuildFiles(filesDir, symbolsDir);
        }

        [Test]
        public void MakeBuildForCertificationTest()
        {
            GGPBuildMethods.MakeBuildForCertification(false, buildDir);

            string packageDir = Path.Combine(buildDir, "package_root");
            string symbolsDir = $"{packageDir}/debug_symbols";

            CheckBuildFiles(packageDir, symbolsDir);

            Assert.IsTrue(GGPBuildMethods.CertCheckBuildDebugSymbols(packageDir));
            //Assert.IsTrue(GGPBuildMethods.CertCheckSdkVersion(packageDir));
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(scenePath);
            FileUtil.DeleteFileOrDirectory(buildDir);
        }

        private string MakeABuild(BuildType buildConfiguration)
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, scenePath);

            var outputDir = $"{buildDir}/build-{BuildTarget.Stadia.ToString().ToLower()}-{buildConfiguration.ToString().ToLower()}";

            UserBuildSettings.buildConfiguration = buildConfiguration;
            UserBuildSettings.copyDebugSymbols = true;

            var buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                locationPathName = outputDir,
                scenes = new [] {newScene.path},
                targetGroup = BuildTargetGroup.Stadia,
                target = BuildTarget.Stadia,
                options = BuildOptions.StrictMode
            });

            Assert.IsTrue(buildReport.summary.result == BuildResult.Succeeded);

            return outputDir;
        }

        private void CheckBuildFiles(string filesDir, string symbolsDir)
        {
            foreach (string buildFilename in buildFilenames)
            {
                Assert.IsTrue(File.Exists($"{filesDir}/{buildFilename}"));
            }

            foreach (string symbolFilename in symbolFilenames)
            {
                Assert.IsTrue(File.Exists($"{symbolsDir}/{symbolFilename}"));
            }
        }
    }
}
#endif
