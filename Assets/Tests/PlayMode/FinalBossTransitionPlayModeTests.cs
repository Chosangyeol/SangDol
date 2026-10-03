using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class FinalBossTransitionPlayModeTests
{
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("FinalBossTransitionRuntimeCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator NormalAnimationFinishesBeforeSwingStarts() => Run(nameof(NormalAnimationFinishesBeforeSwingStarts));
    [UnityTest] public IEnumerator SwingLandsAtCenterBeforeTimelineAndEncounter() => Run(nameof(SwingLandsAtCenterBeforeTimelineAndEncounter));
    [UnityTest] public IEnumerator VideoFinishesBeforeTimelineAndEncounter() => Run(nameof(VideoFinishesBeforeTimelineAndEncounter));
    [UnityTest] public IEnumerator CancellationRestoresAirborneBossAndControls() => Run(nameof(CancellationRestoresAirborneBossAndControls));
    [UnityTest] public IEnumerator CancellationStopsTimelineWithoutStartingEncounter() => Run(nameof(CancellationStopsTimelineWithoutStartingEncounter));
}
