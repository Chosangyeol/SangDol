using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class BuffTooltipEditModeTests
{
    private static void Run(string name)
    {
        var cases = Type.GetType("BuffTooltipCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(cases);
        try { cases.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException error) { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); }
    }

    [Test] public void HidesWhenHoveredBuffSlotIsDisabledOrReused()
        => Run(nameof(HidesWhenHoveredBuffSlotIsDisabledOrReused));

    [Test] public void DisablingOtherSlotKeepsCurrentDescription()
        => Run(nameof(DisablingOtherSlotKeepsCurrentDescription));
}
