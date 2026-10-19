using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;
using UnityEngine.AI;

public static class BossFourIssuesCases
{
    const string EndPrefab = "Assets/03. Prefab/Dungeon/Ending/DungeonEndingCutscene.prefab";
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Flags).Invoke(target,args);

    public static void CoincidentPatternTargetsKeepFacing()
    {
        using (var r = new BossLifecycleRig(false))
        {
            var method = typeof(BossPatternBase).GetMethod("SmoothLookAtRoutine", Flags);
            foreach (float duration in new[] { 0f, .2f })
            foreach (var offset in new[] { Vector3.zero, Vector3.up * 10f, Vector3.right * .00001f })
            {
                var facing = Quaternion.Euler(0, 90, 0); r.Boss.transform.rotation = facing;
                var routine = (IEnumerator)method.Invoke(r.Pattern, new object[] { r.Boss, r.Boss.transform.position + offset, duration });
                Assert.IsFalse(routine.MoveNext(), "No horizontal direction must finish without a rotation or a wait");
                Assert.Less(Quaternion.Angle(facing, r.Boss.transform.rotation), .001f);
            }
            var normal = (IEnumerator)method.Invoke(r.Pattern, new object[] { r.Boss, r.Boss.transform.position + Vector3.left, 0f });
            Assert.IsFalse(normal.MoveNext());
            Assert.Less(Quaternion.Angle(Quaternion.LookRotation(Vector3.left), r.Boss.transform.rotation), .001f);
        }
    }

    public static IEnumerator OverlappingPatternTargetsKeepFacingDuringPlayback()
    {
        using (var r = new BossLifecycleRig(true))
        {
            var facing = Quaternion.Euler(0, 90, 0); r.Boss.transform.rotation = facing;
            var method = typeof(BossPatternBase).GetMethod("SmoothLookAtRoutine", Flags);
            r.Boss.StartCoroutine((IEnumerator)method.Invoke(r.Pattern, new object[] { r.Boss, r.Boss.transform.position, .2f }));
            yield return new WaitForSeconds(.25f);
            Assert.Less(Quaternion.Angle(facing, r.Boss.transform.rotation), .001f);
            r.Boss.StartCoroutine((IEnumerator)method.Invoke(r.Pattern, new object[] { r.Boss, r.Boss.transform.position + Vector3.left + Vector3.up * 10f, .2f }));
            yield return new WaitForSeconds(.25f);
            Assert.Less(Quaternion.Angle(Quaternion.LookRotation(Vector3.left), r.Boss.transform.rotation), .001f);
        }
    }

    public static IEnumerator ChessCutsceneLocksControlsAndWarpsBeforeCameraCut()
    {
        using (var r = new ChessRegressionRig())
        {
            r.Start(); yield return r.WaitFor(() => r.OwnsLock, "Chess acquires its player control lock");
            var model = r.Transition.Player.Model;
            Assert.IsFalse(r.Transition.Director.gameObject.activeSelf, "Lock precedes cutscene activation");
            model.SetCanMove(); model.SetCanAttack(); model.SetCanSkill();
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill || model.canUse);
            Assert.IsFalse(r.Transition.Player.Agent.enabled);
            r.Transition.Player.Controller.RequestMove(model.transform.position + Vector3.forward * 3f);
            Assert.IsFalse(model.canMove);
            yield return r.WaitFor(() => r.Transition.Director.state == PlayState.Playing, "Chess Timeline plays");
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill || model.canUse);
            model.ChangeCam(1, true);
            Assert.AreSame(r.OtherCamera, r.Brain.ActiveVirtualCamera);
            yield return r.WaitFor(() => Vector3.Distance(model.transform.position, r.StartPoint.position) < .15f, "Player reaches the chess start");
            Assert.AreSame(r.FollowCamera, r.Brain.ActiveVirtualCamera);
            Assert.IsFalse(r.Brain.IsBlending);
            Assert.Less(Vector3.Distance(r.Output.transform.position, model.transform.position + r.FollowOffset), .15f, "First post-warp output must use the destination");
            yield return r.WaitFor(() => model.canMove, "Normal completion restores controls", 2f);
            Assert.IsTrue(model.canAttack && model.canSkill && model.canUse && r.Transition.Player.Agent.isOnNavMesh);
            Assert.IsFalse(r.OwnsLock);
            r.Transition.Boss.ForceStopCurrentAction(); // Stop before the full chess board starts.
        }
    }

    public static IEnumerator ChessCancellationRestoresOnlyOwnedControls()
    {
        foreach (int phase in new[] { 0, 1, 2 })
        using (var r = new ChessRegressionRig())
        {
            r.Start(); yield return r.WaitFor(() => r.OwnsLock, "Chess locks player");
            if (phase == 1) yield return r.WaitFor(() => r.Transition.Director.state == PlayState.Playing, "Cancel during playback");
            else Assert.IsFalse(r.Transition.Director.gameObject.activeSelf, "Cancel before cutscene activation");
            var model = r.Transition.Player.Model;
            if (phase == 2) model.isDie = true;
            r.Transition.Boss.ForceStopCurrentAction(); r.Transition.Boss.ForceStopCurrentAction();
            Assert.IsFalse(r.OwnsLock);
            Assert.IsFalse(r.Transition.Director.gameObject.activeSelf);
            if (phase == 2)
                Assert.IsFalse(model.canMove || model.canAttack || model.canSkill || model.canUse || r.Transition.Player.Agent.enabled, "Do not release death/respawn controls");
            else
                Assert.IsTrue(model.canMove && model.canAttack && model.canSkill && model.canUse && r.Transition.Player.Agent.isOnNavMesh);
            yield return new WaitForSeconds(.6f);
            Assert.IsFalse(r.Transition.Director.gameObject.activeSelf, "Cancelled work cannot start late");
        }
    }

    public static IEnumerator ChessWaitsForExistingControlLock()
    {
        using (var r = new ChessRegressionRig())
        {
            var model = r.Transition.Player.Model;
            model.SetControlable(false); r.Start();
            yield return r.WaitFor(() => !r.Transition.Boss.bossMeshs[0].enabled, "Swing presentation finishes");
            yield return new WaitForSeconds(.7f);
            Assert.IsFalse(r.OwnsLock || r.Transition.Director.gameObject.activeSelf);
            r.Transition.Boss.ForceStopCurrentAction();
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill || model.canUse || r.Transition.Player.Agent.enabled, "An existing external lock belongs to its original owner");
            model.SetControlable(true);
        }
    }

    public static void BatAnimationsLeaveGameplayRootUnbound()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab);
        foreach (var clip in prefab.GetComponent<Animator>().runtimeAnimatorController.animationClips.Distinct())
            Assert.IsFalse(AnimationUtility.GetCurveBindings(clip).Any(b => b.path == "" && b.type == typeof(Transform)), clip.name + " must animate the visual skeleton only");
    }

    public static IEnumerator BatRecognizesPlayerAndChasesIntoMeleeRange()
    {
        using (var r = new MovementRig(true))
        {
            Vector3 origin = r.Model.transform.position;
            var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab), origin + Vector3.right * 15f, Quaternion.identity);
            var anchor = new GameObject("BatDetectionSpawn"); anchor.transform.position = go.transform.position;
            var boss = go.GetComponent<D1_MiddleBoss>();
            boss.bossSpawnPoint = anchor.transform; boss.isInField = true; boss.isCombatStarted = false;
            try
            {
                Assert.IsTrue(r.Agent.Warp(origin + Vector3.left * 12f));
                yield return new WaitForSeconds(.15f);
                Assert.IsFalse(boss.isCombatStarted, "Outside the detection radius");
                r.Agent.enabled = false; r.Model.transform.position = go.transform.position + Vector3.up * 100f;
                yield return new WaitForSeconds(.15f);
                Assert.IsFalse(boss.isCombatStarted, "The elevated entrance must not trigger combat");
                r.Model.transform.position = origin; r.Agent.enabled = true; Assert.IsTrue(r.Agent.Warp(origin));
                var patterns = (System.Collections.Generic.List<BossPatternBase>)typeof(BossModel).GetField("normalPatterns", Flags).GetValue(boss);
                patterns.RemoveAll(p => !p.patternName.EndsWith("Swipe"));
                float meleeRange = patterns.Single().range;
                Vector3 initial = boss.transform.position;
                yield return new WaitForSeconds(.5f);
                Assert.AreSame(r.Model, boss.Target, "Acquire the player without damage or injected target");
                Assert.IsTrue(boss.isCombatStarted);
                Assert.Greater(Vector3.Distance(initial, boss.transform.position), .4f, "position=" + boss.transform.position + " next=" + boss.Agent.nextPosition + " speed=" + boss.Agent.speed + " velocity=" + boss.Agent.velocity + " stopped=" + boss.Agent.isStopped + " remaining=" + boss.Agent.remainingDistance + " path=" + boss.Agent.pathStatus + " pending=" + boss.Agent.pathPending);
                Assert.Less(boss.Agent.stoppingDistance, meleeRange);
                float deadline = Time.realtimeSinceStartup + 5f;
                while (Vector3.Distance(boss.transform.position, origin) > meleeRange && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.LessOrEqual(Vector3.Distance(boss.transform.position, origin), meleeRange);
                r.Model.isDie = true; yield return null;
                Assert.IsTrue(boss.Agent.isStopped, "Stop pursuing a dead player");
                boss.Agent.enabled = false;
                yield return null; // Disabled agents must not produce NavMesh exceptions.
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(anchor); }
        }
    }

    public static IEnumerator BatSwoopHitsOnceCanBeDodgedAndCancelsCleanly()
    {
        using (var r = new MovementRig(true))
        {
            r.Model.gameObject.AddComponent<CapsuleCollider>();
            var extra = new GameObject("SwoopExtraCollider"); extra.transform.SetParent(r.Model.transform, false); extra.AddComponent<SphereCollider>();
            Vector3 origin = r.Model.transform.position;
            var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab), origin + Vector3.right * 8f, Quaternion.identity);
            var anchor = new GameObject("BatSwoopSpawn"); anchor.transform.position = go.transform.position;
            var boss = go.GetComponent<D1_MiddleBoss>(); boss.bossSpawnPoint = anchor.transform; boss.isInField = true; boss.isCombatStarted = false; boss.isImmunity = true;
            try
            {
                yield return null; boss.enabled = false; boss.isImmunity = false;
                typeof(D1_MiddleBoss).GetField("swoopWindup", Flags).SetValue(boss, .15f);
                typeof(D1_MiddleBoss).GetField("swoopDuration", Flags).SetValue(boss, .2f);
                typeof(D1_MiddleBoss).GetField("swoopRecovery", Flags).SetValue(boss, .1f);
                var patterns = (System.Collections.Generic.List<BossPatternBase>)typeof(BossModel).GetField("normalPatterns", Flags).GetValue(boss);
                var swoop = patterns.Single(p => p.patternName.EndsWith("Swoop"));
                r.Model.Stat.Stat.curHp = 1000f; Physics.SyncTransforms();
                Assert.IsTrue(swoop.IsReady(boss, r.Model.transform));
                swoop.Execute(boss); yield return new WaitForSeconds(.65f);
                Assert.AreEqual(1000f - Mathf.RoundToInt(boss.Stat.attackDamage * 1.2f), r.Model.Stat.Stat.curHp, "position=" + boss.transform.position + " next=" + boss.Agent.nextPosition + " player=" + r.Model.transform.position + " objects=" + boss.patternObjects.Count);
                Assert.AreEqual(0, boss.patternObjects.Count);
                Assert.Less(Vector3.Distance(origin, boss.transform.position), 1f);
                Assert.IsFalse(swoop.IsReady(boss, r.Model.transform), "Cooldown prevents immediate reuse");

                Assert.IsTrue(boss.Agent.Warp(anchor.transform.position));
                float hp = r.Model.Stat.Stat.curHp;
                swoop.Execute(boss); Assert.AreEqual(1, boss.patternObjects.Count);
                Assert.IsTrue(r.Agent.Warp(origin + Vector3.forward * 6f)); Physics.SyncTransforms();
                yield return new WaitForSeconds(.65f);
                Assert.AreEqual(hp, r.Model.Stat.Stat.curHp, "The strike stays at its announced location");
                Assert.AreEqual(0, boss.patternObjects.Count);
                Assert.IsTrue(boss.Agent.Warp(anchor.transform.position));
                swoop.Execute(boss); boss.ForceStopCurrentAction();
                Vector3 stopped = boss.transform.position;
                yield return new WaitForSeconds(.65f);
                Assert.Less(Vector3.Distance(stopped, boss.transform.position), .1f, "Only NavMesh surface height correction is allowed after cancellation"); Assert.AreEqual(hp, r.Model.Stat.Stat.curHp);
                Assert.AreEqual(0, boss.patternObjects.Count);
                swoop.Execute(boss); yield return new WaitForSeconds(.05f);
                boss.Damaged(new SDamageInfo { damage = 100000, source = r.Model.gameObject });
                yield return new WaitForSeconds(.65f);
                Assert.AreEqual(hp, r.Model.Stat.Stat.curHp); Assert.AreEqual(0, boss.patternObjects.Count);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(anchor); }
        }
    }

    public static IEnumerator CameraCutClearsBlendAndSnapsToWarpedPlayer()
    {
        using (var r = new MovementRig(true))
        {
            var cameraObject = new GameObject("CameraCutOutput");
            var first = new GameObject("CameraCutFollow"); var second = new GameObject("CameraCutOther");
            try
            {
                var camera = cameraObject.AddComponent<Camera>(); var brain = cameraObject.AddComponent<CinemachineBrain>();
                brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 2f);
                var follow = first.AddComponent<CinemachineVirtualCamera>(); follow.Follow = r.Model.transform;
                var body = follow.AddCinemachineComponent<CinemachineTransposer>();
                body.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
                body.m_FollowOffset = new Vector3(0, 6, -8); body.m_XDamping = body.m_YDamping = body.m_ZDamping = 2f;
                var other = second.AddComponent<CinemachineVirtualCamera>(); second.transform.position = Vector3.one * -300f;
                r.Model.mainCam = camera; r.Model.cams = new[] { follow, other };
                r.Model.ChangeCam(0, true); yield return null;
                r.Model.ChangeCam(1); yield return null;
                Assert.IsTrue(brain.IsBlending);
                r.Agent.Warp(r.Model.transform.position + Vector3.right * 10f);
                r.Model.ChangeCam(0, true);
                Assert.IsFalse(brain.IsBlending);
                Assert.AreSame(follow, brain.ActiveVirtualCamera);
                Assert.Less(Vector3.Distance(camera.transform.position, r.Model.transform.position + body.m_FollowOffset), .1f);
                follow.gameObject.SetActive(false);
                r.Model.ChangeCam(0, true);
                Assert.IsTrue(follow.gameObject.activeSelf);
                Assert.AreSame(follow, brain.ActiveVirtualCamera);
                Assert.DoesNotThrow(() => r.Model.ChangeCam(-1, true));
                Assert.DoesNotThrow(() => r.Model.ChangeCam(10, true));
            }
            finally { UnityEngine.Object.DestroyImmediate(first); UnityEngine.Object.DestroyImmediate(second); UnityEngine.Object.DestroyImmediate(cameraObject); }
        }
    }

    public static IEnumerator CutsceneFirstFrameAndRetryUseTimelineCamera()
    {
        using (var r = new Jester60IntroductionRig())
        {
            yield return null;
            var camera = (Camera)Jester60IntroductionCases.Get(r.Binder, "presentationCamera");
            var brain = camera.GetComponent<CinemachineBrain>();
            for (int attempt = 0; attempt < 2; attempt++)
            {
                r.StartTimeline();
                Assert.IsTrue(camera.enabled && brain.enabled);
                Assert.IsNotNull(brain.ActiveVirtualCamera);
                Assert.IsTrue(brain.ActiveVirtualCamera.VirtualCameraGameObject.transform.IsChildOf(r.Intro.transform));
                Assert.Less(Vector3.Distance(camera.transform.position, brain.ActiveVirtualCamera.State.FinalPosition), .1f);
                r.Director.time = 18d; r.Director.Evaluate(); yield return null;
                r.Transition.Boss.ForceStopCurrentAction();
                Assert.IsFalse(camera.enabled || brain.enabled);
                Assert.IsTrue(r.GameplayCamera.enabled);
                yield return null;
            }
        }
    }
    public static void CameraRestoresOriginalFourShots()
    {
        var original=AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/Jester60/Jester60-Assets/CameraTimeline.playable").GetOutputTracks().SelectMany(t=>t.GetClips()).OrderBy(c=>c.start).ToArray();
        var runtime=AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Camera.playable").GetOutputTracks().SelectMany(t=>t.GetClips()).OrderBy(c=>c.start).ToArray();
        Assert.AreEqual(4,runtime.Length);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Jester60IntroductionCases.PrefabPath);
        var director=prefab.GetComponentsInChildren<PlayableDirector>(true).Single(d=>d.name=="Camera");
        for(int i=0;i<4;i++)
        {
            Assert.AreEqual(original[i].displayName,runtime[i].displayName);
            Assert.AreEqual(original[i].start,runtime[i].start,.001);
            Assert.AreEqual(i==3?26d-original[i].start:original[i].duration,runtime[i].duration,.001);
            var shot=(CinemachineShot)runtime[i].asset;
            var vc=shot.VirtualCamera.Resolve(director);
            Assert.IsNotNull(vc);Assert.AreEqual(runtime[i].displayName,vc.name);
            Assert.IsTrue(vc.transform.IsChildOf(prefab.transform));
        }
    }
    public static void EndingHasRealTimelineAndInactiveCamera()
    {
        var p=AssetDatabase.LoadAssetAtPath<GameObject>(EndPrefab);
        Assert.IsNotNull(p);
        var d=p.GetComponent<PlayableDirector>();Assert.AreEqual(7d,d.duration,.001);
        Assert.IsFalse(d.playOnAwake);
        foreach(var output in d.playableAsset.outputs)Assert.IsNotNull(d.GetGenericBinding(output.sourceObject));
        Assert.IsFalse(p.GetComponentInChildren<Camera>(true).gameObject.activeSelf);
        Assert.IsFalse(p.GetComponentInChildren<Canvas>(true).gameObject.activeSelf);
    }
    public static IEnumerator ScaleAndCameraRecoverAcrossRetries()
    {
        using(var r=new Jester60IntroductionRig())
        {
            var actor=r.Transition.Boss.Special3.sceneMiddleBoss;
            yield return null;
            for(int i=0;i<2;i++)
            {
                Call(actor,"PrepareForPresentation");Assert.AreEqual(Vector3.one*20f,actor.transform.localScale);
                r.StartTimeline();yield return null;
                var cd=r.Director.GetComponentsInChildren<PlayableDirector>(true).Single(d=>d.name=="Camera");
                var cam=(Camera)Jester60IntroductionCases.Get(r.Binder,"presentationCamera");
                r.Director.time=9d;r.Director.Evaluate();yield return null;
                cam.GetComponent<Cinemachine.CinemachineBrain>().ManualUpdate();
                Assert.AreEqual("Virtual Camera (3)",cam.GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera.Name);
                r.Transition.Boss.ForceStopCurrentAction();
                Call(r.Binder,"BeginEncounter");
                var spawn=r.Transition.Boss.Special3.middleBossSpawnPoint;
                Call(actor,"BeginCombat",spawn);yield return null;
                Assert.AreEqual(Vector3.one*10f,actor.transform.localScale);
                Assert.IsTrue(actor.Agent.isOnNavMesh);
                var capsule=actor.GetComponent<CapsuleCollider>();
                Assert.That(capsule.radius*actor.transform.lossyScale.x,Is.InRange(1.4f,1.6f));
                Assert.That(capsule.height*actor.transform.lossyScale.y,Is.InRange(15.5f,16.5f));
                Call(r.Binder,"FinishEncounter");
                Assert.AreEqual(Vector3.one*20f,actor.transform.localScale);
            }
        }
    }
    public static IEnumerator ShortAttacksHitOnceAndCancelCleanly()
    {
        using(var r=new MovementRig(true))
        {
            r.Model.gameObject.AddComponent<CapsuleCollider>();
            var extra=new GameObject("ExtraCollider");extra.transform.SetParent(r.Model.transform,false);extra.AddComponent<SphereCollider>();
            var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab),r.Model.transform.position+Vector3.back*1.5f,Quaternion.identity);
            var boss=go.GetComponent<D1_MiddleBoss>();boss.isInField=true;boss.isCombatStarted=false;
            typeof(EnemyBase).GetField("_target",Flags).SetValue(boss,r.Model);
            try
            {
                yield return null;boss.enabled=false;Physics.SyncTransforms();
                var patterns=(System.Collections.Generic.List<BossPatternBase>)typeof(BossModel).GetField("normalPatterns",Flags).GetValue(boss);
                Assert.AreEqual(4,patterns.Count);
                var swipe=patterns.Single(p=>p.patternName.Contains("Swipe"));
                var pulse=patterns.Single(p=>p.patternName.Contains("Pulse"));
                r.Model.Stat.Stat.curHp=1000f;float hp=r.Model.Stat.Stat.curHp;
                swipe.Execute(boss);yield return new WaitForSeconds(.6f);
                Assert.AreEqual(hp-Mathf.Max(1,Mathf.RoundToInt(boss.Stat.attackDamage*.7f)),r.Model.Stat.Stat.curHp);
                yield return new WaitForSeconds(.6f);Assert.AreEqual(0,boss.patternObjects.Count);
                hp=r.Model.Stat.Stat.curHp;
                pulse.Execute(boss);yield return new WaitForSeconds(1.6f);
                Assert.AreEqual(hp-2*Mathf.Max(1,Mathf.RoundToInt(boss.Stat.attackDamage*.55f)),r.Model.Stat.Stat.curHp);
                yield return new WaitForSeconds(.4f);Assert.AreEqual(0,boss.patternObjects.Count);
                hp=r.Model.Stat.Stat.curHp;
                swipe.Execute(boss);boss.ForceStopCurrentAction();yield return new WaitForSeconds(.6f);
                Assert.AreEqual(hp,r.Model.Stat.Stat.curHp);Assert.AreEqual(0,boss.patternObjects.Count);
                pulse.Execute(boss);yield return new WaitForSeconds(.1f);
                boss.Damaged(new SDamageInfo{damage=100000,source=r.Model.gameObject});
                yield return new WaitForSeconds(1.6f);Assert.AreEqual(hp,r.Model.Stat.Stat.curHp);Assert.AreEqual(0,boss.patternObjects.Count);
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
    public static IEnumerator EndingPlaysBeforeDepartureAndCancellationRestoresUI()
    {
        using(var r=new DungeonDepartureRig(true))
        {
            r.AddEndpoint();DungeonDepartureRig.Set(r.Endpoint,"returnDelay",0f);
            var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EndPrefab));
            go.transform.SetParent(r.Manager.transform,false);
            var d=go.GetComponent<PlayableDirector>();DungeonDepartureRig.Set(r.Endpoint,"endingDirector",d);
            var cameraPosition=go.GetComponentInChildren<Camera>(true).transform.position;
            r.Endpoint.transform.SetPositionAndRotation(Vector3.one*500f,Quaternion.Euler(0,90,0));
            r.Endpoint.transform.localScale=Vector3.one*3f;
            Assert.AreEqual(cameraPosition,go.GetComponentInChildren<Camera>(true).transform.position);
            yield return null;r.Manager.dungeonUI.SetActive(true);
            float previousScale=Time.timeScale;
            try
            {
                Time.timeScale=0f;r.Manager.OnSectorCleared(r.Final);
                yield return new WaitForSecondsRealtime(.25f);
                Assert.AreEqual(PlayState.Playing,d.state);
                Assert.IsFalse((bool)DungeonDepartureRig.Get(r.Endpoint,"sceneLoadStarted"));
                Assert.IsFalse(r.Manager.dungeonUI.activeSelf);
                Assert.IsTrue(go.GetComponentInChildren<Camera>(true).enabled);
                d.time=3d;d.Evaluate();yield return null;
                Assert.Greater(go.transform.Find("EndingOverlay/Caption").GetComponent<CanvasGroup>().alpha,.9f);
                r.Endpoint.enabled=false;yield return null;
                Assert.IsFalse(go.GetComponentInChildren<Camera>(true).gameObject.activeSelf);
                Assert.IsTrue(r.Manager.dungeonUI.activeSelf);
                Assert.IsNotNull(DungeonDepartureRig.Get(r.Manager,"dungeonOutRoutine"));
            }
            finally{Time.timeScale=previousScale;}
        }
    }
}
