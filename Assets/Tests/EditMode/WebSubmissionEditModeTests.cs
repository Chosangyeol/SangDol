using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class WebSubmissionEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("WebSubmissionCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException error) { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); }
    }
    [Test] public void UrlEscapesSegmentsAndRejectsTraversal() => Run(nameof(UrlEscapesSegmentsAndRejectsTraversal));
    [Test] public void UrlSourceWorksWithoutImportedClip() => Run(nameof(UrlSourceWorksWithoutImportedClip));
    [Test] public void PreparedMetadataMatchesOriginalAndFile() => Run(nameof(PreparedMetadataMatchesOriginalAndFile));
    [Test] public void BuildCopyKeepsUrlAndOriginalClip() => Run(nameof(BuildCopyKeepsUrlAndOriginalClip));
    [Test] public void RemovingClipClearsGeneratedPath() => Run(nameof(RemovingClipClearsGeneratedPath));
}
