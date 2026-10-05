using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class Jester60IntroductionEditModeTests
{
    [Test] public void PrefabHasSynchronizedTimelinesAndSafeCamera()
    {
        var type = Type.GetType("Jester60IntroductionCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        try { type.GetMethod(nameof(PrefabHasSynchronizedTimelinesAndSafeCamera)).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
}
