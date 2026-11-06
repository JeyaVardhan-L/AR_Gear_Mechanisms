using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;

namespace MechXR.Editor
{
    public static class AndroidBuildTool
    {
        private const string PackageName = "com.mechxr.gearlearning";
        private const string AppName = "MECH-XR";
        private const string ScenePath = "Assets/Scenes/MainScene.unity";
        private const string OutputApkPath = "Builds/MECH-XR.apk";
        private const string OutputApkPathFixed = "Builds/MECH-XR-FIXED.apk";
        private const string OutputApkPathPhase3 = "Builds/MECH-XR-PHASE3.apk";
        private const string OutputApkPathPhase4 = "Builds/MECH-XR-PHASE4.apk";
        private const string OutputApkPathPhase4_1 = "Builds/MECH-XR-PHASE4-1.apk";
        private const string OutputApkPathPhase5 = "Builds/MECH-XR-PHASE5.apk";
        private const string OutputApkPathPhase6 = "Builds/MECH-XR-PHASE6.apk";
        private const string OutputApkPathPhase6_1 = "Builds/MECH-XR-PHASE6-1.apk";
        private const string OutputApkPathPhase6_1_v2 = "Builds/MECH-XR-PHASE6-1-v2.apk";

        [MenuItem("MechXR/Configure Android Build Settings")]
        public static void ConfigureAndroidSettings()
        {
            Debug.Log("[MechXR] Configuring Android Build Settings for ARCore...");

            // 1. Switch Active Build Target to Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[MechXR] Switching active build target to Android...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // 2. Configure PlayerSettings
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
            PlayerSettings.productName = AppName;
            PlayerSettings.companyName = "MechXR";
            PlayerSettings.bundleVersion = "1.1";
            PlayerSettings.Android.bundleVersionCode = 2;

            // Screen Orientation: Portrait
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Android Architecture & IL2CPP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Minimum SDK: API 26 (Android 8.0 Oreo - required for ARCore)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Graphics APIs: OpenGLES3, Vulkan
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan
            });

            // 3. Configure XR Management for Android (ARCore Loader)
            try
            {
                if (!Directory.Exists("Assets/XR"))
                {
                    Directory.CreateDirectory("Assets/XR");
                }

                if (!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var buildTargetSettings) || buildTargetSettings == null)
                {
                    buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(buildTargetSettings, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
                }

                var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                if (xrSettings == null)
                {
                    xrSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
                    xrSettings.name = "Android Settings";
                    var mgr = ScriptableObject.CreateInstance<XRManagerSettings>();
                    mgr.name = "Android Providers";
                    xrSettings.Manager = mgr;
                    buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Android, xrSettings);
                    AssetDatabase.CreateAsset(xrSettings, "Assets/XR/XRGeneralSettingsAndroid.asset");
                    AssetDatabase.CreateAsset(mgr, "Assets/XR/XRManagerSettingsAndroid.asset");
                }

                if (xrSettings != null && xrSettings.AssignedSettings != null)
                {
                    bool assigned = XRPackageMetadataStore.AssignLoader(
                        xrSettings.AssignedSettings,
                        "UnityEngine.XR.ARCore.ARCoreLoader",
                        BuildTargetGroup.Android);

                    Debug.Log($"[MechXR] ARCoreLoader assigned: {assigned}");
                    EditorUtility.SetDirty(xrSettings);
                    EditorUtility.SetDirty(xrSettings.AssignedSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MechXR] XR Management configuration note: {ex.Message}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[MechXR] Android player settings configured successfully!");
        }

