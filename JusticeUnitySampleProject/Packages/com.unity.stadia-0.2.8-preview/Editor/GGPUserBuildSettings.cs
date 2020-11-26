namespace UnityEditor.GGP
{
    public static class GGPUserBuildSettings
    {
        public const string kExecutableName = "StadiaPlayer";

        private const string kSettingApplication = "Application";
        private const string kSettingGamelet = "Gamelet";
        private const string kSettingHeadless = "Headless";
        private const string kSettingDeployOnly = "DeployOnly";
        private const string kSettingsIncrementalDeployment = "IncrementalDeployment";

        private static readonly string BuildTargetStadiaString = BuildTarget.Stadia.ToString();

        public static string application
        {
            get => EditorUserBuildSettings.GetPlatformSettings(BuildTargetStadiaString, kSettingApplication);
            set => EditorUserBuildSettings.SetPlatformSettings(BuildTargetStadiaString, kSettingApplication, value);
        }

        public static string gamelet
        {
            get => EditorUserBuildSettings.GetPlatformSettings(BuildTargetStadiaString, kSettingGamelet);
            set => EditorUserBuildSettings.SetPlatformSettings(BuildTargetStadiaString, kSettingGamelet, value);
        }

        public static bool headless
        {
            get => EditorUserBuildSettings.GetPlatformSettings(BuildTargetStadiaString, kSettingHeadless).ToLower() == "true";
            set => EditorUserBuildSettings.SetPlatformSettings(BuildTargetStadiaString, kSettingHeadless, value.ToString());
        }

        public static bool deployOnly
        {
            get => EditorUserBuildSettings.GetPlatformSettings(BuildTargetStadiaString, kSettingDeployOnly).ToLower() == "true";
            set => EditorUserBuildSettings.SetPlatformSettings(BuildTargetStadiaString, kSettingDeployOnly, value.ToString().ToLower());
        }

        public static bool incrementalDeployment
        {
            get
            {
                // Default is set.
                var incrementalSetting = EditorUserBuildSettings.GetPlatformSettings(BuildTargetStadiaString, kSettingsIncrementalDeployment).ToLower();
                return incrementalSetting == string.Empty || incrementalSetting == "true";
            }
            set => EditorUserBuildSettings.SetPlatformSettings(BuildTargetStadiaString, kSettingsIncrementalDeployment, value.ToString().ToLower());
        }
    }

#if !UNITY_INCLUDE_TESTS
    [InitializeOnLoad]
    internal class RecompileOnEditorStart
    {
        private const string kGGPBuildWindowFileToRecompile = "Packages/com.unity.stadia/Editor/GGPBuildWindowInUse.cs";
        private const string kGGPBuildWindowFileToRecompileKey = "kGGPBuildWindowFileToRecompileKey";

        static RecompileOnEditorStart()
        {
            EditorApplication.quitting += ClearEditorPrefOnQuit;
            bool hasBeenRecompiled = EditorPrefs.GetBool(kGGPBuildWindowFileToRecompileKey);
            if (hasBeenRecompiled)
            {
                return;
            }

            EditorApplication.delayCall += OnEditorInspectorsLoaded;

            EditorPrefs.SetBool(kGGPBuildWindowFileToRecompileKey, true);
        }

        private static void OnEditorInspectorsLoaded()
        {
            AssetDatabase.ImportAsset(kGGPBuildWindowFileToRecompile, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ClearEditorPrefOnQuit()
        {
            EditorPrefs.DeleteKey(kGGPBuildWindowFileToRecompileKey);
        }
    }
#endif
}
