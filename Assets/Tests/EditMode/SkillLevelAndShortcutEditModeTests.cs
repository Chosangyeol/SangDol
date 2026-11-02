using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class SkillLevelAndShortcutEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("SkillLevelAndShortcutCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException exception) { ExceptionDispatchInfo.Capture(exception.InnerException).Throw(); }
    }

    [Test] public void SkillRequiresConfiguredLevelAndDoesNotSpendPointsWhenLockedOrMaxed()
        => Run(nameof(SkillRequiresConfiguredLevelAndDoesNotSpendPointsWhenLockedOrMaxed));

    [Test] public void ShortcutLevelGrantUsesNormalRewardsAndNeverReducesLevel()
        => Run(nameof(ShortcutLevelGrantUsesNormalRewardsAndNeverReducesLevel));

    [Test] public void SkillSlotTintAndButtonFollowRequiredLevel()
        => Run(nameof(SkillSlotTintAndButtonFollowRequiredLevel));
}
