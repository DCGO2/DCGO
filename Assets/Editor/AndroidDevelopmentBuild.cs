using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Builds an Android APK for landscape sideload testing.
/// Prefer the Playable APK menu first — Development+debugger often OOMs IL2CPP on this large project.
/// Batchmode: -executeMethod AndroidDevelopmentBuild.BuildPlayableApk
/// </summary>
public static class AndroidDevelopmentBuild
{
    const string DefaultOutputDir = "Builds/Android";
    const string PlayableApkName = "DCGO-android.apk";
    const string DevApkName = "DCGO-dev.apk";

    [MenuItem("DCGO/Build/Android Playable APK (recommended)")]
    public static void BuildPlayableApkMenu()
    {
        string message = BuildApkInternal(PlayableApkName, development: false);
        ShowResult(message, PlayableApkName);
    }

    [MenuItem("DCGO/Build/Android Development APK (high memory)")]
    public static void BuildDevelopmentApkMenu()
    {
        string message = BuildApkInternal(DevApkName, development: true);
        ShowResult(message, DevApkName);
    }

    /// <summary>Batchmode entry (playable / lower memory).</summary>
    public static void BuildPlayableApk()
    {
        ExitFromBatch(BuildApkInternal(PlayableApkName, development: false));
    }

    /// <summary>Legacy batchmode entry name.</summary>
    public static void BuildDevelopmentApk()
    {
        ExitFromBatch(BuildApkInternal(DevApkName, development: true));
    }

    static void ShowResult(string message, string apkName)
    {
        if (string.IsNullOrEmpty(message))
            EditorUtility.DisplayDialog("Android Build", $"APK written to {Path.Combine(DefaultOutputDir, apkName)}", "OK");
        else
            EditorUtility.DisplayDialog("Android Build Failed", message, "OK");
    }

    static void ExitFromBatch(string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }

    static string BuildApkInternal(string apkName, bool development)
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            return "Android build support is not installed for this Unity editor. Install Android Build Support in Unity Hub for 2021.3.45f2.";

        string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultOutputDir));
        Directory.CreateDirectory(outputDir);
        string apkPath = Path.Combine(outputDir, apkName);

        // ARM64 only: modern phones + less IL2CPP/native link memory than fat ARMv7+ARM64.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.DCGO.DCGO");
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);

        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
            return "No enabled scenes in Build Settings.";

        BuildOptions buildOptions = BuildOptions.CompressWithLz4HC;
        if (development)
        {
            // Development enables IL2CPP debugger support and uses much more RAM during codegen.
            buildOptions |= BuildOptions.Development;
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        Debug.Log($"[AndroidDevelopmentBuild] Starting {(development ? "Development" : "Playable")} APK build → {apkPath}");
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            return "Build failed: " + report.summary.result + " (" + report.summary.totalErrors +
                   " errors).\n\nIf Console shows OutOfMemoryException during IL2CPP:\n" +
                   "1) Use DCGO/Build/Android Playable APK (recommended)\n" +
                   "2) Close browsers/other apps\n" +
                   "3) Restart Unity and retry";
        }

        Debug.Log($"[AndroidDevelopmentBuild] Success: {apkPath} ({report.summary.totalSize} bytes)");
        return null;
    }

    static string[] GetEnabledScenes()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                list.Add(scene.path);
        }
        return list.ToArray();
    }
}
