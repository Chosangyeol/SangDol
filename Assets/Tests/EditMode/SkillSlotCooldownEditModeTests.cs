using System; using System.Reflection; using System.Runtime.ExceptionServices; using NUnit.Framework;
public class SkillSlotCooldownEditModeTests {
private static void Run(string n) { var t=Type.GetType("SkillSlotCooldownCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); try { t.GetMethod(n).Invoke(null,null); } catch(TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); } }
[Test] public void ClearingSlotRefreshRemovesCooldownImmediately()=>Run(nameof(ClearingSlotRefreshRemovesCooldownImmediately));
[Test] public void ReadyReplacementClearsPreviousCooldown()=>Run(nameof(ReadyReplacementClearsPreviousCooldown));
[Test] public void EmptyCooldownOnlySlotHidesAndDoesNotInterceptClicks()=>Run(nameof(EmptyCooldownOnlySlotHidesAndDoesNotInterceptClicks));
[Test] public void LevelZeroSkillRemovalClearsCooldown()=>Run(nameof(LevelZeroSkillRemovalClearsCooldown));
}