using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Jester60IntroductionPlayModeTests
{
    private readonly Dictionary<GameObject, bool> rootStates = new Dictionary<GameObject, bool>();
    [UnitySetUp] public IEnumerator IsolateBootstrapScene()
    {
        rootStates.Clear(); var main = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main");
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects()) { rootStates[root] = root.activeSelf; root.SetActive(false); }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreBootstrapScene()
    {
        foreach (var pair in rootStates) if (pair.Key != null) pair.Key.SetActive(pair.Value);
        rootStates.Clear(); yield return null;
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("Jester60IntroductionRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator PooledBossBindsAndCancellationRestoresCameraAndMusic() => Run(nameof(PooledBossBindsAndCancellationRestoresCameraAndMusic));
    [UnityTest] public IEnumerator IntroductionFinishesBeforeMiddleBossStarts() => Run(nameof(IntroductionFinishesBeforeMiddleBossStarts));
    [UnityTest] public IEnumerator DisablingBinderDuringPlaybackRestoresOwnedState() => Run(nameof(DisablingBinderDuringPlaybackRestoresOwnedState));
    [UnityTest] public IEnumerator SharedActorSurvivesCancellationRetryAndBinderDisable() => Run(nameof(SharedActorSurvivesCancellationRetryAndBinderDisable));
    [UnityTest] public IEnumerator MissingTimelineStillUsesNavigableSharedArenaAndDoesNotCancelOtherPatterns() => Run(nameof(MissingTimelineStillUsesNavigableSharedArenaAndDoesNotCancelOtherPatterns));
    [UnityTest] public IEnumerator FadesUseRealtimeAndCancellationReleasesHeldVideo() => Run(nameof(FadesUseRealtimeAndCancellationReleasesHeldVideo));
}
