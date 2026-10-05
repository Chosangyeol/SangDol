using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class FinalBossTransitionCases
{
    public static void ThresholdWaitsForCurrentPatternAndStartsOnce()
    {
        foreach (int hp in new[] { 61, 60, 59 })
        using (var rig = new BossLifecycleRig(false))
        {
            var special = new BossSpecialPattern { patternName = "중간 보스", hpPercent = 0.6f };
            rig.Boss.specialPatterns = new List<BossSpecialPattern> { special };
            rig.Boss.Stat.curHp = hp;
            typeof(BossModel).GetField("currentPattern", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(rig.Boss, rig.Pattern);
            var check = typeof(BossModel).GetMethod("HandleCheckSpecial", BindingFlags.Instance | BindingFlags.NonPublic);
            check.Invoke(rig.Boss, null);
            Assert.IsFalse(special.hasDone || rig.Boss.isDoingSpecial);
            rig.Boss.OnPatternEnd();
            check.Invoke(rig.Boss, null);
            Assert.AreEqual(hp <= 60, special.hasDone);
            Assert.AreEqual(hp <= 60, rig.Boss.isDoingSpecial);
            rig.Boss.isDoingSpecial = false;
            check.Invoke(rig.Boss, null);
            Assert.IsFalse(rig.Boss.isDoingSpecial, "An already completed threshold must not start twice.");
        }
    }
}

public sealed class FinalBossTransitionRig : IDisposable
{
    public readonly MovementRig Player = new MovementRig(true);
    public readonly D1_FinalBoss Boss;
    public readonly PlayableDirector Director;
    public readonly Animator Animator;
    private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
    private readonly VideoPlayManager previousVideo = VideoPlayManager.instance;
    private readonly Action<BossModel> previousBossState = GameEvent.OnBossStateChange;
    private readonly Action previousHidden = GameEvent.OnUIInvisable;
    private readonly Action previousVisible = GameEvent.OnMainUIviable;
    private readonly GameObject root;
    public FinalBossTransitionRig()
    {
        VideoPlayManager.instance = null;
        GameEvent.OnBossStateChange = null;
        GameEvent.OnUIInvisable = GameEvent.OnMainUIviable = null;
        Vector3 origin = Player.Model.transform.position;
        var center = Own(new GameObject("TransitionCenter"));
        center.transform.position = origin;
        var waiting = Own(new GameObject("TransitionWaiting"));
        waiting.transform.position = origin + Vector3.right * 5f;
        var spawn = Own(new GameObject("TransitionMiddleSpawn"));
        spawn.transform.position = origin + Vector3.right * 12f;
        var swing = Own(new GameObject("TransitionSwing"));
        swing.SetActive(false);
        var stat = Own(ScriptableObject.CreateInstance<EnemyStatSO>());
        stat.maxHp = 100;
        stat.attackRange = 2f;
        var middle = Own(new GameObject("TransitionMiddleTemplate"));
        middle.SetActive(false);
        middle.transform.position = spawn.transform.position;
        var middleBoss = middle.AddComponent<D1_MiddleBoss>();
        middleBoss.enabled = false;
        middleBoss.statSO = stat;
        middle.SetActive(true);

        var timelineRoot = Own(new GameObject("TransitionTimeline"));
        Director = timelineRoot.AddComponent<PlayableDirector>();
        Director.playOnAwake = false;
        var timeline = Own(ScriptableObject.CreateInstance<TimelineAsset>());
        timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
        timeline.fixedDuration = 0.5d;
        timeline.CreateTrack<ActivationTrack>(null, "Presentation");
        Director.playableAsset = timeline;
        Director.extrapolationMode = DirectorWrapMode.Loop;
        timelineRoot.SetActive(false);

        root = Own(new GameObject("TransitionFinalBoss"));
        root.SetActive(false);
        root.transform.position = origin + Vector3.left * 5f;
        root.AddComponent<NavMeshAgent>();
        Animator = root.AddComponent<Animator>();
        var controller = Own(new AnimatorController());
        controller.AddLayer("Base Layer");
        controller.AddParameter("Move", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Normal2End", AnimatorControllerParameterType.Trigger);
        var machine = controller.layers[0].stateMachine;
        var idle = machine.AddState("Idle");
        machine.defaultState = idle;
        var normal = machine.AddState("Normal2");
        var exit = normal.AddTransition(idle);
        exit.hasExitTime = false;
        exit.duration = 0.25f;
        exit.hasFixedDuration = true;
        exit.AddCondition(AnimatorConditionMode.If, 0f, "Normal2End");
        Animator.runtimeAnimatorController = controller;
        var mesh = root.AddComponent<SkinnedMeshRenderer>();
        Boss = root.AddComponent<D1_FinalBoss>();
        Boss.enabled = false;
        Boss.statSO = stat;
        Boss.center = Boss.playerStartPos = center.transform;
        Boss.bossMeshs = new[] { mesh };
        Boss.pattern3 = new D1_Final_Normal3Data { swingPrefab = swing, jumpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f) };
        Boss.Special3 = new D1_Final_Special3Data
        {
            prefab = middle, waitingArea = waiting.transform, middleBossSpawnPoint = spawn.transform,
            cutsceneDirector = Director
        };
        root.SetActive(true);
        typeof(EnemyBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Boss, Player.Model);
    }
    private T Own<T>(T obj) where T : UnityEngine.Object { owned.Add(obj); return obj; }
    public object Get(string name) => typeof(D1_FinalBoss).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Boss);
    public void Start()
    {
        Boss.isDoingSpecial = true;
        Boss.StartCoroutine((IEnumerator)typeof(D1_FinalBoss)
            .GetMethod("Special_MiddleBoss", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Boss, null));
    }
    public void Dispose()
    {
        Boss.ForceStopCurrentAction();
        for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        Player.Dispose();
        VideoPlayManager.instance = previousVideo;
        GameEvent.OnBossStateChange = previousBossState;
        GameEvent.OnUIInvisable = previousHidden;
        GameEvent.OnMainUIviable = previousVisible;
    }
}

