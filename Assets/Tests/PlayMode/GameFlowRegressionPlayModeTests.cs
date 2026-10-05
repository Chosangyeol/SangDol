using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameFlowRegressionPlayModeTests
{
    private readonly Dictionary<GameObject, bool> rootStates = new Dictionary<GameObject, bool>();
    [UnitySetUp] public IEnumerator IsolateMain()
    {
        rootStates.Clear(); var main = SceneManager.GetSceneByName("Main");
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects()) { rootStates[root] = root.activeSelf; root.SetActive(false); }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreMain()
    {
        foreach (var entry in rootStates) if (entry.Key != null) entry.Key.SetActive(entry.Value);
        rootStates.Clear(); yield return null;
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("GameFlowRegressionRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator BoxPatternYieldsAndEndsWhenGroundIsMissing() => Run(nameof(BoxPatternYieldsAndEndsWhenGroundIsMissing));
    [UnityTest] public IEnumerator MissingGameplayCameraDoesNotBreakHover() => Run(nameof(MissingGameplayCameraDoesNotBreakHover));
    [UnityTest] public IEnumerator ProjectileResolvesOnlyOnePlayerHit() => Run(nameof(ProjectileResolvesOnlyOnePlayerHit));
    [UnityTest] public IEnumerator SwingLastFrameCannotReviveDeadOrDestroyedEnemy() => Run(nameof(SwingLastFrameCannotReviveDeadOrDestroyedEnemy));
    [UnityTest] public IEnumerator JumpReleasesOnlyLivingPlayersAndHandlesCancellation() => Run(nameof(JumpReleasesOnlyLivingPlayersAndHandlesCancellation));
}
