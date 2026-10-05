using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameFlowPlayModeTests
{
    [UnityTearDown] public IEnumerator CleanupActualScenes()
    {
        Time.timeScale = 1f;
        foreach (string name in new[] { "Map1-Forest", "Circus-Main-Hall" })
        {
            var world = SceneManager.GetSceneByName(name);
            if (world.IsValid() && world.isLoaded)
                foreach (var root in world.GetRootGameObjects()) root.SetActive(false);
        }
        var type = Type.GetType("PoolManager, Assembly-CSharp");
        var pool = type.GetProperty("Instance").GetValue(null) as Component;
        if (pool != null) UnityEngine.Object.Destroy(pool.gameObject);
        yield return null;
        var cleanup = SceneManager.CreateScene("GameFlowTestCleanup"); SceneManager.SetActiveScene(cleanup);
        foreach (string name in new[] { "Title", "Map1-Forest", "Circus-Main-Hall", "Main" })
        {
            var scene = SceneManager.GetSceneByName(name);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }
        yield return null;
    }
    [UnityTest] public IEnumerator TitleForestQuestsDungeonBossAndReturn()
    {
        var type = Type.GetType("GameFlowRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(nameof(TitleForestQuestsDungeonBossAndReturn)).Invoke(null, null);
    }
}
