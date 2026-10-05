using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

internal enum VideoPlaybackResult { InProgress, Completed, Skipped, Failed, Cancelled }

public class VideoPlayManager : MonoBehaviour
{
    public static VideoPlayManager instance;
    private RawImage textureImage;
    private VideoPlayer vp;
    public bool isPlaying = false;
    [SerializeField, Min(0.1f)] private float preparationTimeout = 15f;
    private int playbackSession;
    private VideoPlaybackResult result = VideoPlaybackResult.Cancelled;
    private Coroutine watchdog;
    private bool holdFrameOnCompletion;
    private int heldFrameSession;
    private DungeonManager objectiveDungeon;

    private void Awake()
    {
        instance = this;
        vp = GetComponent<VideoPlayer>();
        textureImage = GetComponent<RawImage>();
        if (textureImage != null) textureImage.enabled = false;
        if (vp != null)
        {
            vp.playOnAwake = false;
            vp.loopPointReached += OnVideoFinished;
            vp.errorReceived += OnVideoError;
        }
    }

    public void PlayVideo(VideoClip clip)
    {
        if (isPlaying) SkipVideo();
        if (heldFrameSession != 0) CancelPlayback(heldFrameSession);
        TryPlayVideo(clip, out _);
    }

    internal bool TryPlayVideo(VideoClip clip, out int session)
    {
        session = 0;
        // Other callers must not take over an owned playback.
        if (isPlaying || heldFrameSession != 0) return false;
        holdFrameOnCompletion = false;
        playbackSession++;
        result = VideoPlaybackResult.Failed;
        if (clip == null || vp == null || !vp.isActiveAndEnabled || !isActiveAndEnabled)
        {
            Debug.LogWarning("[VideoPlayManager] Playback unavailable: clip or active VideoPlayer missing.");
            return false;
        }
        session = playbackSession;
        result = VideoPlaybackResult.InProgress;
        isPlaying = true;
        try
        {
            vp.Stop();
            if (vp.targetTexture != null) vp.targetTexture.Release();
            vp.clip = clip; vp.isLooping = false;
            objectiveDungeon = DungeonManager.instance;
            if (objectiveDungeon != null) objectiveDungeon.SuppressObjectives(this);
            if (textureImage != null) textureImage.enabled = true;
            vp.Play();
            watchdog = StartCoroutine(WatchPlayback(session, clip.length));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[VideoPlayManager] Playback failed: " + ex.Message);
            Finish(session, VideoPlaybackResult.Failed);
            return false;
        }
        return IsPlaybackActive(session);
    }

    internal bool IsPlaybackActive(int session) => session != 0 && session == playbackSession && isPlaying;
    internal VideoPlaybackResult GetResult(int session) => session == playbackSession ? result : VideoPlaybackResult.Cancelled;
    internal void HoldFrameForTransition(int session)
    {
        if (IsPlaybackActive(session)) holdFrameOnCompletion = true;
    }
    internal void CancelPlayback(int session)
    {
        if (IsPlaybackActive(session)) Finish(session, VideoPlaybackResult.Cancelled);
        else if (session != 0 && session == heldFrameSession && session == playbackSession) ReleaseFrame();
    }

    private IEnumerator WatchPlayback(int session, double length)
    {
        float prepareDeadline = Time.realtimeSinceStartup + Mathf.Max(0.1f, preparationTimeout);
        float deadline = Time.realtimeSinceStartup + Mathf.Clamp((float)length + preparationTimeout + 5f, 5f, 3600f);
        while (IsPlaybackActive(session))
        {
            if (vp == null || !vp.isActiveAndEnabled ||
                (!vp.isPrepared && Time.realtimeSinceStartup >= prepareDeadline) ||
                Time.realtimeSinceStartup >= deadline)
            {
                Debug.LogWarning("[VideoPlayManager] Playback timed out or VideoPlayer became unavailable.");
                Finish(session, VideoPlaybackResult.Failed);
                yield break;
            }
            yield return null;
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        if (source != vp || !isPlaying) return;
        Debug.LogWarning("[VideoPlayManager] Video error: " + message);
        Finish(playbackSession, VideoPlaybackResult.Failed);
    }

    private void Finish(int session, VideoPlaybackResult completion)
    {
        if (!IsPlaybackActive(session)) return;
        isPlaying = false; result = completion;
        if (watchdog != null) StopCoroutine(watchdog);
        watchdog = null;
        if (holdFrameOnCompletion && (completion == VideoPlaybackResult.Completed || completion == VideoPlaybackResult.Skipped))
        {
            heldFrameSession = session;
            if (vp != null) vp.Pause();
        }
        else ReleaseFrame();
    }
    private void ReleaseFrame()
    {
        holdFrameOnCompletion = false;
        heldFrameSession = 0;
        if (vp != null) { vp.Stop(); vp.clip = null; }
        if (textureImage != null) textureImage.enabled = false;
        if (objectiveDungeon != null) objectiveDungeon.RestoreObjectives(this);
        objectiveDungeon = null;
    }
    public void ClearClip()
    {
        CancelPlayback(playbackSession);
        if (vp != null) vp.clip = null;
    }
    public void OnVideoFinished(VideoPlayer source)
    {
        if (source == vp && source != null) Finish(playbackSession, VideoPlaybackResult.Completed);
    }
    public void SkipVideo() => Finish(playbackSession, VideoPlaybackResult.Skipped);

    private void OnDisable() => CancelPlayback(playbackSession);
    private void OnDestroy()
    {
        CancelPlayback(playbackSession);
        if (vp != null) { vp.loopPointReached -= OnVideoFinished; vp.errorReceived -= OnVideoError; }
        if (instance == this) instance = null;
    }
}
