using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class GameFlowRegressionEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("GameFlowRegressionCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
    [Test] public void MariaTurnInPrecedesAppleQuest() => Run(nameof(MariaTurnInPrecedesAppleQuest));
    [Test] public void RuntimeMiddleBossCanBeHitBySkillQueries() => Run(nameof(RuntimeMiddleBossCanBeHitBySkillQueries));
    [Test] public void PotionConsumesOnceSharesCooldownAndHealsExactlyItsTotal() => Run(nameof(PotionConsumesOnceSharesCooldownAndHealsExactlyItsTotal));
    [Test] public void PotionHandlesUnevenFramesAndInfiniteHealingKeepsItsInterval() => Run(nameof(PotionHandlesUnevenFramesAndInfiniteHealingKeepsItsInterval));
}
