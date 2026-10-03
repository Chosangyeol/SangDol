using System; using System.Collections; using NUnit.Framework; using UnityEngine.TestTools;
public class BossLifecyclePlayModeTests { private static IEnumerator Run(string n) { var t=Type.GetType("BossLifecycleRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); return (IEnumerator)t.GetMethod(n).Invoke(null,null); }
[UnityTest] public IEnumerator DeathStopsAttacksAndWaitsBeforeReturn()=>Run(nameof(DeathStopsAttacksAndWaitsBeforeReturn));
[UnityTest] public IEnumerator PoolReuseResetsDeathAndSpecialState()=>Run(nameof(PoolReuseResetsDeathAndSpecialState));
[UnityTest] public IEnumerator PlayerDeathDuringDefeatCompletesOnce()=>Run(nameof(PlayerDeathDuringDefeatCompletesOnce));
[UnityTest] public IEnumerator SectorWaitsForPresentationAndRetainsCompletionAfterReuse()=>Run(nameof(SectorWaitsForPresentationAndRetainsCompletionAfterReuse));
}

