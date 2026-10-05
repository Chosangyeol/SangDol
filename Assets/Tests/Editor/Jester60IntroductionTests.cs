using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class Jester60IntroductionCases
{
    public const string PrefabPath = "Assets/03. Prefab/Dungeon/Jester60Intro/Jester60Introduction.prefab";
    public static void PrefabHasSynchronizedTimelinesAndSafeCamera()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath); Assert.IsNotNull(prefab);
        Assert.IsTrue(prefab.activeSelf); Assert.IsFalse(prefab.transform.Find("Presentation").gameObject.activeSelf);
        var binder = prefab.GetComponent<D1_FinalBoss60Presentation>(); Assert.IsNotNull(binder);
        var master = (PlayableDirector)Get(binder, "director"); Assert.AreEqual(26d, master.duration, .001d);
        var musicTrack = ((TimelineAsset)master.playableAsset).GetOutputTracks().OfType<AudioTrack>().Single();
        Assert.IsNotNull(master.GetGenericBinding(musicTrack));
        Assert.AreEqual(7, AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(master.playableAsset)).Length, "Tracks and clips must survive asset reimport and domain reload.");
        var controls = ((TimelineAsset)master.playableAsset).GetOutputTracks().OfType<ControlTrack>().ToArray();
        Assert.AreEqual(2, controls.Length);
        foreach (var track in controls)
        {
            var clip = track.GetClips().Single(); var control = (ControlPlayableAsset)clip.asset;
            Assert.IsTrue(EditorUtility.IsPersistent(track) && EditorUtility.IsPersistent(control));
            Assert.AreEqual(26d, clip.duration, .001d); Assert.IsFalse(control.searchHierarchy || control.updateParticle || control.updateITimeControl || control.active);
            Assert.IsTrue(control.updateDirector);
            var child = control.sourceGameObject.Resolve(master).GetComponent<PlayableDirector>(); Assert.IsNotNull(child);
            foreach (var output in child.playableAsset.outputs) Assert.IsNotNull(child.GetGenericBinding(output.sourceObject), output.streamName);
            foreach (var shot in ((TimelineAsset)child.playableAsset).GetOutputTracks().SelectMany(t => t.GetClips()).Select(c => c.asset).OfType<CinemachineShot>())
                Assert.IsNotNull(shot.VirtualCamera.Resolve(child));
        }
        foreach (var director in prefab.GetComponentsInChildren<PlayableDirector>(true)) Assert.IsFalse(director.playOnAwake);
        foreach (var audio in prefab.GetComponentsInChildren<AudioSource>(true)) Assert.IsFalse(audio.playOnAwake);
        Assert.AreEqual(0, prefab.GetComponentsInChildren<AudioListener>(true).Count(x => x.enabled));
        var camera = prefab.GetComponentInChildren<Camera>(true); Assert.IsFalse(camera.CompareTag("MainCamera"));
        Assert.Greater(camera.depth, 0f);
        var actor = (D1_MiddleBoss)Get(binder, "middleBoss");
        Assert.AreEqual(1, prefab.GetComponentsInChildren<D1_MiddleBoss>(true).Length);
        Assert.AreSame(actor, master.GetComponentsInChildren<D1_MiddleBoss>(true).Single());
        Assert.IsFalse(actor.enabled || actor.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled);
        Assert.IsTrue(actor.GetComponentsInChildren<Collider>(true).All(c => !c.enabled));
        Assert.AreEqual("Assets/08. Sound/Jester60Intro.mp4", AssetDatabase.GetAssetPath((UnityEngine.Video.VideoClip)Get(binder, "introductionVideo")));
        var surface = prefab.GetComponentInChildren<Unity.AI.Navigation.NavMeshSurface>(true);
        Assert.IsNotNull(surface.navMeshData);
        Assert.Less(actor.GetComponent<UnityEngine.AI.NavMeshAgent>().radius * actor.transform.lossyScale.x, 3f);
        Assert.Greater(((Transform)Get(binder, "waitingArea")).position.y, 90f);
        Assert.AreEqual(1, prefab.GetComponentsInChildren<JumpObject>(true).Length);
        var drop = prefab.GetComponentInChildren<JumpObject>(true);
        Assert.Greater(drop.transform.position.y - drop.targetPos.position.y, 90f);
        Assert.IsFalse(drop.isLocked);
        var fade = (CanvasGroup)Get(binder, "transitionFade");
        Assert.AreSame(prefab.transform, fade.transform.parent);
        Assert.AreEqual(RenderMode.ScreenSpaceOverlay, fade.GetComponent<Canvas>().renderMode);
        Assert.Greater(fade.GetComponent<Canvas>().sortingOrder, 1000);
        Assert.IsFalse(fade.blocksRaycasts);
        Assert.AreEqual(0f, fade.alpha);
    }
    public static object Get(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
}

