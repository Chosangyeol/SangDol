using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class McpLifecycleEditModeTests
{
    [Test]
    public void CleanupUsesEditorQuitAndInstallationIsIdempotent()
    {
        var type = Type.GetType("McpLifecycleCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(nameof(CleanupUsesEditorQuitAndInstallationIsIdempotent)).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
}
