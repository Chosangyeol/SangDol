using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;

public sealed class DungeonObjectiveVisibilityRig : IDisposable
{
    public readonly GameObject Root;
    public readonly DungeonManager Manager;
    public readonly GameObject UI;
    private readonly DungeonManager previous = DungeonManager.instance;
    public DungeonObjectiveVisibilityRig()
    {
        DungeonManager.instance = null;
        Root = new GameObject("ObjectiveVisibilityTest"); Root.SetActive(false);
        Manager = Root.AddComponent<DungeonManager>(); Manager.isEnterStart = false;
        UI = new GameObject("ObjectiveVisibilityUI"); UI.transform.SetParent(Root.transform);
        Manager.dungeonUI = UI;
        var sectorRoot = new GameObject("ObjectiveVisibilitySector"); sectorRoot.SetActive(false); sectorRoot.transform.SetParent(Root.transform);
        var sector = sectorRoot.AddComponent<SectorController>(); sector.sectorName = "Updated sector"; sector.sectorObjects = new List<GameObject>();
        typeof(SectorController).GetField("_conditions", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(sector, new List<ISectorCondition> { new Goal() });
        Manager.allSectors = new List<SectorController> { sector };
        Manager.sectorName = Text("SectorName"); Manager.sectorGoal = Text("SectorGoal");
        Root.SetActive(true);
        if (!Application.isPlaying) DungeonManager.instance = Manager;
    }
    private TMP_Text Text(string name)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(UI.transform);
        return go.AddComponent<TextMeshProUGUI>();
    }
    private sealed class Goal : ISectorCondition
    {
        public string SectorGoal => "Test goal";
        public bool IsSatisfied => false;
        public string GetProgressString() => "Updated progress 1/2";
        public void OnConditionStart() { }
    }
    public object Call(string name, params object[] args) => typeof(DungeonManager).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Manager, args);
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(Root); DungeonManager.instance = previous;
    }
}

public sealed class VideoRecoveryRig : IDisposable
{
    public readonly GameObject Root;
    public readonly VideoPlayManager Manager;
    public readonly VideoPlayer Player;
    public readonly RawImage Image;
    private readonly VideoPlayManager previous = VideoPlayManager.instance;
    public VideoRecoveryRig(bool withPlayer = true)
    {
        Root = new GameObject("VideoRecoveryTest"); Root.SetActive(false);
        if (withPlayer) { Player = Root.AddComponent<VideoPlayer>(); Player.playOnAwake = false; Player.renderMode = VideoRenderMode.APIOnly; }
        Image = Root.AddComponent<RawImage>();
        Manager = Root.AddComponent<VideoPlayManager>();
        Root.SetActive(true);
        if (!Application.isPlaying) Call("Awake");
    }
    public object Call(string name, params object[] args) => typeof(VideoPlayManager).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Manager, args);
    public void Set(string name, object value) => typeof(VideoPlayManager).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Manager, value);
    public void Seed(int session)
    {
        Set("playbackSession", session); Manager.isPlaying = true; Image.enabled = true;
    }
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(Root); VideoPlayManager.instance = previous;
    }
}
public static class VideoRecoveryCases
{
    public static void ObjectivesRestoreOnlyAfterLastOwnerAndPreserveHiddenState()
    {
        using (var d = new DungeonObjectiveVisibilityRig())
        using (var video = new VideoRecoveryRig())
        {
            foreach (bool visible in new[] { false, true })
            {
                d.UI.SetActive(visible);
                d.Call("SuppressObjectives", video.Manager); d.Call("SuppressObjectives", video.Manager);
                d.Call("SuppressObjectives", video.Player);
                Assert.IsFalse(d.UI.activeSelf);
                d.Call("RestoreObjectives", video.Manager); d.Call("RestoreObjectives", video.Manager);
                Assert.IsFalse(d.UI.activeSelf);
                d.Call("RestoreObjectives", video.Player); Assert.AreEqual(visible, d.UI.activeSelf);
            }
        }
    }

