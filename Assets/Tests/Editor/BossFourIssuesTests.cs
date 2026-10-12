using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class BossFourIssuesCases
{
    const string EndPrefab = "Assets/03. Prefab/Dungeon/Ending/DungeonEndingCutscene.prefab";
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Flags).Invoke(target,args);
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
                yield return null;Physics.SyncTransforms();
                var patterns=(System.Collections.Generic.List<BossPatternBase>)typeof(BossModel).GetField("normalPatterns",Flags).GetValue(boss);
                Assert.AreEqual(3,patterns.Count);
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
