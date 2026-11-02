using UnityEngine;
using UnityEngine.Video;

// Added to build-scene copies for standalone VideoPlayers such as the title background.
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(VideoPlayer))]
public sealed class WebVideoPlayerSource : MonoBehaviour
{
    [SerializeField] private WebVideoSource source;
    [SerializeField] private bool autoplay;
    private VideoPlayer player;
    private bool waitingForInteraction;

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        player = GetComponent<VideoPlayer>();
        player.playOnAwake = false;
        if (!WebVideoSource.Configure(player, null, source, out _)) return;
        if (autoplay)
        {
            // Muted background video can start before the browser receives a gesture.
            waitingForInteraction = player.audioOutputMode != VideoAudioOutputMode.None;
            player.controlledAudioTrackCount = source.audioTracks;
            for (ushort track = 0; track < player.controlledAudioTrackCount; track++) player.SetDirectAudioMute(track, true);
            player.Play();
        }
#endif
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!waitingForInteraction || !Input.anyKeyDown) return;
        waitingForInteraction = false;
        for (ushort track = 0; track < player.controlledAudioTrackCount; track++) player.SetDirectAudioMute(track, false);
        if (!player.isPlaying) player.Play();
#endif
    }
}