    public static void ObjectiveProgressUpdatesWithoutReappearingDuringPresentation()
    {
        using (var d = new DungeonObjectiveVisibilityRig())
        using (var video = new VideoRecoveryRig())
        {
            d.UI.SetActive(false); d.Call("SuppressObjectives", video.Manager);
            d.Manager.UpdateDungeonUI();
            Assert.IsFalse(d.UI.activeSelf);
            Assert.AreEqual("Updated sector", d.Manager.sectorName.text);
            Assert.AreEqual("Updated progress 1/2", d.Manager.sectorGoal.text);
            d.Call("RestoreObjectives", video.Manager); Assert.IsTrue(d.UI.activeSelf);
        }
    }
    public static void MissingPlayerAndClipAreSafe()
    {
        using (var r = new VideoRecoveryRig(false))
        {
            Assert.DoesNotThrow(() => r.Manager.PlayVideo(null)); Assert.IsFalse(r.Manager.isPlaying);
            Assert.DoesNotThrow(() => { r.Manager.SkipVideo(); r.Manager.ClearClip(); r.Manager.OnVideoFinished(null); });
            Assert.IsFalse(r.Image.enabled);
        }
    }
    public static void BusyPlaybackDoesNotLoseOwnership()
    {
        using (var r = new VideoRecoveryRig())
        {
            r.Seed(7);
            object[] args = { null, 0 };
            Assert.IsFalse((bool)r.Call("TryPlayVideo", args));
            Assert.AreEqual(0, args[1]);
            Assert.IsTrue((bool)r.Call("IsPlaybackActive", 7));
            r.Manager.SkipVideo();
        }
    }
    public static void Special3GameplayConfigurationAllowsMissingVideo()
    {
        using (var r = new MovementRig(false))
        {
            var previous = VideoPlayManager.instance; VideoPlayManager.instance = null;
            var bossObject = new GameObject("Special3ConfigBoss"); bossObject.SetActive(false);
            var middleObject = new GameObject("Special3ConfigMiddle"); middleObject.SetActive(false);
            var anchor = new GameObject("Special3ConfigAnchor");
            var stat = ScriptableObject.CreateInstance<EnemyStatSO>();
            var boss = bossObject.AddComponent<D1_FinalBoss>();
            var middle = middleObject.AddComponent<D1_MiddleBoss>(); middle.statSO = stat;
            try
            {
                boss.center = boss.playerStartPos = anchor.transform;
                boss.Special3 = new D1_Final_Special3Data { prefab = middleObject, waitingArea = anchor.transform, middleBossSpawnPoint = anchor.transform };
                typeof(EnemyBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(boss, r.Model);
                var configured = typeof(D1_FinalBoss).GetMethod("IsSpecial3Configured", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsTrue((bool)configured.Invoke(boss, null));
                boss.Special3.waitingArea = null; Assert.IsFalse((bool)configured.Invoke(boss, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bossObject); UnityEngine.Object.DestroyImmediate(middleObject);
                UnityEngine.Object.DestroyImmediate(anchor); UnityEngine.Object.DestroyImmediate(stat);
                VideoPlayManager.instance = previous;
            }
        }
    }
    public static void InvalidWarpDoesNotTakeControls()
    {
        using (var r = new MovementRig(false))
        {
            var previous = DungeonManager.instance; var go = new GameObject("InvalidWarpTest"); go.SetActive(false);
            var manager = go.AddComponent<DungeonManager>();
            try
            {
                InventoryTransferRig.Set(manager, "_model", r.Model);
                Assert.DoesNotThrow(() => { manager.WarpPlayer(-1); manager.WarpPlayer(100); });
                Assert.IsFalse((bool)typeof(CharacterModel).GetProperty("IsExternalControlLocked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Model));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); DungeonManager.instance = previous; }
        }
    }
}
public static class VideoRecoveryRuntimeCases
{
    public static IEnumerator DungeonObjectivesStayHiddenUntilVideoFrameIsReleased()
    {
        using (var d = new DungeonObjectiveVisibilityRig())
        using (var video = new VideoRecoveryRig())
        {
            yield return null;
            video.Player.audioOutputMode = VideoAudioOutputMode.None;
            var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/08. Sound/Jester60Intro.mp4"); Assert.IsNotNull(clip);
            foreach (bool visible in new[] { false, true })
            foreach (string completion in new[] { "Completed", "Skipped", "Failed", "Cancelled" })
            {
                d.UI.SetActive(visible);
                object[] args = { clip, 0 }; Assert.IsTrue((bool)video.Call("TryPlayVideo", args));
                int session = (int)args[1]; Assert.IsFalse(d.UI.activeSelf);
                video.Call("HoldFrameForTransition", session);
                video.Call("CancelPlayback", session - 1); Assert.IsFalse(d.UI.activeSelf);
                if (completion == "Completed") video.Manager.OnVideoFinished(video.Player);
                else if (completion == "Skipped") video.Manager.SkipVideo();
                else if (completion == "Failed") video.Call("OnVideoError", video.Player, "objective visibility test decode failure");
                else video.Call("CancelPlayback", session);
                bool held = completion == "Completed" || completion == "Skipped";
                Assert.AreEqual(held, video.Image.enabled);
                Assert.AreEqual(held ? false : visible, d.UI.activeSelf);
                if (held)
                {
                    video.Call("CancelPlayback", session - 1); Assert.IsFalse(d.UI.activeSelf);
                    video.Call("CancelPlayback", session); Assert.AreEqual(visible, d.UI.activeSelf);
                }
            }
            d.UI.SetActive(true);
            object[] finalArgs = { clip, 0 }; Assert.IsTrue((bool)video.Call("TryPlayVideo", finalArgs));
            video.Call("HoldFrameForTransition", (int)finalArgs[1]); video.Manager.SkipVideo();
            video.Root.SetActive(false); Assert.IsTrue(d.UI.activeSelf);
            Assert.IsFalse(video.Image.enabled);
        }
    }

    public static IEnumerator OverlappingTimelinesPauseAndCancellationRestoreObjectives()
    {
        using (var d = new DungeonObjectiveVisibilityRig())
        {
            yield return null;
            var aRoot = new GameObject("ObjectiveTimelineA"); var bRoot = new GameObject("ObjectiveTimelineB");
            var a = aRoot.AddComponent<PlayableDirector>(); var b = bRoot.AddComponent<PlayableDirector>();
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>(); timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = 1d; timeline.CreateTrack<ActivationTrack>(null, "Test");
            a.playableAsset = b.playableAsset = timeline; a.extrapolationMode = b.extrapolationMode = DirectorWrapMode.None;
            try
            {
                foreach (bool visible in new[] { false, true })
                {
                    d.UI.SetActive(visible); d.Call("PlayCutscene", a); d.Call("PlayCutscene", b);
                    Assert.IsFalse(d.UI.activeSelf);
                    a.Pause(); yield return null; Assert.IsFalse(d.UI.activeSelf, "Pause retains the cinematic frame");
                    a.Stop(); Assert.IsFalse(d.UI.activeSelf); b.Stop(); Assert.AreEqual(visible, d.UI.activeSelf);
                }
                d.UI.SetActive(true); d.Call("PlayCutscene", a);
                d.Manager.UpdateDungeonUI(); Assert.IsFalse(d.UI.activeSelf);
                a.enabled = false; yield return null; Assert.IsTrue(d.UI.activeSelf);
                a.enabled = true; d.Call("PlayCutscene", a); Assert.IsFalse(d.UI.activeSelf);
                aRoot.SetActive(false); yield return null; Assert.IsTrue(d.UI.activeSelf);
                aRoot.SetActive(true); d.Call("PlayCutscene", a); Assert.IsFalse(d.UI.activeSelf);
                UnityEngine.Object.Destroy(aRoot); yield return null; Assert.IsTrue(d.UI.activeSelf);
                d.Call("PlayCutscene", b); d.Manager.enabled = false; Assert.IsTrue(d.UI.activeSelf);
                b.Stop(); b.Play(); Assert.IsTrue(d.UI.activeSelf, "Disabled dungeon unsubscribes from Timeline events");
                b.Stop(); d.Manager.enabled = true;
                timeline.fixedDuration = .05d; d.Call("PlayCutscene", b);
                Assert.IsFalse(d.UI.activeSelf); yield return new WaitForSecondsRealtime(.15f); Assert.IsTrue(d.UI.activeSelf);
                b.playableAsset = null; d.Call("PlayCutscene", b); yield return null;
                Assert.IsTrue(d.UI.activeSelf, "Missing Timeline never owns a hide request");
            }
            finally
            {
                if (aRoot != null) UnityEngine.Object.DestroyImmediate(aRoot);
                UnityEngine.Object.DestroyImmediate(bRoot); UnityEngine.Object.DestroyImmediate(timeline);
            }
        }
    }

    public static IEnumerator TransitionFrameIsOwnedAndReleasedOnCancelDisableOrFailure()
    {
        foreach (string completion in new[] { "Completed", "Skipped", "Failed" })
        using (var r = new VideoRecoveryRig())
        {
            r.Seed(20); r.Call("HoldFrameForTransition", 20);
            if (completion == "Completed") r.Manager.OnVideoFinished(r.Player);
            else if (completion == "Skipped") r.Manager.SkipVideo();
            else r.Call("OnVideoError", r.Player, "transition decode failure");
            Assert.IsFalse(r.Manager.isPlaying);
            Assert.AreEqual(completion, r.Call("GetResult", 20).ToString());
            Assert.AreEqual(completion != "Failed", r.Image.enabled);
            if (completion != "Failed")
            {
                r.Call("CancelPlayback", 19); Assert.IsTrue(r.Image.enabled);
                object[] args = { null, 0 };
                Assert.IsFalse((bool)r.Call("TryPlayVideo", args));
                Assert.AreEqual(completion, r.Call("GetResult", 20).ToString());
                if (completion == "Completed") r.Call("CancelPlayback", 20);
                else r.Root.SetActive(false);
            }
            Assert.IsFalse(r.Image.enabled);
            Assert.IsNull(r.Player.clip);
            yield return null;
        }
    }

    public static IEnumerator WatchdogUsesRealtimeAndUnlocksFailedPlayback()
    {
        using (var r = new VideoRecoveryRig())
        {
            float previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f; r.Set("preparationTimeout", 0.05f); r.Seed(1);
                r.Manager.StartCoroutine((IEnumerator)r.Call("WatchPlayback", 1, 100d));
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.IsFalse(r.Manager.isPlaying); Assert.IsFalse(r.Image.enabled);
                Assert.AreEqual("Failed", r.Call("GetResult", 1).ToString());
            }
            finally { Time.timeScale = previousScale; }
        }
    }
    public static IEnumerator SkipDisableAndStaleSessionAreSafe()
    {
        using (var r = new VideoRecoveryRig())
        {
            r.Seed(1); r.Manager.SkipVideo(); Assert.AreEqual("Skipped", r.Call("GetResult", 1).ToString());
            r.Seed(2); r.Call("CancelPlayback", 1); Assert.IsTrue(r.Manager.isPlaying);
            r.Root.SetActive(false); Assert.IsFalse(r.Manager.isPlaying); Assert.IsFalse(r.Image.enabled);
            Assert.AreEqual("Cancelled", r.Call("GetResult", 2).ToString()); yield return null;
        }
    }
    public static IEnumerator ErrorAndCompletionHidePresentation()
    {
        using (var r = new VideoRecoveryRig())
        {
            r.Seed(1); r.Call("OnVideoError", r.Player, "test decode failure");
            Assert.IsFalse(r.Manager.isPlaying); Assert.AreEqual("Failed", r.Call("GetResult", 1).ToString());
            r.Seed(2); r.Manager.OnVideoFinished(r.Player);
            Assert.IsFalse(r.Manager.isPlaying); Assert.IsFalse(r.Image.enabled);
            Assert.AreEqual("Completed", r.Call("GetResult", 2).ToString()); yield return null;
        }
    }
    public static IEnumerator MissingVideoWarpCompletesAndRestoresControls()
    {
        return WarpFailureCase(false);
    }
    public static IEnumerator DisabledManagerCancelsWarpWithoutMoving()
    {
        return WarpFailureCase(true);
    }
    private static IEnumerator WarpFailureCase(bool cancel)
    {
        using (var r = new MovementRig(true))
        {
            var previous = DungeonManager.instance; DungeonManager.instance = null;
            var previousVideo = VideoPlayManager.instance; VideoPlayManager.instance = null;
            var previousCount = GameEvent.OnBossRoomEnterCount; GameEvent.OnBossRoomEnterCount = null;
            var go = new GameObject("WarpRecoveryTest"); go.SetActive(false); var manager = go.AddComponent<DungeonManager>();
            var ui = new GameObject("WarpRecoveryUI"); ui.transform.SetParent(go.transform);
            var destination = new GameObject("WarpRecoveryDestination"); var origin = r.Model.transform.position;
            destination.transform.position = origin + Vector3.right * 2f;
            var data = new WarpData { hasVideo = true, targetPos = destination.transform };
            try
            {
                manager.dungeonUI = ui; manager.isEnterStart = false;
                InventoryTransferRig.Set(manager, "warpDatas", new List<WarpData> { data });
                InventoryTransferRig.Set(manager, "warpCountdownDuration", cancel ? 5f : 0f);
                InventoryTransferRig.Set(manager, "warpTransitionDelay", 0f);
                go.SetActive(true); yield return null;
                InventoryTransferRig.Set(manager, "_model", r.Model);
                manager.WarpPlayer(0); Assert.IsTrue((bool)typeof(CharacterModel).GetProperty("IsExternalControlLocked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Model));
                manager.WarpPlayer(0);
                if (cancel) go.SetActive(false);
                yield return new WaitForSecondsRealtime(0.1f);
                Assert.IsFalse((bool)typeof(CharacterModel).GetProperty("IsExternalControlLocked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Model));
                Assert.IsTrue(r.Model.canMove && r.Model.canAttack && r.Model.canSkill);
                Assert.IsFalse(data.hasPlayed);
                Assert.Less(Vector3.Distance(r.Model.transform.position, cancel ? origin : destination.transform.position), 0.2f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(destination);
                DungeonManager.instance = previous; VideoPlayManager.instance = previousVideo;
                GameEvent.OnBossRoomEnterCount = previousCount;
            }
        }
    }
}
