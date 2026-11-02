using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VolumeComponent
{
    public Slider volumeSlider;
    public TMP_Text volumeText;
    public Toggle volumeMuteToggle;

    [HideInInspector]
    public bool isMute;
}

public class OptionUI : MonoBehaviour
{
    public GameObject optionUI;
    internal bool IsOpen => optionUI != null && optionUI.activeSelf;

    private GameObject soundPanel;
    private GameObject controlKeysPanel;

    [Header("사운드 UI [0:Master , 1:BGM, 2:SFX]")]
    public VolumeComponent[] volumeComponents;

    private void Start()
    {
        for (int i = 0; i < volumeComponents.Length; i++)
        {
            int index = i; // ⭐️ 람다식 캡처(Closure)를 위해 지역 변수로 복사 (매우 중요!)

            // 슬라이더 값이 바뀔 때 OnVolumeChanged 실행
            if (volumeComponents[index].volumeSlider != null)
                volumeComponents[index].volumeSlider.onValueChanged.AddListener((value) => OnVolumeChanged(index, value));

            // 토글 값이 바뀔 때 OnMuteToggled 실행
            if (volumeComponents[index].volumeMuteToggle != null)
                volumeComponents[index].volumeMuteToggle.onValueChanged.AddListener((isMute) => OnMuteToggled(index, isMute));
        }

        // 2. 저장된 설정 불러와서 UI에 적용
        LoadOption();
        InitializeControlKeysPanel();
    }

    private void InitializeControlKeysPanel()
    {
        if (optionUI == null) return;

        Transform section = optionUI.transform.Find("Section");
        if (section == null) return;

        Transform sound = section.Find("Sound");
        if (sound != null) soundPanel = sound.gameObject;

        Transform existingControlKeysPanel = section.Find("ControlKeysPanel");
        if (existingControlKeysPanel != null)
        {
            controlKeysPanel = existingControlKeysPanel.gameObject;
            controlKeysPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OptionUI: Section 아래에서 ControlKeysPanel을 찾을 수 없습니다.", this);
        }

        Transform tabs = optionUI.transform.Find("Tab");
        if (tabs == null) return;

        Transform soundTab = tabs.Find("Tab1");
        if (soundTab != null)
        {
            Button soundTabButton = soundTab.GetComponent<Button>();
            if (soundTabButton != null) soundTabButton.onClick.AddListener(ShowSoundPanel);
        }

        Transform controlKeysTab = tabs.Find("Tab3");
        if (controlKeysTab != null)
        {
            Button controlKeysTabButton = controlKeysTab.GetComponent<Button>();
            if (controlKeysTabButton != null) controlKeysTabButton.onClick.AddListener(ShowControlKeysPanel);
        }
    }

    private void ShowSoundPanel()
    {
        if (soundPanel != null) soundPanel.SetActive(true);
        if (controlKeysPanel != null) controlKeysPanel.SetActive(false);
    }

    private void ShowControlKeysPanel()
    {
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlKeysPanel != null) controlKeysPanel.SetActive(true);
    }

    private void LoadOption()
    {
        float master = OptionManager.instance.GetVolume("Master");
        float bgm = OptionManager.instance.GetVolume("BGM");
        float sfx = OptionManager.instance.GetVolume("SFX");

        if (volumeComponents[0].volumeSlider != null)
        {
            volumeComponents[0].volumeSlider.SetValueWithoutNotify(master);
            volumeComponents[0].volumeText.text = $"{(int)(master * 100)}%";
        }

        if (volumeComponents[1].volumeSlider != null)
        {
            volumeComponents[1].volumeSlider.SetValueWithoutNotify(bgm);
            volumeComponents[1].volumeText.text = $"{(int)(bgm * 100)}%";
        }

        if (volumeComponents[2].volumeSlider != null)
        {
            volumeComponents[2].volumeSlider.SetValueWithoutNotify(sfx);
            volumeComponents[2].volumeText.text = $"{(int)(sfx * 100)}%";
        }

    }


    void OnVolumeChanged(int index, float value)
    {
        if (volumeComponents[index].volumeText != null)
            volumeComponents[index].volumeText.text = $"{(int)(value * 100)}%";

        switch (index)
        {
            case 0: OptionManager.instance.SetMasterVolume(value); break;
            case 1: OptionManager.instance.SetBGMVolume(value); break;
            case 2: OptionManager.instance.SetSFXVolume(value); break;
        }
    }

    void OnMuteToggled(int index, bool isMute)
    {
        volumeComponents[index].isMute = isMute;

        // 시각적 피드백: 음소거 중일 때는 슬라이더를 못 움직이게 비활성화
        if (volumeComponents[index].volumeSlider != null)
        {
            volumeComponents[index].volumeSlider.interactable = !isMute;
        }

        // OptionManager로 음소거 명령 전달 
        // (OptionManager 쪽에 아래 주석 처리된 함수들을 만들어두셔야 합니다)
        switch (index)
        {
            case 0: OptionManager.instance.SetMasterMute(isMute); break;
            case 1: OptionManager.instance.SetBGMMute(isMute); break;
            case 2: OptionManager.instance.SetSFXMute(isMute); break;
        }
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (optionUI == null) return;

        if (onlyFalse)
        {
            optionUI.SetActive(false);
            return;
        }

        bool showOption = !optionUI.activeSelf;
        optionUI.SetActive(showOption);
        if (showOption) ShowSoundPanel();
    }
}
