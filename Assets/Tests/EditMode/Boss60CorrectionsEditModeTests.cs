using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class Boss60CorrectionsEditModeTests
{
    [Test] public void MiddleBossUsesAnimatedBatAndCombatCollider()
    {
        var type = Type.GetType("Boss60CorrectionsCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        try { type.GetMethod(nameof(MiddleBossUsesAnimatedBatAndCombatCollider)).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
}
