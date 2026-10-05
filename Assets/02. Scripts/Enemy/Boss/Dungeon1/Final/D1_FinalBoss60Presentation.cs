using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

/// <summary>Scene-owned introduction for pooled final bosses; combat remains on the boss.</summary>
[DisallowMultipleComponent]
public sealed class D1_FinalBoss60Presentation : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private Camera presentationCamera;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource voiceSource;
    [Header("Shared introduction and combat arena")]
    [SerializeField] private D1_MiddleBoss middleBoss;
    [SerializeField] private GameObject middleBossPrefab;
    [SerializeField] private Transform waitingArea;
    [SerializeField] private Transform middleBossSpawnPoint;
    [SerializeField] private Transform middleBossIntroductionPoint;
    [SerializeField] private VideoClip introductionVideo;
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject cinematicPlayerStandIn;
    [Header("Transition fade")]
    [SerializeField] private CanvasGroup transitionFade;
    [SerializeField, Min(0f)] private float fadeDuration = .5f;
    internal float FadeDuration => Mathf.Max(0f, fadeDuration);

    internal IEnumerator FadeTo(float alpha)
    {
        if (transitionFade == null) yield break;
        float start = transitionFade.alpha;
        if (Mathf.Approximately(start, alpha)) yield break;
        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transitionFade.alpha = Mathf.Lerp(start, alpha, FadeDuration > 0f ? elapsed / FadeDuration : 1f);
            if (elapsed < FadeDuration) yield return null;
        }
        transitionFade.alpha = alpha;
    }

    private sealed class PreviousBinding
    {
        public PlayableDirector director;
        public D1_MiddleBoss actor;
        public GameObject prefab;
        public Transform waiting, spawn;
        public VideoClip video;
    }
    private readonly Dictionary<D1_FinalBoss, PreviousBinding> previousDirectors = new Dictionary<D1_FinalBoss, PreviousBinding>();
    private Camera gameplayCamera;
    private AudioSource pausedMusic;
    private bool presenting;
    private bool keepArena;

    private void OnEnable()
    {
        if (middleBoss != null)
        {
            middleBoss.PrepareForPresentation();
            if (director != null && director.gameObject != gameObject) director.gameObject.SetActive(false);
        }
        GameEvent.OnBossStateChange += BindBoss;
        if (director != null)
        {
            director.played += OnPlayed;
            director.stopped += OnStopped;
        }
        foreach (var boss in FindObjectsByType<D1_FinalBoss>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) BindBoss(boss);
    }

    private void BindBoss(BossModel model)
    {
        if (!(model is D1_FinalBoss boss) || director == null || boss.Special3 == null || previousDirectors.ContainsKey(boss)) return;
        // Pooled bosses belong to the PoolManager's DontDestroyOnLoad scene.
        previousDirectors.Add(boss, new PreviousBinding
        {
            director = boss.Special3.cutsceneDirector, actor = boss.Special3.sceneMiddleBoss,
            prefab = boss.Special3.prefab, waiting = boss.Special3.waitingArea,
            spawn = boss.Special3.middleBossSpawnPoint, video = boss.Special3.cutsceneClip
        });
        boss.Special3.cutsceneDirector = director;
        if (middleBoss != null) boss.Special3.sceneMiddleBoss = middleBoss;
        if (middleBossPrefab != null) boss.Special3.prefab = middleBossPrefab;
        if (waitingArea != null) boss.Special3.waitingArea = waitingArea;
        if (middleBossSpawnPoint != null) boss.Special3.middleBossSpawnPoint = middleBossSpawnPoint;
        if (introductionVideo != null) boss.Special3.cutsceneClip = introductionVideo;
    }

    private void OnPlayed(PlayableDirector source)
    {
        if (presenting) return;
        presenting = true;
        keepArena = false;
        foreach (var pair in previousDirectors)
            if (pair.Key != null && pair.Key.isDoingSpecial && pair.Key.Special3.sceneMiddleBoss == middleBoss)
                keepArena = middleBoss != null && pair.Key.IsSpecial3Active;
        if (middleBoss != null) middleBoss.PrepareForPresentation();
        if (middleBoss != null && middleBossIntroductionPoint != null)
            middleBoss.transform.SetPositionAndRotation(middleBossIntroductionPoint.position, middleBossIntroductionPoint.rotation);
        if (cameraRig != null) cameraRig.SetActive(true);
        if (cinematicPlayerStandIn != null) cinematicPlayerStandIn.SetActive(true);
        if (presentationCamera != null) presentationCamera.enabled = true;
        var camera = Camera.main;
        if (camera != null && camera != presentationCamera && camera.enabled)
        {
            gameplayCamera = camera;
            gameplayCamera.enabled = false;
        }
        var audio = AudioManager.instance;
        if (audio != null)
        {
            if (musicSource != null) musicSource.outputAudioMixerGroup = audio.bgmMixerGroup;
            if (voiceSource != null) voiceSource.outputAudioMixerGroup = audio.sfxMixerGroup;
            var bgm = audio.transform.Find("BGM_Player")?.GetComponent<AudioSource>();
            if (bgm != null && !bgm.mute && bgm.isPlaying) { pausedMusic = bgm; bgm.Pause(); }
        }
    }

    private void OnStopped(PlayableDirector source)
    {
        RestorePresentation();
        if (cinematicPlayerStandIn != null) cinematicPlayerStandIn.SetActive(false);
        // Keep the same map and actor alive for combat; remove only the cinematic cameras.
        if (!keepArena && source != null && source.gameObject != gameObject && source.gameObject.activeSelf)
            source.gameObject.SetActive(false);
    }

    private void RestorePresentation()
    {
        if (presentationCamera != null) presentationCamera.enabled = false;
        if (cameraRig != null) cameraRig.SetActive(false);
        if (director != null)
            foreach (var child in director.GetComponentsInChildren<PlayableDirector>(true))
                if (child != director) child.Stop();
        if (musicSource != null) musicSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
        if (gameplayCamera != null) gameplayCamera.enabled = true;
        gameplayCamera = null;
        if (pausedMusic != null && pausedMusic.isActiveAndEnabled) pausedMusic.UnPause();
        pausedMusic = null;
        presenting = false;
    }

    internal void BeginEncounter()
    {
        keepArena = true;
        if (director != null && director.gameObject != gameObject) director.gameObject.SetActive(true);
        RestorePresentation();
        if (cinematicPlayerStandIn != null) cinematicPlayerStandIn.SetActive(false);
    }

    internal void FinishEncounter()
    {
        if (transitionFade != null) transitionFade.alpha = 0f;
        keepArena = false;
        if (director != null) director.Stop();
        RestorePresentation();
        if (middleBoss != null)
        {
            middleBoss.enabled = false;
            var agent = middleBoss.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            foreach (var collider in middleBoss.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        }
        if (director != null && director.gameObject != gameObject) director.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameEvent.OnBossStateChange -= BindBoss;
        if (director != null)
        {
            director.played -= OnPlayed;
            director.stopped -= OnStopped;
        }
        foreach (var pair in previousDirectors)
            if (middleBoss != null && pair.Key != null && pair.Key.Special3 != null &&
                pair.Key.Special3.sceneMiddleBoss == middleBoss && pair.Key.IsSpecial3Active)
                pair.Key.ForceStopCurrentAction();
        FinishEncounter();
        foreach (var pair in previousDirectors)
        {
            if (pair.Key == null || pair.Key.Special3 == null) continue;
            var data = pair.Key.Special3;
            if (data.cutsceneDirector == director) data.cutsceneDirector = pair.Value.director;
            if (middleBoss != null && data.sceneMiddleBoss == middleBoss) data.sceneMiddleBoss = pair.Value.actor;
            if (middleBossPrefab != null && data.prefab == middleBossPrefab) data.prefab = pair.Value.prefab;
            if (waitingArea != null && data.waitingArea == waitingArea) data.waitingArea = pair.Value.waiting;
            if (middleBossSpawnPoint != null && data.middleBossSpawnPoint == middleBossSpawnPoint) data.middleBossSpawnPoint = pair.Value.spawn;
            if (introductionVideo != null && data.cutsceneClip == introductionVideo) data.cutsceneClip = pair.Value.video;
        }
        previousDirectors.Clear();
    }
}
