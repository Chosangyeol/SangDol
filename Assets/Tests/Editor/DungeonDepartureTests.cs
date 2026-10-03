using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class DungeonDepartureRig : IDisposable
{
    public readonly DungeonManager Manager;
    public readonly SectorController Final;
    public DungeonCompletionReturnToTitle Endpoint;
    private readonly DungeonManager previous = DungeonManager.instance;
    private readonly GameObject root;
    public static object Get(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    public static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    public DungeonDepartureRig(bool runtime)
    {
        root = new GameObject("DepartureTestDungeon"); root.SetActive(false);
        Manager = root.AddComponent<DungeonManager>(); Manager.isEnterStart = false;
        Manager.dungeonUI = new GameObject("DepartureTestUI"); Manager.dungeonUI.transform.SetParent(root.transform);
        var sector = new GameObject("DepartureTestFinal"); sector.SetActive(false); sector.transform.SetParent(root.transform);
        Final = sector.AddComponent<SectorController>(); Final.sectorObjects = new List<GameObject>();
        Manager.allSectors = new List<SectorController> { Final };
        if (runtime) { DungeonManager.instance = null; root.SetActive(true); }
    }
    public void AddEndpoint(string scene = "Title", bool automatic = false)
    {
        var go = new GameObject("DepartureTestEndpoint"); go.SetActive(false); go.transform.SetParent(root.transform);
        Endpoint = go.AddComponent<DungeonCompletionReturnToTitle>();
        Set(Endpoint, "dungeonManager", automatic ? null : Manager);
        Set(Endpoint, "titleSceneName", scene); Set(Endpoint, "returnDelay", 10f);
        go.SetActive(true);
    }
    public void Dispose()
    {
        if (root != null) UnityEngine.Object.DestroyImmediate(root);
        DungeonManager.instance = previous;
    }
}

public static class DungeonDepartureCases
{
    public static void UnknownAndEmptySectorsCannotFinishDungeon()
    {
        using (var r = new DungeonDepartureRig(false))
        {
            int calls = 0;
            DungeonDepartureRig.Set(r.Manager, "CompletionRequested", new Func<DungeonManager, bool>(_ => { calls++; return true; }));
            r.Manager.OnSectorCleared(null);
            r.Manager.allSectors.Clear(); r.Manager.OnSectorCleared(r.Final);
            r.Manager.allSectors = null; r.Manager.OnSectorCleared(r.Final);
            r.Manager.allSectors = new List<SectorController> { null }; r.Manager.OnSectorCleared(r.Final);
            Assert.AreEqual(0, calls); Assert.IsFalse((bool)DungeonDepartureRig.Get(r.Manager, "dungeonCompleted"));
        }
    }
    public static void CompletionIsClaimedByFirstHandlerAndOnlyOnce()
    {
        using (var r = new DungeonDepartureRig(false))
        {
            int declined = 0, claimed = 0, skipped = 0;
            Func<DungeonManager, bool> handlers = _ => { declined++; return false; };
            handlers += source => { Assert.AreSame(r.Manager, source); claimed++; return true; };
            handlers += _ => { skipped++; return true; };
            DungeonDepartureRig.Set(r.Manager, "CompletionRequested", handlers);
            r.Manager.OnSectorCleared(r.Final); r.Manager.OnSectorCleared(r.Final);
            Assert.AreEqual(1, declined); Assert.AreEqual(1, claimed); Assert.AreEqual(0, skipped);
            Assert.IsNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
        }
    }
    public static void IntermediateSectorDoesNotTriggerDeparture()
    {
        using (var r = new DungeonDepartureRig(false))
        {
            int calls = 0;
            DungeonDepartureRig.Set(r.Manager, "CompletionRequested", new Func<DungeonManager, bool>(_ => { calls++; return true; }));
            var go = new GameObject("DepartureTestFirst"); go.SetActive(false); go.transform.SetParent(r.Final.transform.parent);
            var first = go.AddComponent<SectorController>(); first.sectorObjects = new List<GameObject>();
            r.Manager.allSectors.Insert(0, first);
            r.Manager.OnSectorCleared(first);
            Assert.AreEqual(0, calls); Assert.AreEqual(1, r.Manager.currentSector);
        }
    }
}

public static class DungeonDepartureRuntimeCases
{
    public static IEnumerator DisabledOrRemovedEndpointRestoresDefaultDeparture()
    {
        using (var player = new MovementRig(true))
        using (var r = new DungeonDepartureRig(true))
        {
            r.AddEndpoint(); yield return null;
            r.Manager.OnSectorCleared(r.Final); r.Manager.OnSectorCleared(r.Final);
            Assert.IsFalse(player.Model.canMove);
            Assert.IsNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
            r.Endpoint.enabled = false;
            Assert.IsTrue(player.Model.canMove);
            Assert.IsNotNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
            Assert.IsNull(DungeonDepartureRig.Get(r.Endpoint, "returnRoutine"));
        }
        using (var r = new DungeonDepartureRig(true))
        {
            r.AddEndpoint(); yield return null; r.Manager.OnSectorCleared(r.Final);
            UnityEngine.Object.Destroy(r.Endpoint.gameObject); yield return null;
            Assert.IsNotNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
        }
        using (var player = new MovementRig(true))
        using (var r = new DungeonDepartureRig(true))
        {
            player.Model.ControlDisable(); r.AddEndpoint(); yield return null;
            r.Manager.OnSectorCleared(r.Final); r.Endpoint.enabled = false;
            Assert.IsFalse(player.Model.canMove, "Cancelling departure must not release another system's lock.");
        }
    }
    public static IEnumerator MissingEndpointAndInvalidMenuUseDefaultDeparture()
    {
        using (var r = new DungeonDepartureRig(true))
        {
            yield return null; r.Manager.OnSectorCleared(r.Final);
            Assert.IsNotNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
        }
        using (var r = new DungeonDepartureRig(true))
        {
            r.AddEndpoint("MissingDepartureTestScene"); yield return null; r.Manager.OnSectorCleared(r.Final);
            Assert.IsFalse((bool)DungeonDepartureRig.Get(r.Endpoint, "departureClaimed"));
            Assert.IsNotNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
        }
    }
    public static IEnumerator MovingAndReenablingEndpointKeepsOneSubscription()
    {
        using (var r = new DungeonDepartureRig(true))
        {
            r.AddEndpoint(automatic: true); yield return null;
            r.Endpoint.enabled = false; r.Endpoint.transform.position = Vector3.one * 500f; r.Endpoint.enabled = true;
            var handlers = (Delegate)DungeonDepartureRig.Get(r.Manager, "CompletionRequested");
            Assert.AreEqual(1, handlers.GetInvocationList().Length);
            r.Manager.OnSectorCleared(r.Final);
            Assert.IsTrue((bool)DungeonDepartureRig.Get(r.Endpoint, "departureClaimed"));
            Assert.IsNull(DungeonDepartureRig.Get(r.Manager, "dungeonOutRoutine"));
        }
    }
    public static IEnumerator ReturnsToActualTitleAndCanStartFreshGame()
    {
        var r = new DungeonDepartureRig(true);
        var previousScale = Time.timeScale;
        try
        {
            if (PoolManager.Instance == null) new GameObject("DepartureTestPool").AddComponent<PoolManager>();
            var oldPool = PoolManager.Instance;
            var template = new GameObject("DeparturePoolSentinel"); template.SetActive(false);
            template.transform.SetParent(r.Manager.transform);
            var source = template.AddComponent<PoolableMono>();
            oldPool.CreatePool(source, 1, false); var sentinel = oldPool.Pop(source.name);
            r.AddEndpoint(); DungeonDepartureRig.Set(r.Endpoint, "returnDelay", .02f);
            yield return null; Time.timeScale = 0f; r.Manager.OnSectorCleared(r.Final);
            float deadline = Time.realtimeSinceStartup + 15f;
            while (!(bool)DungeonDepartureRig.Get(r.Endpoint, "sceneLoadStarted") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue((bool)DungeonDepartureRig.Get(r.Endpoint, "sceneLoadStarted"));
            UnityEngine.Object.DestroyImmediate(r.Endpoint.gameObject);
            while (!SceneManager.GetSceneByName("Title").isLoaded && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(SceneManager.GetSceneByName("Title").isLoaded);
            Assert.AreEqual(1, SceneManager.sceneCount); Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(oldPool == null && sentinel == null); Assert.IsTrue(PoolManager.Instance == null);
            Assert.IsNotNull(TitleManager.instance);
            TitleManager.instance.StartGame();
            deadline = Time.realtimeSinceStartup + 20f;
            while ((!SceneManager.GetSceneByName("Map1-Forest").isLoaded || SceneManager.GetSceneByName("Title").isLoaded) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(3.5f);
            Assert.IsTrue(SceneManager.GetSceneByName("Main").isLoaded);
            Assert.IsTrue(SceneManager.GetSceneByName("Map1-Forest").isLoaded);
            Assert.IsFalse(SceneManager.GetSceneByName("Title").isLoaded);
            Assert.IsNotNull(PoolManager.Instance);
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsByType<CharacterModel>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsByType<UIManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
        }
        finally { Time.timeScale = previousScale; r.Dispose(); }
    }
}
