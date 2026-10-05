using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class Boss60CorrectionsCases
{
    public const string MiddlePrefab = "Assets/03. Prefab/Enemy/Boss/D1/Middle/ClockworkBatBoss.prefab";
    public static void MiddleBossUsesAnimatedBatAndCombatCollider()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MiddlePrefab);
        Assert.IsNotNull(prefab.GetComponent<D1_MiddleBoss>());
        Assert.AreSame(AssetDatabase.LoadAssetAtPath<EnemyStatSO>("Assets/02. Scripts/Enemy/SO/D1_MiddleBoss.asset"), prefab.GetComponent<D1_MiddleBoss>().statSO);
        Assert.IsNull(prefab.GetComponent<MeshRenderer>(), "No capsule dummy remains on the cutscene boss");
        var animator = prefab.GetComponentsInChildren<Animator>(true).Single();
        Assert.AreSame(prefab.transform, animator.transform);
        Assert.IsNotNull(animator.runtimeAnimatorController);
        Assert.IsFalse(animator.applyRootMotion);
        Assert.AreEqual(1, prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length);
        Assert.IsTrue(prefab.GetComponentsInChildren<Transform>(true).All(t => t.gameObject.layer == LayerMask.NameToLayer("Enemy")));
        Assert.Greater(prefab.GetComponent<CapsuleCollider>().height * prefab.transform.localScale.y, 3f);
        var clips = animator.runtimeAnimatorController.animationClips;
        Assert.AreEqual(3, clips.Length);
        Assert.IsTrue(clips.Single(c => c.name == "ClockworkBatIdle").isLooping);
        Assert.IsFalse(clips.Single(c => c.name == "ClockworkBatCast").isLooping);
        Assert.Greater(clips.Single(c => c.name == "ClockworkBatDeath").length, .7f);
    }
}

public sealed class FinalReturnDelayProbe : BossPatternBase
{
    public int Executions;
    public FinalReturnDelayProbe() { weight = 1f; }
    public override bool IsReady(BossModel boss, Transform player) => true;
    public override void Execute(BossModel boss) { Executions++; }
}

