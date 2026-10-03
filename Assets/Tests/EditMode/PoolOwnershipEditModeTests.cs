using System; using System.Reflection; using System.Runtime.ExceptionServices; using NUnit.Framework;
public class PoolOwnershipEditModeTests {
private static void Run(string n) { var t=Type.GetType("PoolOwnershipCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); try { t.GetMethod(n).Invoke(null,null); } catch(TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); } }
[Test] public void DuplicateReturnCannotLeaseOneObjectTwice()=>Run(nameof(DuplicateReturnCannotLeaseOneObjectTwice));
[Test] public void ForeignAndRenamedObjectsRespectOwnership()=>Run(nameof(ForeignAndRenamedObjectsRespectOwnership));
[Test] public void DestroyedAvailableObjectIsReplaced()=>Run(nameof(DestroyedAvailableObjectIsReplaced));
[Test] public void DuplicateCreationAndStageClearPreserveGlobal()=>Run(nameof(DuplicateCreationAndStageClearPreserveGlobal));
}