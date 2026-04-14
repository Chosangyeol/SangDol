using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayManager : MonoBehaviour
{
    public static VideoPlayManager instance;
    private RawImage textureImage;
    private VideoPlayer vp;
    public bool isPlaying = false;

    private void Awake()
    {
        instance = this;
        vp = GetComponent<VideoPlayer>();
        textureImage = GetComponent<RawImage>();
        textureImage.enabled = false;


        if (vp != null)
        {
            // ⭐️ 핵심: 영상이 끝났을 때 실행될 함수(OnVideoFinished)를 연결해 둡니다.
            vp.loopPointReached += OnVideoFinished;
        }
    }

    public void PlayVideo(VideoClip clip)
    {
        vp.targetTexture.Release();

        textureImage.enabled = true;

        vp.clip = clip;
        isPlaying = true;
        vp.Play();
    }

    public void ClearClip()
    {
        vp.clip = null;
    }

    public void OnVideoFinished(VideoPlayer vp)
    {
        isPlaying = false;

        textureImage.enabled = false;
    }

    public void SkipVideo()
    {
        vp.Stop();
        OnVideoFinished(vp);
        ClearClip();
    }
}
