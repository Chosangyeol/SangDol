using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class DungeonDeparturePlayModeTests
{
    private readonly Dictionary<GameObject, bool> rootStates = new Dictionary<GameObject, bool>();
    private bool loadedActualScenes;
    [UnitySetUp] public IEnumerator IsolateBootstrapScene()
    {
        loadedActualScenes = false; rootStates.Clear(); var main = SceneManager.GetSceneByName("Main");
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects()) { rootStates[root] = root.activeSelf; root.SetActive(false); }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreBootstrapScene()
    {
        // Runs on assertion failures as well: actual Single scene loads replace
        // the runner's temporary scene, so clear fresh gameplay before reuse.
        if (loadedActualScenes)
        {
            var type = Type.GetType("PoolManager, Assembly-CSharp");
            var pool = type.GetProperty("Instance").GetValue(null) as Component;
            if (pool != null) UnityEngine.Object.Destroy(pool.gameObject);
            yield return null;
            var cleanup = SceneManager.CreateScene("DungeonDepartureTestCleanup"); SceneManager.SetActiveScene(cleanup);
            foreach (string name in new[] { "Title", "Map1-Forest", "Main" })
            {
                var scene = SceneManager.GetSceneByName(name);
                if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            }
        }
        foreach (var entry in rootStates) if (entry.Key != null) entry.Key.SetActive(entry.Value);
        rootStates.Clear(); yield return null;
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("DungeonDepartureRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator DisabledOrRemovedEndpointRestoresDefaultDeparture() => Run(nameof(DisabledOrRemovedEndpointRestoresDefaultDeparture));
    [UnityTest] public IEnumerator MissingEndpointAndInvalidMenuUseDefaultDeparture() => Run(nameof(MissingEndpointAndInvalidMenuUseDefaultDeparture));
    [UnityTest] public IEnumerator MovingAndReenablingEndpointKeepsOneSubscription() => Run(nameof(MovingAndReenablingEndpointKeepsOneSubscription));
    [UnityTest] public IEnumerator ReturnsToActualTitleAndCanStartFreshGame()
    {
        loadedActualScenes = true;
        return Run(nameof(ReturnsToActualTitleAndCanStartFreshGame));
    }
}
