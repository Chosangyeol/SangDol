using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public static class WebSubmissionCases
{
    private static readonly Type Source = typeof(VideoPlayManager).Assembly.GetType("WebVideoSource");
    private static object Call(string method, params object[] args) => Source.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

    public static void BuildCopyKeepsUrlAndOriginalClip()
    {
        var clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/08. Sound/Jester60Intro.mp4");
        string path = "Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var player = root.AddComponent<VideoPlayer>();
            player.clip = clip;
            player.playOnAwake = true;
            typeof(WebSubmissionBuild).GetMethod("PrepareRoot", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { root, true });
            Assert.IsNull(player.clip);
            Assert.AreEqual(VideoSource.Url, player.source);
            Assert.IsFalse(player.playOnAwake);
            var bridge = new SerializedObject(root.GetComponent<WebVideoPlayerSource>());
            Assert.IsTrue(bridge.FindProperty("autoplay").boolValue);
            Assert.IsNotEmpty(bridge.FindProperty("source").FindPropertyRelative("relativePath").stringValue);
            var holder = new SerializedObject(root.GetComponent<DungeonCompletionReturnToTitle>());
            Assert.IsNull(holder.FindProperty("endingVideo").objectReferenceValue);
            Assert.IsNotEmpty(holder.FindProperty("endingVideoWeb").FindPropertyRelative("relativePath").stringValue);
            var original = new SerializedObject(AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<DungeonCompletionReturnToTitle>());
            Assert.IsNotNull(original.FindProperty("endingVideo").objectReferenceValue, "Authoring asset must retain its original clip.");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    public static void RemovingClipClearsGeneratedPath()
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab");
        try
        {
            var holder = new SerializedObject(root.GetComponent<DungeonCompletionReturnToTitle>());
            holder.FindProperty("endingVideo").objectReferenceValue = null;
            holder.ApplyModifiedPropertiesWithoutUndo();
            typeof(WebSubmissionBuild).GetMethod("PrepareRoot", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { root, false });
            holder.Update();
            Assert.IsEmpty(holder.FindProperty("endingVideoWeb").FindPropertyRelative("relativePath").stringValue);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    public static void UrlEscapesSegmentsAndRejectsTraversal()
    {
        Assert.AreEqual("https://example.org/game/StreamingAssets/Videos/a%20b%23%ED%95%9C.mp4",
            Call("BuildUrl", "https://example.org/game/StreamingAssets/", "Videos/a b#한.mp4"));
        foreach (var path in new[] { "../video.mp4", "Videos/../../video.mp4", "/video.mp4" })
        {
            var error = Assert.Throws<TargetInvocationException>(() => Call("BuildUrl", "https://example.org/StreamingAssets", path));
            Assert.IsInstanceOf<ArgumentException>(error.InnerException);
        }
    }

    public static void UrlSourceWorksWithoutImportedClip()
    {
        var root = new GameObject("WebVideoSourceTest");
        try
        {
            var player = root.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            var source = Activator.CreateInstance(Source);
            Source.GetField("relativePath").SetValue(source, "Videos/test.mp4");
            Source.GetField("length").SetValue(source, 43.5d);
            Assert.IsTrue((bool)Call("HasVideo", null, source));
            object[] args = { player, null, source, 0d };
            Assert.IsTrue((bool)Call("Configure", args));
            Assert.AreEqual(VideoSource.Url, player.source);
            Assert.IsNull(player.clip);
            StringAssert.EndsWith("Videos/test.mp4", player.url.Replace('\\', '/'));
            Assert.AreEqual(43.5d, args[3]);
            Assert.IsFalse((bool)Call("HasVideo", null, null));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    public static void PreparedMetadataMatchesOriginalAndFile()
    {
        string[] prefabs = {
            "Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab",
            "Assets/03. Prefab/Dungeon/DungeonEnd_ReturnToTitle.prefab",
            "Assets/03. Prefab/Enemy/Boss/D1/Final/Jester.prefab"
        };
        int checkedSources = 0;
        foreach (var path in prefabs)
        foreach (var component in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component == null) continue;
            var so = new SerializedObject(component);
            var p = so.GetIterator();
            while (p.Next(true))
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference || !(p.objectReferenceValue is VideoClip clip)) continue;
                var web = so.FindProperty(p.propertyPath + "Web");
                if (web == null) continue;
                string original = AssetDatabase.GetAssetPath(clip);
                string relative = web.FindPropertyRelative("relativePath").stringValue;
                StringAssert.Contains(AssetDatabase.AssetPathToGUID(original), relative);
                string copied = Path.Combine(Application.streamingAssetsPath, relative);
                Assert.IsTrue(File.Exists(copied), copied);
                CollectionAssert.AreEqual(File.ReadAllBytes(original), File.ReadAllBytes(copied));
                Assert.AreEqual(clip.length, web.FindPropertyRelative("length").doubleValue, .001d);
                checkedSources++;
            }
        }
        Assert.GreaterOrEqual(checkedSources, 3);
    }
}
