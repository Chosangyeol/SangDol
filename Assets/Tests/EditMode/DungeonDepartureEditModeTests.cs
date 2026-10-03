using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class DungeonDepartureEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("DungeonDepartureCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
    [Test] public void UnknownAndEmptySectorsCannotFinishDungeon() => Run(nameof(UnknownAndEmptySectorsCannotFinishDungeon));
    [Test] public void CompletionIsClaimedByFirstHandlerAndOnlyOnce() => Run(nameof(CompletionIsClaimedByFirstHandlerAndOnlyOnce));
    [Test] public void IntermediateSectorDoesNotTriggerDeparture() => Run(nameof(IntermediateSectorDoesNotTriggerDeparture));
}
