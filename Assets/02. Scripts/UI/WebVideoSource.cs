using System;
using UnityEngine;
using UnityEngine.Video;

// Generated companions keep WebGL playback independent of imported VideoClip assets.
[Serializable]
internal sealed class WebVideoSource
{
    public string relativePath;
    public double length;
    public ushort audioTracks;

    internal static bool HasVideo(VideoClip clip, WebVideoSource web)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return web != null && !string.IsNullOrEmpty(web.relativePath);
#else
        return clip != null || (web != null && !string.IsNullOrEmpty(web.relativePath));
#endif
    }

    internal static bool Configure(VideoPlayer player, VideoClip clip, WebVideoSource web, out double duration)
    {
        duration = 0d;
#if !UNITY_WEBGL || UNITY_EDITOR
        if (clip != null)
        {
            player.source = VideoSource.VideoClip;
            player.clip = clip;
            duration = clip.length;
            return true;
        }
#endif
        if (web == null || string.IsNullOrEmpty(web.relativePath)) return false;
        player.clip = null;
        player.source = VideoSource.Url;
        player.url = BuildUrl(Application.streamingAssetsPath, web.relativePath);
        duration = web.length;
        return true;
    }

    internal static string BuildUrl(string root, string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        if (path.StartsWith("/") || Array.Exists(path.Split('/'), p => p == ".."))
            throw new ArgumentException("Video path must stay within StreamingAssets.", nameof(relativePath));
        if (root.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || root.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return root.TrimEnd('/') + "/" + string.Join("/", Array.ConvertAll(path.Split('/'), Uri.EscapeDataString));
        return System.IO.Path.Combine(root, path);
    }
}
