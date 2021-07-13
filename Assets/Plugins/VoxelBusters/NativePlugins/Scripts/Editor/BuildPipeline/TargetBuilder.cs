using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VoxelBusters.CoreLibrary.Editor.NativePlugins.Build
{
    public class TargetBuilder
    {
        public static void Build()
        {
#if UNITY_IOS
            BuildIos();
#elif UNITY_ANDROID
            BuildAndroid();
#elif UNITY_STANDALONE_WIN
            BuildWindows();
#elif UNITY_STANDALONE_OSX
            BuildOsx();
#elif UNITY_STANDALONE_LINUX
            BuildLinux();
#elif UNITY_WEBGL
            BuildLinux();
#else
            Debug.LogError("No platform target defined!");
#endif
        }

        private static void BuildIos()
        {
            Build(BuildTargetGroup.iOS, BuildTarget.iOS, "iOS", null);
        }

        private static void BuildAndroid()
        {
            Build(BuildTargetGroup.Android, BuildTarget.Android, "Android", "apk");
        }

        private static void BuildWindows()
        {
            Build(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "Windows", "exe");
        }

        private static void BuildOsx()
        {
            Build(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, "OSX", "app");
        }

        private static void BuildLinux()
        {
            Build(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, "Linux", null);
        }

        private static void BuildWebGl()
        {
            Build(BuildTargetGroup.WebGL, BuildTarget.WebGL, "WebGL", null);
        }

        private static void BuildAllPlatforms()
        {
            BuildMobilePlatforms();
            BuildNonMobilePlatforms();
        }

        private static void BuildMobilePlatforms()
        {
            BuildIos();
            BuildAndroid();
        }

        private static void BuildNonMobilePlatforms()
        {
            BuildOsx();
            BuildWindows();
            BuildLinux();
            BuildWebGl();
        }

        private static void Build(BuildTargetGroup targetGroup, BuildTarget target, string platform, string extension)
        {
            string product      = PlayerSettings.productName;
            string targetPath   = (extension != null) ? string.Format("Builds/{0}/{1}.{2}", platform, product, extension) : string.Format("Builds/{0}/{1}", platform, product);

            //Switch to active target
            bool isSwitchSuccess = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

            if (!isSwitchSuccess)
                Debug.LogError(string.Format("Failed switching to platform : {0}", platform));

            //Build
            BuildReport report  = UnityEditor.BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, targetPath, target, BuildOptions.None);
            BuildSummary    summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(string.Format("Finished building for {0} : {1}", platform, targetPath));
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError(string.Format("Failed building for {0} : {1}", platform, targetPath));
            }
            else if (summary.result == BuildResult.Cancelled)
            {
                Debug.LogError(string.Format("Cancelled building for {0} : {1}", platform, targetPath));
            }
            else
            {
                Debug.LogError(string.Format("Failed building for {0} : {1}", platform, targetPath));
            }
        }
    }
}