public static class Boss60CorrectionsRuntimeCases
{
    public static IEnumerator RealJumpInteractionEndsLoopOnlyAtLanding()
    {
        foreach (float duration in new[] { 0f, .001f, .15f, 1f })
        using (var r = new MovementRig(true))
        {
            var controller = (AnimatorController)r.Model.Anim.runtimeAnimatorController;
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("JumpEnd", AnimatorControllerParameterType.Trigger);
            var state = controller.layers[0].stateMachine.AddState("JumpLoop");
            var clip = new AnimationClip();
            clip.SetCurve("", typeof(Transform), "localScale.x", AnimationCurve.Linear(0, 1, 1, 1));
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings); state.motion = clip;
            var entry = controller.layers[0].stateMachine.AddAnyStateTransition(state);
            entry.hasExitTime = false; entry.duration = 0f; entry.canTransitionToSelf = false;
            entry.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            var go = new GameObject("ActualInteractionJump"); go.SetActive(false); go.layer = 30;
            go.transform.position = r.Model.transform.position;
            go.AddComponent<CapsuleCollider>().isTrigger = true;
            var jump = go.AddComponent<JumpObject>();
            var landing = new GameObject("ActualJumpLanding");
            var apex = new GameObject("ActualJumpApex");
            landing.transform.position = r.Model.transform.position + Vector3.right;
            apex.transform.position = r.Model.transform.position + Vector3.up * 3f;
            try
            {
                typeof(InteractableObject).GetField("_interactType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(jump, InteractType.Jump);
                jump.targetPos = landing.transform; jump.apexPos = apex.transform;
                jump.jumpDuration = duration; jump.jumpCurve = AnimationCurve.Linear(0, 0, 1, 1);
                jump.SetLock(false); go.SetActive(true); jump.EnableInteract(r.Model.transform);
                r.Model.interactableLayer = 1 << 30; r.Model.interactableDistance = 2f;
                Physics.SyncTransforms(); r.Model.TryInteract();
                Assert.IsFalse(r.Model.canMove || r.Agent.enabled);
                var active = typeof(JumpObject).GetField("jumpingPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
                float timeout = Time.realtimeSinceStartup + 3f;
                while (active.GetValue(jump) != null && Time.realtimeSinceStartup < timeout)
                {
                    Assert.IsFalse(r.Model.canMove || r.Model.canAttack || r.Model.canSkill || r.Agent.enabled);
                    yield return null;
                }
                Assert.IsNull(active.GetValue(jump));
                Assert.Less(Vector3.Distance(landing.transform.position, r.Model.transform.position), .1f);
                Assert.IsTrue(r.Model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
                Assert.IsFalse(r.Model.Anim.IsInTransition(0));
                Assert.IsTrue(r.Model.canMove && r.Model.canAttack && r.Model.canSkill && r.Agent.isOnNavMesh);
                yield return new WaitForSeconds(.1f);
                Assert.IsTrue(r.Model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "TryInteract must not send a late jump after landing");
                r.Controller.RequestMove(landing.transform.position + Vector3.forward * 2f);
                yield return new WaitForSeconds(.2f);
                Assert.Greater(Vector3.Distance(landing.transform.position, r.Model.transform.position), .3f);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(landing); UnityEngine.Object.DestroyImmediate(apex); UnityEngine.Object.DestroyImmediate(clip); }
        }
    }

    public static IEnumerator SwingAllowsMovementAndBossIgnoresDamage()
    {
        using (var r = new FinalBossTransitionRig())
        {
            Vector3 initial = r.Player.Model.transform.position;
            int hp = r.Boss.Stat.curHp;
            r.Boss.EnableCounter(); r.Start();
            float timeout = Time.realtimeSinceStartup + 2f;
            while (!r.Boss.isImmunity && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsTrue(r.Boss.isImmunity); Assert.IsFalse(r.Boss.CanCounter);
            r.Boss.Damaged(new SDamageInfo { damage = 100, source = r.Player.Model.gameObject });
            Assert.AreEqual(hp, r.Boss.Stat.curHp);
            r.Player.Controller.RequestMove(initial + Vector3.forward * 3f);
            yield return new WaitForSeconds(.5f);
            Assert.IsTrue(r.Player.Model.canMove && r.Player.Model.canAttack && r.Player.Model.canSkill);
            Assert.Greater(Vector3.Distance(initial, r.Player.Model.transform.position), .5f);
            Assert.IsTrue(r.Boss.isImmunity);
        }
    }

    public static IEnumerator MusicResumesOnlyAfterMiddleBossDeathAndReturn()
    {
        using (var r = new Jester60IntroductionRig())
        {
            r.Transition.Start();
            float timeout = Time.realtimeSinceStartup + 15f;
            while (r.Director.state != UnityEngine.Playables.PlayState.Playing && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.AreEqual(UnityEngine.Playables.PlayState.Playing, r.Director.state);
            Assert.IsTrue(r.Bgm.mute && !r.Bgm.isPlaying);
            r.Director.time = 25.8d; r.Director.Evaluate();
            timeout = Time.realtimeSinceStartup + 3f;
            while (r.Transition.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < timeout) yield return null;
            var middle = (D1_MiddleBoss)r.Transition.Get("_special3MiddleBoss"); Assert.IsNotNull(middle);
            yield return Jester60IntroductionRuntimeCases.WaitForCombat(r);
            Assert.IsTrue(r.Bgm.mute && !r.Bgm.isPlaying && r.Transition.Boss.isImmunity);
            middle.Damaged(new SDamageInfo { damage = 10000, source = r.Transition.Player.Model.gameObject });
            yield return new WaitForSecondsRealtime(.2f);
            Assert.IsTrue(middle.IsDead && r.Bgm.mute && !r.Bgm.isPlaying && r.Transition.Boss.isImmunity);
            timeout = Time.realtimeSinceStartup + 5f;
            while (r.Transition.Boss.isImmunity && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsFalse(r.Transition.Boss.isImmunity);
            Assert.IsTrue(r.Transition.Boss.isDoingSpecial && r.Transition.Player.Model.canMove && r.Bgm.isPlaying && !r.Bgm.mute);
            var probe = new FinalReturnDelayProbe();
            typeof(BossModel).GetField("normalPatterns", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(r.Transition.Boss, new System.Collections.Generic.List<BossPatternBase> { probe });
            var update = typeof(BossModel).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            update.Invoke(r.Transition.Boss, null); Assert.AreEqual(0, probe.Executions);
            int beforeReturnHit = r.Transition.Boss.Stat.curHp;
            r.Transition.Boss.Damaged(new SDamageInfo { damage = 1, source = r.Transition.Player.Model.gameObject });
            Assert.Less(r.Transition.Boss.Stat.curHp, beforeReturnHit, "The return delay does not extend boss immunity");
            float returnedAt = Time.time;
            yield return new WaitForSeconds(2.6f);
            Assert.IsTrue(r.Transition.Boss.isDoingSpecial);
            update.Invoke(r.Transition.Boss, null); Assert.AreEqual(0, probe.Executions);
            Assert.IsNull(typeof(BossModel).GetField("currentPattern", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Transition.Boss));
            Assert.IsTrue(r.Transition.Boss.Agent.isStopped);
            timeout = Time.realtimeSinceStartup + 2f;
            while (r.Transition.Boss.isDoingSpecial && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.GreaterOrEqual(Time.time - returnedAt, 2.9f);
            update.Invoke(r.Transition.Boss, null); Assert.AreEqual(1, probe.Executions);
            r.Transition.Boss.OnPatternEnd();
            Assert.IsFalse(r.Transition.Boss.isDoingSpecial || r.Transition.Boss.isImmunity || r.Bgm.mute);
            Assert.IsTrue(r.Bgm.isPlaying && r.Transition.Player.Model.canMove);
            Assert.Less(Vector3.Distance(r.Transition.Player.Model.transform.position, r.Transition.Boss.playerStartPos.position), .1f);
            Assert.IsNotNull(middle, "The shared cinematic actor survives encounter completion");
            Assert.AreSame(middle, r.Transition.Boss.Special3.sceneMiddleBoss);
            Assert.IsFalse(middle.gameObject.activeInHierarchy);
            r.Transition.Start();
            timeout = Time.realtimeSinceStartup + 15f;
            while (r.Director.state != UnityEngine.Playables.PlayState.Playing && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.AreEqual(UnityEngine.Playables.PlayState.Playing, r.Director.state);
            Assert.IsTrue(middle.gameObject.activeInHierarchy, "The same actor is visible again after a completed encounter");
            Assert.IsFalse(middle.IsDead || middle.enabled || middle.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled);
            Assert.AreEqual(middle.Stat.maxHp, middle.Stat.curHp);
            r.Director.time = 25.8d; r.Director.Evaluate();
            timeout = Time.realtimeSinceStartup + 3f;
            while (r.Transition.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < timeout) yield return null;
            yield return Jester60IntroductionRuntimeCases.WaitForCombat(r);
            Assert.AreSame(middle, r.Transition.Get("_special3MiddleBoss"));
            Assert.IsTrue(middle.enabled && middle.Agent.isOnNavMesh);
            r.Transition.Boss.ForceStopCurrentAction();
        }
    }

    public static IEnumerator MusicCancellationPreservesOriginalPlaybackAndMute()
    {
        foreach (bool paused in new[] { false, true })
        using (var r = new Jester60IntroductionRig())
        {
            if (paused) { r.Bgm.mute = true; r.Bgm.Pause(); }
            typeof(D1_FinalBoss).GetMethod("PauseSpecial3Bgm", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(r.Transition.Boss, null);
            Assert.IsTrue(r.Bgm.mute && !r.Bgm.isPlaying);
            r.Transition.Boss.ForceStopCurrentAction(); yield return null;
            Assert.AreEqual(paused, r.Bgm.mute);
            Assert.AreEqual(!paused, r.Bgm.isPlaying);
        }
    }

    public static IEnumerator ActualBatCastsOncePerTargetAndStopsOnDeath()
    {
        using (var r = new MovementRig(true))
        {
            r.Model.gameObject.AddComponent<CapsuleCollider>();
            var extra = new GameObject("ExtraPlayerCollider"); extra.transform.SetParent(r.Model.transform, false); extra.AddComponent<SphereCollider>();
            var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab), r.Model.transform.position + Vector3.forward * 1.5f, Quaternion.identity);
            var anchor = new GameObject("BatSpawn"); anchor.transform.position = go.transform.position;
            var boss = go.GetComponent<D1_MiddleBoss>(); boss.bossSpawnPoint = anchor.transform; boss.isInField = false;
            typeof(EnemyBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(boss, r.Model);
            try
            {
                r.Model.Stat.Stat.curHp = 100f;
                float hp = r.Model.Stat.Stat.curHp;
                yield return new WaitForSeconds(.3f);
                Assert.IsTrue(boss.Anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"), "position=" + boss.transform.position + " player=" + r.Model.transform.position + " started=" + boss.isCombatStarted + " enabled=" + boss.enabled + " field=" + boss.isInField + " state=" + boss.Anim.GetCurrentAnimatorStateInfo(0).shortNameHash);
                Assert.IsTrue(boss.Agent.isOnNavMesh);
                CaptureBat(boss, "Temp/ClockworkBatCombat.png");
                yield return new WaitForSeconds(1.1f);
                Assert.AreEqual(hp - boss.Stat.attackDamage, r.Model.Stat.Stat.curHp, "Multiple player colliders receive one hit");
                boss.Damaged(new SDamageInfo { damage = 100000, source = r.Model.gameObject });
                yield return new WaitForSeconds(.2f);
                Assert.IsTrue(boss.IsDead && boss.Anim.GetCurrentAnimatorStateInfo(0).IsName("Die"));
                CaptureBat(boss, "Temp/ClockworkBatDeath.png");
                float after = r.Model.Stat.Stat.curHp;
                yield return new WaitForSecondsRealtime(3.1f);
                Assert.AreEqual(after, r.Model.Stat.Stat.curHp); Assert.IsFalse(go.activeSelf);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(anchor); }
        }
    }
    private static void CaptureBat(D1_MiddleBoss boss, string path)
    {
        var cameraObject = new GameObject("BatVerificationCamera");
        var lightObject = new GameObject("BatVerificationLight");
        var camera = cameraObject.AddComponent<Camera>(); camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.12f, .14f, .18f);
        camera.cullingMask = LayerMask.GetMask("Enemy"); camera.orthographic = true;
        var bounds = boss.GetComponentInChildren<SkinnedMeshRenderer>().bounds;
        camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * 1.25f;
        camera.transform.position = bounds.center + Vector3.back * (bounds.size.magnitude + 5f);
        camera.transform.LookAt(bounds.center); camera.farClipPlane = 100f;
        var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);
        var prior = RenderTexture.active; var render = RenderTexture.GetTemporary(640, 640, 24);
        var texture = new Texture2D(640, 640, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = render; camera.Render(); RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); texture.Apply();
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = prior; RenderTexture.ReleaseTemporary(render);
            UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(lightObject);
        }
    }
}
