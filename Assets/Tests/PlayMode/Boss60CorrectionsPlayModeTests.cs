using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Boss60CorrectionsPlayModeTests
{
    private readonly Dictionary<GameObject, bool> roots = new Dictionary<GameObject, bool>();
    private object previousDamageText;
    private System.Reflection.FieldInfo damageTextInstance;
    [UnitySetUp] public IEnumerator IsolateMain()
    {
        damageTextInstance = Type.GetType("DamageTextManager, Assembly-CSharp").GetField("Instance");
        previousDamageText = damageTextInstance.GetValue(null); damageTextInstance.SetValue(null, null);
        roots.Clear(); var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main");
        if (scene.IsValid() && scene.isLoaded)
            foreach (var root in scene.GetRootGameObjects()) { roots[root] = root.activeSelf; root.SetActive(false); }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreMain()
    {
        foreach (var pair in roots) if (pair.Key != null) pair.Key.SetActive(pair.Value);
        roots.Clear(); yield return null;
        damageTextInstance.SetValue(null, previousDamageText);
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("Boss60CorrectionsRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator RealJumpInteractionEndsLoopOnlyAtLanding() => Run(nameof(RealJumpInteractionEndsLoopOnlyAtLanding));
    [UnityTest] public IEnumerator SwingAllowsMovementAndBossIgnoresDamage() => Run(nameof(SwingAllowsMovementAndBossIgnoresDamage));
    [UnityTest] public IEnumerator MusicResumesOnlyAfterMiddleBossDeathAndReturn() => Run(nameof(MusicResumesOnlyAfterMiddleBossDeathAndReturn));
    [UnityTest] public IEnumerator MusicCancellationPreservesOriginalPlaybackAndMute() => Run(nameof(MusicCancellationPreservesOriginalPlaybackAndMute));
    [UnityTest] public IEnumerator ActualBatCastsOncePerTargetAndStopsOnDeath() => Run(nameof(ActualBatCastsOncePerTargetAndStopsOnDeath));
}
