using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// Keeps original VideoClip references in authoring assets and adds portable URL metadata.
public sealed class WebSubmissionBuild : IPreprocessBuildWithReport, IProcessSceneWithReport
{
    public int callbackOrder => 100;

    [MenuItem("Tools/Web Submission/Build WebGL")]
    public static void BuildWebGL()
    {
        string resultPath = "Temp/WebSubmissionBuildResult.txt";
        File.WriteAllText(resultPath, "BUILDING " + DateTime.Now.ToString("O"));
        try
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                target = BuildTarget.WebGL,
                locationPathName = "Build/WebSubmission",
                options = BuildOptions.None
            });
            var messages = report.steps.SelectMany(s => s.messages)
                .Where(m => m.type == LogType.Error || m.type == LogType.Exception || m.type == LogType.Warning)
                .Select(m => m.type + ": " + m.content);
            File.WriteAllText(resultPath, "Result=" + report.summary.result + "; Errors=" + report.summary.totalErrors +
                "; Warnings=" + report.summary.totalWarnings + "; Bytes=" + report.summary.totalSize +
                "; Duration=" + report.summary.totalTime + "\n" + string.Join("\n", messages));
        }
        catch (Exception error)
        {
            File.WriteAllText(resultPath, "EXCEPTION " + error);
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/Web Submission/Prepare Videos and Screen")]
    public static void Prepare()
    {
        if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play Mode and wait for compilation before preparing the submission.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save open scenes before preparing the web submission.");

        int changed = 0;
        var active = SceneManager.GetActiveScene();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
            // Video references use this class-specific file ID in text-serialized assets.
            if (!File.ReadAllText(path).Contains("32900000")) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool dirty = PrepareRoot(root, false);
                if (path == "Assets/03. Prefab/Manager/UIManager.prefab") dirty |= PrepareScreenCanvases(root);
                if (dirty) { PrefabUtility.SaveAsPrefabAsset(root, path); changed++; }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        foreach (var entry in EditorBuildSettings.scenes.Where(s => s.enabled))
        {
            var scene = SceneManager.GetSceneByPath(entry.path);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Additive);
            try
            {
                bool dirty = false;
                foreach (var root in scene.GetRootGameObjects())
                {
                    dirty |= PrepareRoot(root, false);
                    if (entry.path == "Assets/01. Scene/Title.unity") dirty |= PrepareScreenCanvases(root);
                }
                if (dirty) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); changed++; }
            }
            finally { if (openedHere) EditorSceneManager.CloseScene(scene, true); }
        }
        if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 600;
        PlayerSettings.WebGL.template = "PROJECT:Submission";
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WebSubmission] Prepared URL metadata and 16:9 screen layout. Updated assets: " + changed);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL) Prepare();
    }

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null || report.summary.platform != BuildTarget.WebGL) return;
        foreach (var root in scene.GetRootGameObjects()) PrepareRoot(root, true);
    }

    private static bool PrepareScreenCanvases(GameObject root)
    {
        bool changed = false;
        foreach (var scaler in root.GetComponentsInChildren<CanvasScaler>(true))
        {
            var canvas = scaler.GetComponent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) continue;
            if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize && scaler.referenceResolution == new Vector2(1920, 1080)) continue;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            EditorUtility.SetDirty(scaler);
            changed = true;
        }
        return changed;
    }

    private static bool PrepareRoot(GameObject root, bool buildCopy)
    {
        bool changed = false;
        foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component == null) continue;
            var so = new SerializedObject(component);
            var iterator = so.GetIterator();
            var paths = new List<string>();
            while (iterator.Next(true))
                if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                    (iterator.objectReferenceValue is VideoClip || iterator.type.Contains("VideoClip")))
                    paths.Add(iterator.propertyPath);
            foreach (var path in paths)
            {
                var clipProperty = so.FindProperty(path);
                var companion = so.FindProperty(path + "Web");
                if (companion == null) continue;
                var clip = (VideoClip)clipProperty.objectReferenceValue;
                if (clip == null)
                {
                    if (!buildCopy && !string.IsNullOrEmpty(companion.FindPropertyRelative("relativePath").stringValue))
                    {
                        companion.FindPropertyRelative("relativePath").stringValue = string.Empty;
                        companion.FindPropertyRelative("length").doubleValue = 0d;
                        companion.FindPropertyRelative("audioTracks").intValue = 0;
                    }
                    continue;
                }
                changed |= WriteSource(companion, clip);
                if (buildCopy) clipProperty.objectReferenceValue = null;
            }
            if (so.hasModifiedProperties) { so.ApplyModifiedPropertiesWithoutUndo(); changed = true; }
        }
        foreach (var player in root.GetComponentsInChildren<VideoPlayer>(true))
        {
            var bridge = player.GetComponent<WebVideoPlayerSource>();
            if (player.clip == null)
            {
                if (!buildCopy && bridge != null)
                {
                    var stale = new SerializedObject(bridge);
                    stale.FindProperty("source").FindPropertyRelative("relativePath").stringValue = string.Empty;
                    stale.FindProperty("source").FindPropertyRelative("length").doubleValue = 0d;
                    stale.FindProperty("source").FindPropertyRelative("audioTracks").intValue = 0;
                    stale.FindProperty("autoplay").boolValue = false;
                    if (stale.hasModifiedProperties) { stale.ApplyModifiedPropertiesWithoutUndo(); changed = true; }
                }
                continue;
            }
            if (bridge == null) { bridge = player.gameObject.AddComponent<WebVideoPlayerSource>(); changed = true; }
            var so = new SerializedObject(bridge);
            changed |= WriteSource(so.FindProperty("source"), player.clip);
            var autoplay = so.FindProperty("autoplay");
            if (autoplay.boolValue != player.playOnAwake) { autoplay.boolValue = player.playOnAwake; changed = true; }
            if (so.hasModifiedProperties) so.ApplyModifiedPropertiesWithoutUndo();
            if (buildCopy)
            {
                player.clip = null;
                player.source = VideoSource.Url;
                player.playOnAwake = false;
            }
        }
        return changed;
    }

    private static bool WriteSource(SerializedProperty source, VideoClip clip)
    {
        string original = AssetDatabase.GetAssetPath(clip);
        string relative = "Videos/" + AssetDatabase.AssetPathToGUID(original) + Path.GetExtension(original).ToLowerInvariant();
        string destination = Path.Combine(Application.streamingAssetsPath, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        if (!File.Exists(destination) || !FilesEqual(original, destination)) File.Copy(original, destination, true);
        var path = source.FindPropertyRelative("relativePath");
        var length = source.FindPropertyRelative("length");
        var tracks = source.FindPropertyRelative("audioTracks");
        bool changed = path.stringValue != relative || Math.Abs(length.doubleValue - clip.length) > .001d || tracks.intValue != clip.audioTrackCount;
        if (changed) { path.stringValue = relative; length.doubleValue = clip.length; tracks.intValue = clip.audioTrackCount; }
        return changed;
    }

    private static bool FilesEqual(string a, string b)
    {
        if (new FileInfo(a).Length != new FileInfo(b).Length) return false;
        using (var hash = System.Security.Cryptography.SHA256.Create())
        using (var first = File.OpenRead(a))
        using (var second = File.OpenRead(b)) return hash.ComputeHash(first).SequenceEqual(hash.ComputeHash(second));
    }
}
