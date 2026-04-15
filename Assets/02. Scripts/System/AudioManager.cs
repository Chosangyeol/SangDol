using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct SFXData
{
    public C_Enums.SFX_List soundName;
    public AudioClip clip;
}

[System.Serializable]
public struct BGMData
{
    public C_Enums.BGM_List soundName;
    public AudioClip clip;
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("믹서")]
    public AudioMixerGroup bgmMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("사운드 데이터")]
    public SFXData[] sfxDatabase;
    public BGMData[] bgmDatabase;

    [Header("BGM 재생기")]
    private AudioSource bgmPlayer;

    [Header("SFX 풀링(돌려쓰기) 설정")]
    public int sfxPoolSize = 15; // 동시에 재생될 수 있는 최대 소리 개수
    private AudioSource[] sfxPlayers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init(); // 재생기들 세팅
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        // 1. BGM 재생기 생성 및 세팅
        GameObject bgmObject = new GameObject("BGM_Player");
        bgmObject.transform.SetParent(transform);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true; // BGM은 무한반복
        bgmPlayer.outputAudioMixerGroup = bgmMixerGroup; // ⭐️ 옵션 시스템과 연결!

        // 2. SFX 재생기 여러 개 생성 및 배열에 담기
        GameObject sfxObject = new GameObject("SFX_Players");
        sfxObject.transform.SetParent(transform);
        sfxPlayers = new AudioSource[sfxPoolSize];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].outputAudioMixerGroup = sfxMixerGroup; // ⭐️ 옵션 시스템과 연결!

            // 3D 사운드(거리에 따라 소리 작아짐)를 원한다면 아래 값을 1f로, 2D는 0f로 설정
            sfxPlayers[i].spatialBlend = 0f;
        }
    }

    public void PlayBGM(C_Enums.BGM_List data)
    {
        AudioClip clipToPlay = null;

        // 1. 데이터베이스에서 이름표에 맞는 소리를 찾는다.
        for (int i = 0; i < bgmDatabase.Length; i++)
        {
            if (bgmDatabase[i].soundName == data)
            {
                clipToPlay = bgmDatabase[i].clip;
                break;
            }
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning($"{data} 오디오 클립을 데이터베이스에서 찾을 수 없습니다!");
            return;
        }

        // 2. 찾아낸 클립을 빈 재생기에 넣고 튼다 (기존 로직 재활용)
        PlayBGM(clipToPlay);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmPlayer.isPlaying && bgmPlayer.clip == clip) return;

        bgmPlayer.clip = clip;
        bgmPlayer.Play();
    }

    public void PlaySFX(C_Enums.SFX_List data)
    {
        AudioClip clipToPlay = null;

        // 1. 데이터베이스에서 이름표에 맞는 소리를 찾는다.
        for (int i = 0; i < sfxDatabase.Length; i++)
        {
            if (sfxDatabase[i].soundName == data)
            {
                clipToPlay = sfxDatabase[i].clip;
                break;
            }
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning($"{data} 오디오 클립을 데이터베이스에서 찾을 수 없습니다!");
            return;
        }

        // 2. 찾아낸 클립을 빈 재생기에 넣고 튼다 (기존 로직 재활용)
        PlaySFX(clipToPlay);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (!sfxPlayers[i].isPlaying)
            {
                sfxPlayers[i].clip = clip;
                sfxPlayers[i].Play();
                return;
            }
        }
    }
}