        [MenuItem("MechXR/Build Android APK")]
        public static void BuildAndroidApk()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Android APK Build Process");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building APK to: {OutputApkPath}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPath))
            {
                FileInfo fi = new FileInfo(OutputApkPath);
                Debug.Log($"[MechXR] [SUCCESS] APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Android APK (Fixed)")]
        public static void BuildAndroidApkFixed()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Fixed Android APK Build Process");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathFixed;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Fixed APK to: {OutputApkPathFixed}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathFixed))
            {
                FileInfo fi = new FileInfo(OutputApkPathFixed);
                Debug.Log($"[MechXR] [SUCCESS] Fixed APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Android APK (Phase 3)")]
        public static void BuildAndroidApkPhase3()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 3 Android APK Build Process");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase3;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 3 APK to: {OutputApkPathPhase3}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase3))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase3);
                Debug.Log($"[MechXR] [SUCCESS] Phase 3 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Android APK (Phase 4 Overhaul)")]
        public static void BuildAndroidApkPhase4()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 4 Android APK Production Build");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase4;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 4 APK to: {OutputApkPathPhase4}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase4))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase4);
                Debug.Log($"[MechXR] [SUCCESS] Phase 4 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Phase 4.1 Android APK")]
        public static void BuildAndroidApkPhase4_1()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 4.1 Android APK Production Build");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase4_1;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 4.1 APK to: {OutputApkPathPhase4_1}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase4_1))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase4_1);
                Debug.Log($"[MechXR] [SUCCESS] Phase 4.1 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Phase 5 Android APK")]
        public static void BuildAndroidApkPhase5()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 5 Android APK Production Build");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase5;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 5 APK to: {OutputApkPathPhase5}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase5))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase5);
                Debug.Log($"[MechXR] [SUCCESS] Phase 5 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Phase 6 Android APK")]
        public static void BuildAndroidApkPhase6()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 6 Android APK Production Build");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase6;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 6 APK to: {OutputApkPathPhase6}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase6))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase6);
                Debug.Log($"[MechXR] [SUCCESS] Phase 6 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }

        [MenuItem("MechXR/Build Phase 6.1 Android APK")]
        public static void BuildAndroidApkPhase6_1()
        {
            Debug.Log("==================================================");
            Debug.Log("[MechXR] Starting Phase 6.1 Android APK Production Build");
            Debug.Log("==================================================");

            // Step 1: Ensure Main Scene exists and is ready
            SceneSetupTool.BuildMainScene();

            // Step 2: Configure Android Settings
            ConfigureAndroidSettings();

            // Step 3: Create Output Directory
            if (!Directory.Exists("Builds"))
            {
                Directory.CreateDirectory("Builds");
            }

            // Step 4: Perform Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { ScenePath };
            buildPlayerOptions.locationPathName = OutputApkPathPhase6_1_v2;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;

            Debug.Log($"[MechXR] Building Phase 6.1 APK v2 to: {OutputApkPathPhase6_1_v2}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (File.Exists(OutputApkPathPhase6_1_v2))
            {
                File.Copy(OutputApkPathPhase6_1_v2, OutputApkPathPhase6_1, true);
                FileInfo fi2 = new FileInfo(OutputApkPathPhase6_1_v2);
                Debug.Log($"[MechXR] [SUCCESS] Phase 6.1 v2 APK Created! File Size: {(fi2.Length / (1024f * 1024f)):F2} MB");
            }

            Debug.Log("==================================================");
            Debug.Log($"[MechXR] Build Result: {report.summary.result}");
            Debug.Log($"[MechXR] Total Errors: {report.summary.totalErrors}");
            Debug.Log($"[MechXR] Total Warnings: {report.summary.totalWarnings}");
            Debug.Log($"[MechXR] Total Time: {report.summary.totalTime.TotalSeconds:F1} seconds");
            if (File.Exists(OutputApkPathPhase6_1))
            {
                FileInfo fi = new FileInfo(OutputApkPathPhase6_1);
                Debug.Log($"[MechXR] [SUCCESS] Phase 6.1 APK Created! File Size: {(fi.Length / (1024f * 1024f)):F2} MB");
            }
            Debug.Log("==================================================");
        }
    }
}
