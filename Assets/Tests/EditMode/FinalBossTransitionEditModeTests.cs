using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class FinalBossTransitionEditModeTests
{
    [Test]
    public void ThresholdWaitsForCurrentPatternAndStartsOnce()
    {
        var type = Type.GetType("FinalBossTransitionCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(nameof(ThresholdWaitsForCurrentPatternAndStartsOnce)).Invoke(null, null); }
        catch (TargetInvocationException ex) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
    }
}
