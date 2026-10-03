using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

public class GameplayAuditPlayModeTests
{
    private readonly Dictionary<UnityEngine.GameObject, bool> rootStates = new Dictionary<UnityEngine.GameObject, bool>();
    [UnitySetUp] public IEnumerator IsolateBootstrapScene()
    {
        var main = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main");
        rootStates.Clear();
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects())
            {
                rootStates[root] = root.activeSelf;
                root.SetActive(false);
            }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreBootstrapScene()
    {
        foreach (var entry in rootStates) if (entry.Key != null) entry.Key.SetActive(entry.Value);
        rootStates.Clear();
        yield return null;
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("GameplayAuditRuntimeCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator BasicAttackDamagesAndCountersEachEnemyOnce() => Run(nameof(BasicAttackDamagesAndCountersEachEnemyOnce));
    [UnityTest] public IEnumerator PumpkinAttackHitsOnceAndRejectsInvalidTargets() => Run(nameof(PumpkinAttackHitsOnceAndRejectsInvalidTargets));
    [UnityTest] public IEnumerator ReopeningNpcUiDoesNotDuplicateTalkButtons() => Run(nameof(ReopeningNpcUiDoesNotDuplicateTalkButtons));
}
