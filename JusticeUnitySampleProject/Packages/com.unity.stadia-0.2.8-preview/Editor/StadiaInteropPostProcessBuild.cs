namespace UnityEditor.GGP
{
    using System.IO;
    using UnityEditor;
    using Callbacks;

    public static class StadiaInteropPostProcessBuild
    {
        [PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {

            if (target != BuildTarget.Stadia)
                return;

#if ENABLE_GGP_PREVIEW_APIS
            const string filesToDelete = "libunitystadia.s*";
#else
            const string filesToDelete = "libunitystadia_preview.s*";
#endif

            foreach (var fileInfo in (new DirectoryInfo(pathToBuiltProject)).GetFiles(filesToDelete, SearchOption.AllDirectories)) {
                FileUtil.DeleteFileOrDirectory(fileInfo.FullName);
            }
        }
    }
}
