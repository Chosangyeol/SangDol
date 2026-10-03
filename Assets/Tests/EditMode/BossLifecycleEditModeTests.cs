using System; using System.Reflection; using System.Runtime.ExceptionServices; using NUnit.Framework;
public class BossLifecycleEditModeTests { private static void Run(string n) { var t=Type.GetType("BossLifecycleCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); try { t.GetMethod(n).Invoke(null,null); } catch(TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); } }
[Test] public void ResetClearsEncounterFlagsAndCooldown()=>Run(nameof(ResetClearsEncounterFlagsAndCooldown));
[Test] public void ResetAllowsMissingOptionalData()=>Run(nameof(ResetAllowsMissingOptionalData));
}

