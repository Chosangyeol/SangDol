using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using UnityEngine;


public class OptionManager : MonoBehaviour
{
    public static OptionManager instance;

    [Header("사운드 설정")]
    public AudioMixer mainMixer;

    private float masterVolume;
    private float preMasterVolume;
    private bool isMasterMuted = false;

    private float bgmVolume;
    private float preBgmVolume;
    private bool isBGMMuted = false;

    private float sfxVolume;
    private float preSfxVolume;
    private bool isSFXMuted = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyAllVolumes();
    }


    #region 사운드 세팅
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp(value, 0.0001f, 1f);
        mainMixer.SetFloat("Master", Mathf.Log10(masterVolume)*20);
        PlayerPrefs.SetFloat("MasterVolume",masterVolume);
    }

    public void SetMasterMute(bool isMute)
    {
        isMasterMuted = isMute;
        if (isMute)
        {
            preMasterVolume = masterVolume;
            SetMasterVolume(0.0001f);
        }
        else
        {
            SetMasterVolume(preMasterVolume);
        }
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp(value, 0.0001f, 1f);
        mainMixer.SetFloat("BGM", Mathf.Log10(bgmVolume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetBGMMute(bool isMute)
    {
        isBGMMuted = isMute;
        if (isMute)
        {
            preBgmVolume = bgmVolume;
            SetBGMVolume(0.0001f);
        }
        else
        {
            SetBGMVolume(preBgmVolume);
        }
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp(value, 0.0001f, 1f);
        mainMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetSFXMute(bool isMute)
    {
        isSFXMuted = isMute;
        if (isMute)
        {
            preSfxVolume = sfxVolume;
            SetMasterVolume(0.0001f);
        }
        else
        {
            SetSFXVolume(preSfxVolume);
        }
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void ApplyAllVolumes()
    {
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }

    public float GetVolume(string type)
    {
        switch (type)
        {
            case "Master": return masterVolume;
            case "BGM": return bgmVolume;
            case "SFX": return sfxVolume;
            default: return 1f;
        }
    }
    #endregion
}