public static class FinalBossTransitionRuntimeCases
{
    public static IEnumerator NormalAnimationFinishesBeforeSwingStarts()
    {
        using (var r = new FinalBossTransitionRig())
        {
            yield return null;
            r.Animator.Play("Normal2", 0, 0f);
            r.Animator.Update(0f);
            r.Animator.SetTrigger("Normal2End");
            r.Start();
            yield return null;
            Assert.AreEqual(0, r.Boss.patternObjects.Count);
            Assert.IsTrue(r.Boss.Agent.enabled);
            float timeout = Time.realtimeSinceStartup + 2f;
            while (r.Boss.patternObjects.Count == 0 && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsTrue(r.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            Assert.IsFalse(r.Animator.IsInTransition(0));
            Assert.Greater(r.Boss.patternObjects.Count, 0);
            Assert.IsFalse(r.Boss.Agent.enabled);
        }
    }

    public static IEnumerator SwingLandsAtCenterBeforeTimelineAndEncounter() => PresentationOrder(false);
    public static IEnumerator VideoFinishesBeforeTimelineAndEncounter() => PresentationOrder(true);

    private static IEnumerator PresentationOrder(bool withVideo)
    {
        using (var r = new FinalBossTransitionRig())
        using (var video = withVideo ? new VideoRecoveryRig() : null)
        {
            if (withVideo)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath("91118005997c0ee44813a892433aef45");
                r.Boss.Special3.cutsceneClip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(path);
                Assert.IsNotNull(r.Boss.Special3.cutsceneClip);
            }
            r.Start();
            float timeout = Time.realtimeSinceStartup + 15f;
            bool rose = false, hid = false, sawVideo = false;
            GameEvent.OnBossStateChange = boss => { if (boss == null) hid = true; };
            while (r.Director.state != PlayState.Playing && Time.realtimeSinceStartup < timeout)
            {
                rose |= r.Boss.transform.position.y > 10f;
                Assert.IsNull(r.Get("_special3MiddleBoss"));
                if (video != null && video.Manager.isPlaying)
                {
                    sawVideo = true;
                    Assert.Less(Vector3.Distance(r.Boss.center.position, r.Boss.transform.position), 0.01f);
                    Assert.AreNotEqual(PlayState.Playing, r.Director.state);
                    Assert.IsFalse(r.Player.Model.canMove);
                    video.Manager.SkipVideo();
                }
                if ((bool)r.Get("special3Moving"))
                {
                    Assert.IsTrue(r.Boss.isDoingSpecial && r.Boss.isImmunity);
                    if (!(bool)r.Get("special3ControlsLocked"))
                        Assert.IsTrue(r.Player.Model.canMove && r.Player.Model.canAttack && r.Player.Model.canSkill);
                }
                yield return null;
            }
            Assert.IsTrue(rose && hid, "The existing swing ascent and hiding presentation must run.");
            Assert.AreEqual(withVideo, sawVideo);
            Assert.AreEqual(PlayState.Playing, r.Director.state);
            Assert.Less(Vector3.Distance(r.Boss.center.position, r.Boss.transform.position), 0.01f);
            Assert.IsTrue(r.Boss.bossMeshs[0].enabled);
            Assert.IsNull(r.Get("_special3MiddleBoss"));
            Assert.IsFalse(r.Player.Model.canMove);
            timeout = Time.realtimeSinceStartup + 2f;
            while (r.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsNotNull(r.Get("_special3MiddleBoss"));
            Assert.IsFalse(r.Director.gameObject.activeSelf);
            Assert.AreEqual(DirectorWrapMode.Loop, r.Director.extrapolationMode);
            Assert.Less(Vector3.Distance(r.Boss.Special3.waitingArea.position, r.Player.Model.transform.position), 0.1f);
            Assert.IsTrue(r.Player.Model.canMove && r.Player.Model.canAttack && r.Player.Model.canSkill);
            Assert.IsTrue(r.Boss.isDoingSpecial && r.Boss.isImmunity);
        }
    }

    public static IEnumerator CancellationRestoresAirborneBossAndControls()
    {
        using (var r = new FinalBossTransitionRig())
        {
            r.Start();
            float timeout = Time.realtimeSinceStartup + 8f;
            while (r.Boss.transform.position.y < 10f && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.Greater(r.Boss.transform.position.y, 10f);
            r.Boss.ForceStopCurrentAction();
            r.Boss.ForceStopCurrentAction();
            yield return null;
            Assert.Less(Vector3.Distance(r.Boss.center.position, r.Boss.transform.position), 0.1f);
            Assert.IsTrue(r.Boss.Agent.enabled && r.Boss.bossMeshs[0].enabled);
            Assert.IsFalse(r.Boss.isDoingSpecial || r.Boss.isImmunity);
            Assert.IsTrue(r.Player.Model.canMove && r.Player.Model.canAttack && r.Player.Model.canSkill);
            Assert.AreEqual(0, r.Boss.patternObjects.Count);
            Assert.IsNull(r.Get("_special3MiddleBoss"));
            Assert.AreNotEqual(PlayState.Playing, r.Director.state);
        }
    }

    public static IEnumerator CancellationStopsTimelineWithoutStartingEncounter()
    {
        using (var r = new FinalBossTransitionRig())
        {
            // Exercise timeline cancellation separately from the already-covered swing movement.
            var routine = (IEnumerator)typeof(D1_FinalBoss).GetMethod("PlaySpecial3Timeline",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(r.Boss, null);
            r.Boss.StartCoroutine(routine);
            Assert.AreEqual(PlayState.Playing, r.Director.state);
            r.Boss.ForceStopCurrentAction();
            r.Boss.ForceStopCurrentAction();
            yield return null;
            Assert.AreNotEqual(PlayState.Playing, r.Director.state);
            Assert.IsFalse(r.Director.gameObject.activeSelf);
            Assert.AreEqual(DirectorWrapMode.Loop, r.Director.extrapolationMode);
            Assert.IsNull(r.Get("_special3MiddleBoss"));
        }
    }
}
