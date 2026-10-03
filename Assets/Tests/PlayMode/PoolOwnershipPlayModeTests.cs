using System; using System.Collections; using NUnit.Framework; using UnityEngine.TestTools;
public class PoolOwnershipPlayModeTests {
private static IEnumerator Run(string n) { var t=Type.GetType("PoolOwnershipRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); return (IEnumerator)t.GetMethod(n).Invoke(null,null); }
[UnityTest] public IEnumerator ClearDestroysActiveAndInactiveAndRejectsLateReturn()=>Run(nameof(ClearDestroysActiveAndInactiveAndRejectsLateReturn));
[UnityTest] public IEnumerator ReentrantDisableReturn()
{
    var t=Type.GetType("PoolOwnershipRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t);
    return (IEnumerator)t.GetMethod(nameof(ReentrantDisableReturn)).Invoke(null,new object[] { typeof(PoolReturnProbe) });
}
[UnityTest] public IEnumerator ReturnRestoresPoolParentAndSupportsOverflow()=>Run(nameof(ReturnRestoresPoolParentAndSupportsOverflow));
}