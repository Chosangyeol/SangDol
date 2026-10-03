using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

// The installed MCP package treats Application.quitting as Editor shutdown.
// Unity also raises that event when PlayMode ends, while test results may still
// be reconnecting. Keep the package's cleanup, but run it on actual Editor quit.
[InitializeOnLoad]
internal static class McpEditorQuitLifecycle
{
    static McpEditorQuitLifecycle()
    {
        if (!Install())
            Debug.LogWarning("[MCP] Editor quit lifecycle hook changed; the project compatibility fix was not applied.");
    }

    internal static bool Install()
    {
        var startup = Type.GetType("com.IvanMurzak.Unity.MCP.Editor.Startup, com.IvanMurzak.Unity.MCP.Editor");
        if (startup == null) return false;
        RuntimeHelpers.RunClassConstructor(startup.TypeHandle);
        var method = startup.GetMethod("OnApplicationQuitting", BindingFlags.Static | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);
        if (method == null || method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
            return false;

        var cleanup = (Action)Delegate.CreateDelegate(typeof(Action), method);
        Application.quitting -= cleanup;
        EditorApplication.quitting -= cleanup;
        EditorApplication.quitting += cleanup;
        return true;
    }
}
