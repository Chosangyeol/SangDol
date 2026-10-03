using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public static class McpLifecycleCases
{
    public static void CleanupUsesEditorQuitAndInstallationIsIdempotent()
    {
        Action unrelated = () => { };
        Application.quitting += unrelated;
        try
        {
            Assert.IsTrue(McpEditorQuitLifecycle.Install());
            Assert.IsTrue(McpEditorQuitLifecycle.Install());
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var applicationQuit = (Delegate)typeof(Application).GetField("quitting", flags).GetValue(null);
            var callbacks = applicationQuit?.GetInvocationList() ?? new Delegate[0];
            Assert.Contains(unrelated, new List<Delegate>(callbacks), "Other quit listeners must be preserved.");
            foreach (var callback in callbacks) Assert.IsFalse(IsMcpCleanup(callback));

            // Unity 2022 tracks event subscribers in EventWithPerformanceTracker.
            // Inspect its delegate storage without invoking Editor shutdown.
            var tracker = typeof(EditorApplication).GetField("m_QuittingEvent", flags).GetValue(null);
            int cleanupCount = 0;
            var enumerator = (System.Collections.IEnumerator)tracker.GetType().GetMethod("GetEnumerator").Invoke(tracker, null);
            try
            {
                while (enumerator.MoveNext()) if (IsMcpCleanup((Delegate)enumerator.Current)) cleanupCount++;
            }
            finally { (enumerator as IDisposable)?.Dispose(); }
            Assert.AreEqual(1, cleanupCount, "Editor shutdown must retain exactly one MCP cleanup callback.");
        }
        finally { Application.quitting -= unrelated; }
    }

    private static bool IsMcpCleanup(Delegate callback)
        => callback.Method.DeclaringType == typeof(com.IvanMurzak.Unity.MCP.Editor.Startup)
           && callback.Method.Name == "OnApplicationQuitting";
}