public sealed class Jester60IntroductionRig : IDisposable
{
    public readonly FinalBossTransitionRig Transition = new FinalBossTransitionRig();
    public readonly GameObject Intro;
    public readonly D1_FinalBoss60Presentation Binder;
    public readonly PlayableDirector Director;
    public readonly Camera GameplayCamera;
    public readonly AudioSource Bgm;
    private readonly GameObject cameraObject, audioObject;
    private readonly AudioManager previousAudio = AudioManager.instance;
    private readonly float previousVolume = AudioListener.volume;
    private readonly AudioClip music;
    public Jester60IntroductionRig()
    {
        AudioListener.volume = 0f;
        cameraObject = new GameObject("IntroTestGameplayCamera"); cameraObject.tag = "MainCamera";
        GameplayCamera = cameraObject.AddComponent<Camera>(); cameraObject.AddComponent<AudioListener>();
        audioObject = new GameObject("IntroTestAudioManager"); audioObject.SetActive(false);
        AudioManager.instance = null; var audio = audioObject.AddComponent<AudioManager>(); audioObject.SetActive(true);
        music = AudioClip.Create("IntroTestMusic", 44100, 1, 44100, false); audio.PlayBGM(music);
        Bgm = audio.transform.Find("BGM_Player").GetComponent<AudioSource>();
        UnityEngine.Object.DontDestroyOnLoad(Transition.Boss.gameObject);
        Intro = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Jester60IntroductionCases.PrefabPath));
        Binder = Intro.GetComponent<D1_FinalBoss60Presentation>();
        Director = (PlayableDirector)Jester60IntroductionCases.Get(Binder, "director");
        GameEvent.OnBossStateChange?.Invoke(Transition.Boss);
    }
    public void StartTimeline() => Transition.Boss.StartCoroutine((IEnumerator)typeof(D1_FinalBoss).GetMethod("PlaySpecial3Timeline", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Transition.Boss, null));
    public void Dispose()
    {
        Transition.Dispose();
        UnityEngine.Object.DestroyImmediate(Intro);
        UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(audioObject); UnityEngine.Object.DestroyImmediate(music);
        AudioManager.instance = previousAudio; AudioListener.volume = previousVolume;
    }
}

