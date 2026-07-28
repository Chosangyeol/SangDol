using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StigmaDescUI : MonoBehaviour
{
    private StigmaDataSO _data;

    [Header("UI 컴포넌트")]
    public TMP_Text stigmaNameText;
    public TMP_Text stigmaRequireLevelText;
    public TMP_Text stigmaDescText;
    public Button stigmaEquipButton;

    // 🌟 변경: 이름 변경 및 현재 장착/잠금 상태 파라미터 추가
    public void ShowDescription(StigmaDataSO data, bool isEquipped, bool isLocked)
    {
        _data = data;
        UpdateUI(isEquipped, isLocked);
    }

    // 🌟 변경: 비주얼 업데이트 시 버튼 텍스트와 상호작용 제어 추가
    public void UpdateUI(bool isEquipped, bool isLocked)
    {
        if (_data == null)
        {
            Debug.LogError("StigmaDataSO is null. Cannot update UI.");
            return;
        }

        stigmaNameText.text = _data.stigmaName;
        stigmaRequireLevelText.text = $"요구 레벨: {_data.requireLevel}";
        stigmaDescText.text = _data.stigmaDescription;

        // 버튼 텍스트 컴포넌트 확보 (TextMeshPro 사용 기준)
        TMP_Text buttonText = stigmaEquipButton.GetComponentInChildren<TMP_Text>();

        // 🌟 상태별 버튼 세팅 분기
        if (isLocked)
        {
            stigmaEquipButton.interactable = false; // 버튼 클릭 막기
            if (buttonText != null) buttonText.text = "잠김";
        }
        else
        {
            stigmaEquipButton.interactable = true;  // 버튼 클릭 허용
            if (buttonText != null)
            {
                // 이미 장착 중이면 "해제", 미장착 상태면 "장착"으로 유동적 변경
                buttonText.text = isEquipped ? "해제" : "장착";
            }
        }
    }
}