public static class Jester60IntroductionRuntimeCases
{
    public static IEnumerator PooledBossBindsAndCancellationRestoresCameraAndMusic()
    {
        using (var r = new Jester60IntroductionRig())
        using (var objective = new DungeonObjectiveVisibilityRig())
        {
            yield return null;
            objective.UI.SetActive(true);
            Assert.AreSame(r.Director, r.Transition.Boss.Special3.cutsceneDirector);
            Assert.IsTrue(r.GameplayCamera.enabled && r.Bgm.isPlaying);
            r.StartTimeline(); yield return null;
            Assert.IsFalse(objective.UI.activeSelf);
            var camera = (Camera)Jester60IntroductionCases.Get(r.Binder, "presentationCamera");
            Assert.IsTrue(camera.enabled);
            Assert.IsFalse(r.GameplayCamera.enabled || r.Bgm.isPlaying);
            r.Director.time = 18d; r.Director.Evaluate(); yield return null;
            var children = r.Director.GetComponentsInChildren<PlayableDirector>(true).Where(d => d != r.Director).ToArray();
            Assert.AreEqual(2, children.Length);
            foreach (var child in children) Assert.That(child.time, Is.EqualTo(r.Director.time).Within(.15d));
            r.Transition.Boss.ForceStopCurrentAction();
            Assert.IsTrue(objective.UI.activeSelf);
            Assert.IsFalse(camera.enabled); Assert.IsTrue(r.GameplayCamera.enabled);
            yield return null;
            Assert.IsFalse(r.Director.gameObject.activeSelf);
            Assert.IsTrue(r.GameplayCamera.enabled && r.Bgm.isPlaying);
            foreach (var child in children) Assert.AreNotEqual(PlayState.Playing, child.state);
            r.StartTimeline(); Assert.IsTrue(camera.enabled);
            r.Transition.Boss.ForceStopCurrentAction(); Assert.IsFalse(camera.enabled);
            r.Binder.enabled = false;
            Assert.AreSame(r.Transition.Director, r.Transition.Boss.Special3.cutsceneDirector);
            GameEvent.OnBossStateChange?.Invoke(r.Transition.Boss);
            Assert.AreSame(r.Transition.Director, r.Transition.Boss.Special3.cutsceneDirector);
        }
    }
    public static IEnumerator IntroductionFinishesBeforeMiddleBossStarts()
    {
        using (var r = new Jester60IntroductionRig())
        using (var video = new VideoRecoveryRig())
        using (var objective = new DungeonObjectiveVisibilityRig())
        {
            yield return null; objective.UI.SetActive(true);
            r.Transition.Start();
            float timeout = Time.realtimeSinceStartup + 15f;
            while (!video.Manager.isPlaying && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsTrue(video.Manager.isPlaying);
            Assert.IsFalse(objective.UI.activeSelf);
            Assert.AreEqual("Assets/08. Sound/Jester60Intro.mp4", AssetDatabase.GetAssetPath(video.Player.clip));
            Assert.AreNotEqual(PlayState.Playing, r.Director.state);
            Assert.IsTrue(r.Bgm.mute && !r.Bgm.isPlaying);
            Assert.IsFalse(r.Transition.Boss.Special3.sceneMiddleBoss.enabled);
            timeout = Time.realtimeSinceStartup + 10f;
            while (!video.Player.isPlaying && video.Manager.isPlaying && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsTrue(video.Player.isPlaying, "The newly assigned intro video is decoded and played");
            var fade = (CanvasGroup)Jester60IntroductionCases.Get(r.Binder, "transitionFade");
            r.Director.played += _ => Assert.AreEqual(1f, fade.alpha, "Camera changes only behind an opaque fade");
            video.Manager.SkipVideo();
            Assert.IsFalse(objective.UI.activeSelf);
            Assert.IsTrue(video.Image.enabled, "The last video frame remains visible until the fade covers it");
            timeout = Time.realtimeSinceStartup + 5f;
            while (r.Director.state != PlayState.Playing && Time.realtimeSinceStartup < timeout)
            {
                Assert.IsNull(r.Transition.Get("_special3MiddleBoss")); yield return null;
            }
            Assert.AreEqual(PlayState.Playing, r.Director.state); Assert.IsFalse(r.Transition.Player.Model.canMove);
            Assert.IsFalse(video.Image.enabled);
            Assert.IsFalse(objective.UI.activeSelf, "The Timeline continues hiding dungeon objectives after the video releases its frame");
            timeout = Time.realtimeSinceStartup + 2f;
            while (fade.alpha > .001f && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.AreEqual(0f, fade.alpha);
            var camera = (Camera)Jester60IntroductionCases.Get(r.Binder, "presentationCamera");
            Vector3 initialPosition = camera.transform.position;
            var cameraDirector = r.Director.GetComponentsInChildren<PlayableDirector>(true).Single(d => d.name == "Camera");
            foreach (double time in new[] { 2d, 9d, 15d, 23d })
            {
                r.Director.time = time; r.Director.Evaluate(); yield return null;
                Assert.IsFalse(objective.UI.activeSelf);
                camera.GetComponent<Cinemachine.CinemachineBrain>().ManualUpdate();
                Assert.IsNotNull(camera.GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera);
                Assert.That(cameraDirector.time, Is.EqualTo(r.Director.time).Within(.15d));
                Capture(camera, "Temp/Jester60Applied_" + (int)time + ".png");
                Assert.IsNull(r.Transition.Get("_special3MiddleBoss"));
            }
            Assert.Greater(Vector3.Distance(initialPosition, camera.transform.position), 1f, "Timeline must drive the camera, not just wait out its duration.");
            r.Director.time = 25.75d; r.Director.Evaluate(); yield return null;
            Assert.IsNull(r.Transition.Get("_special3MiddleBoss"));
            bool stoppedObserved = false;
            r.Director.stopped += _ => { stoppedObserved = true; Assert.AreEqual(1f, fade.alpha); Assert.IsFalse(camera.enabled); Assert.IsTrue(r.GameplayCamera.enabled); };
            timeout = Time.realtimeSinceStartup + 3f;
            while (r.Transition.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsNotNull(r.Transition.Get("_special3MiddleBoss"));
            yield return WaitForCombat(r);
            Assert.IsTrue(stoppedObserved);
            Assert.IsTrue(r.Director.gameObject.activeSelf, "The same cinematic map remains active for combat");
            Assert.IsTrue(r.GameplayCamera.enabled && r.Transition.Player.Model.canMove);
            Assert.IsTrue(objective.UI.activeSelf, "Dungeon objectives return when cinematic playback finishes");
            Assert.IsTrue(r.Bgm.mute && !r.Bgm.isPlaying, "Gameplay BGM stays paused throughout the middle boss encounter");
        }
    }
    public static IEnumerator DisablingBinderDuringPlaybackRestoresOwnedState()
    {
        using (var r = new Jester60IntroductionRig())
        {
            yield return null; r.StartTimeline(); yield return null;
            r.Binder.enabled = false; yield return null;
            Assert.AreNotEqual(PlayState.Playing, r.Director.state);
            Assert.IsTrue(r.GameplayCamera.enabled && r.Bgm.isPlaying);
            Assert.AreSame(r.Transition.Director, r.Transition.Boss.Special3.cutsceneDirector);
        }
    }
    public static IEnumerator SharedActorSurvivesCancellationRetryAndBinderDisable()
    {
        using (var r = new Jester60IntroductionRig())
        {
            var actor = r.Transition.Boss.Special3.sceneMiddleBoss;
            int actorId = actor.GetInstanceID();
            for (int attempt = 0; attempt < 2; attempt++)
            {
                r.Transition.Start();
                float deadline = Time.realtimeSinceStartup + 15f;
                while (r.Director.state != PlayState.Playing && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(PlayState.Playing, r.Director.state);
                Assert.IsFalse(actor.enabled || actor.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled);
                r.Director.time = 25.8d; r.Director.Evaluate();
                deadline = Time.realtimeSinceStartup + 3f;
                while (r.Transition.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < deadline) yield return null;
                yield return WaitForCombat(r);
                Assert.AreSame(actor, r.Transition.Get("_special3MiddleBoss"));
                Assert.AreEqual(actorId, actor.GetInstanceID());
                Assert.AreEqual(1, r.Director.GetComponentsInChildren<D1_MiddleBoss>(true).Length);
                Assert.IsTrue(actor.enabled && actor.Agent.enabled && actor.Agent.isOnNavMesh);
                Assert.IsTrue(r.Transition.Player.Agent.isOnNavMesh);
                Assert.IsTrue(actor.GetComponentsInChildren<Collider>(true).All(c => c.enabled));
                var path = new UnityEngine.AI.NavMeshPath();
                Assert.IsTrue(UnityEngine.AI.NavMesh.CalculatePath(r.Transition.Player.Model.transform.position, actor.transform.position, UnityEngine.AI.NavMesh.AllAreas, path));
                Assert.AreNotEqual(UnityEngine.AI.NavMeshPathStatus.PathComplete, path.status, "The elevated entrance has no walking route into combat");
                if (attempt == 0) yield return DropIntoArena(r);
                if (attempt == 0) r.Transition.Boss.ForceStopCurrentAction();
                else r.Binder.enabled = false;
                yield return null;
                Assert.IsNotNull(actor);
                Assert.IsFalse(actor.gameObject.activeInHierarchy || r.Transition.Boss.isDoingSpecial || r.Transition.Boss.isImmunity);
                Assert.IsTrue(r.Transition.Player.Model.canMove && r.GameplayCamera.enabled && r.Bgm.isPlaying && !r.Bgm.mute);
                Assert.Less(Vector3.Distance(r.Transition.Player.Model.transform.position, r.Transition.Boss.playerStartPos.position), .1f);
            }
            Assert.AreSame(r.Transition.Director, r.Transition.Boss.Special3.cutsceneDirector);
        }
    }

    public static IEnumerator MissingTimelineStillUsesNavigableSharedArenaAndDoesNotCancelOtherPatterns()
    {
        using (var r = new Jester60IntroductionRig())
        {
            r.Transition.Boss.isDoingSpecial = true;
            r.Binder.enabled = false;
            Assert.IsTrue(r.Transition.Boss.isDoingSpecial, "Disabling the 60% binder does not cancel unrelated special patterns");
            r.Transition.Boss.isDoingSpecial = false;
            r.Binder.enabled = true;
            r.Director.playableAsset = null;
            r.Transition.Start();
            float deadline = Time.realtimeSinceStartup + 15f;
            while (r.Transition.Get("_special3MiddleBoss") == null && Time.realtimeSinceStartup < deadline) yield return null;
            yield return WaitForCombat(r);
            var actor = r.Transition.Boss.Special3.sceneMiddleBoss;
            Assert.AreSame(actor, r.Transition.Get("_special3MiddleBoss"));
            Assert.IsTrue(actor.gameObject.activeInHierarchy && actor.Agent.isOnNavMesh && r.Transition.Player.Agent.isOnNavMesh);
            Assert.IsTrue(r.GameplayCamera.enabled && r.Transition.Player.Model.canMove);
            Assert.IsFalse(((GameObject)Jester60IntroductionCases.Get(r.Binder, "cameraRig")).activeSelf);
            r.Transition.Boss.ForceStopCurrentAction();
            Assert.IsFalse(r.Director.gameObject.activeSelf);
        }
    }

    public static IEnumerator WaitForCombat(Jester60IntroductionRig r)
    {
        float deadline = Time.realtimeSinceStartup + 4f;
        var fade = (CanvasGroup)Jester60IntroductionCases.Get(r.Binder, "transitionFade");
        while ((r.Transition.Get("_special3MiddleBoss") == null || !r.Transition.Player.Model.canMove || fade.alpha > .001f) && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsNotNull(r.Transition.Get("_special3MiddleBoss"));
        Assert.IsTrue(r.Transition.Player.Model.canMove);
        Assert.AreEqual(0f, fade.alpha);
    }

    public static IEnumerator DropIntoArena(Jester60IntroductionRig r)
    {
        var jump = r.Director.GetComponentInChildren<JumpObject>(true);
        var model = r.Transition.Player.Model;
        Assert.Greater(model.transform.position.y, 90f);
        var path = new UnityEngine.AI.NavMeshPath();
        UnityEngine.AI.NavMesh.CalculatePath(model.transform.position, jump.transform.position, UnityEngine.AI.NavMesh.AllAreas, path);
        Assert.AreEqual(UnityEngine.AI.NavMeshPathStatus.PathComplete, path.status);
        r.Transition.Player.Controller.StopMove();
        Assert.IsTrue(r.Transition.Player.Agent.Warp(jump.transform.position));
        var controller = (UnityEditor.Animations.AnimatorController)model.Anim.runtimeAnimatorController;
        foreach (string name in new[] { "Jump", "JumpEnd" })
            if (!controller.parameters.Any(p => p.name == name)) controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
        model.interactableLayer = 1 << jump.gameObject.layer;
        model.interactableDistance = 4f;
        jump.EnableInteract(model.transform);
        Physics.SyncTransforms();
        model.TryInteract();
        Assert.IsFalse(model.canMove || r.Transition.Player.Agent.enabled);
        float deadline = Time.realtimeSinceStartup + jump.jumpDuration + 3f;
        while (!model.canMove && Time.realtimeSinceStartup < deadline) { Assert.IsFalse(r.Transition.Player.Agent.enabled); yield return null; }
        Assert.IsTrue(model.canMove && r.Transition.Player.Agent.isOnNavMesh);
        Assert.Less(Vector3.Distance(model.transform.position, jump.targetPos.position), .2f);
        Assert.IsTrue(model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        UnityEngine.AI.NavMesh.CalculatePath(model.transform.position, r.Transition.Boss.Special3.sceneMiddleBoss.transform.position, UnityEngine.AI.NavMesh.AllAreas, path);
        Assert.AreEqual(UnityEngine.AI.NavMeshPathStatus.PathComplete, path.status);
        UnityEngine.AI.NavMesh.CalculatePath(model.transform.position, r.Transition.Boss.Special3.waitingArea.position, UnityEngine.AI.NavMesh.AllAreas, path);
        Assert.AreNotEqual(UnityEngine.AI.NavMeshPathStatus.PathComplete, path.status, "There is no return route to the upper platform");
        jump.DisableInteract(model.transform);
        model.TryInteract(); yield return null;
        Assert.IsTrue(model.canMove && r.Transition.Player.Agent.enabled, "The upper jump cannot be reused from the lower arena");
    }

    public static IEnumerator FadesUseRealtimeAndCancellationReleasesHeldVideo()
    {
        using (var r = new Jester60IntroductionRig())
        using (var video = new VideoRecoveryRig())
        {
            var fade = (CanvasGroup)Jester60IntroductionCases.Get(r.Binder, "transitionFade");
            float previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                var fadeRoutine = (IEnumerator)typeof(D1_FinalBoss60Presentation).GetMethod("FadeTo", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(r.Binder, new object[] { 1f });
                r.Binder.StartCoroutine(fadeRoutine);
                yield return new WaitForSecondsRealtime(.65f);
                Assert.AreEqual(1f, fade.alpha);
            }
            finally { Time.timeScale = previousScale; }
            r.Transition.Boss.ForceStopCurrentAction(); Assert.AreEqual(0f, fade.alpha);
            r.Transition.Start();
            float deadline = Time.realtimeSinceStartup + 15f;
            while (!video.Manager.isPlaying && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(video.Manager.isPlaying);
            video.Manager.SkipVideo();
            Assert.IsTrue(video.Image.enabled);
            deadline = Time.realtimeSinceStartup + 2f;
            while (fade.alpha <= .01f && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.Greater(fade.alpha, .01f);
            r.Binder.enabled = false; yield return null;
            Assert.AreEqual(0f, fade.alpha);
            Assert.IsFalse(video.Image.enabled || video.Manager.isPlaying || r.Transition.Boss.isDoingSpecial);
            Assert.IsTrue(r.GameplayCamera.enabled && r.Transition.Player.Model.canMove && r.Bgm.isPlaying);
            Assert.IsNull(r.Transition.Get("_special3MiddleBoss"));
        }
    }

    private static void Capture(Camera camera, string path)
    {
        var priorTarget = camera.targetTexture; var priorActive = RenderTexture.active;
        var render = RenderTexture.GetTemporary(960, 540, 24); var texture = new Texture2D(960, 540, TextureFormat.RGB24, false);
        try { camera.targetTexture = render; camera.Render(); RenderTexture.active = render; texture.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); texture.Apply(); System.IO.File.WriteAllBytes(path, texture.EncodeToPNG()); }
        finally { camera.targetTexture = priorTarget; RenderTexture.active = priorActive; RenderTexture.ReleaseTemporary(render); UnityEngine.Object.DestroyImmediate(texture); }
    }
}
