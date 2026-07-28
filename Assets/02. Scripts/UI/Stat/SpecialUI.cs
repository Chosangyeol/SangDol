using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static C_Enums;

public class SpecialUI : MonoBehaviour
{
    [Header("스텟 투자 UI")]
    [SerializeField] TMP_Text remainPointText;

    [Header("스텟 투자 버튼")]
    [SerializeField] Button _s1Button;
    [SerializeField] Button _s2Button;
    [SerializeField] Button _s3Button;
    [SerializeField] Button _s4Button;
    [SerializeField] Button _s5Button;

    [Header("현제 스텟 현황 텍스트")]
    [SerializeField] TMP_Text _statS1Text;
    [SerializeField] TMP_Text _statS2Text;
    [SerializeField] TMP_Text _statS3Text;
    [SerializeField] TMP_Text _statS4Text;
    [SerializeField] TMP_Text _statS5Text;

    [Header("로스트아크 스타일 툴팁 시스템")]
    [SerializeField] private SpecialPopup _tooltipUI;

    private C_SpecialStat _special;

    public void Init(C_SpecialStat special)
    {
        _special = special;

        _s1Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S1));
        _s2Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S2));
        _s3Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S3));
        _s4Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S4));
        _s5Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S5));

        SetupTooltipTrigger(_s1Button.gameObject, C_Enums.SpecialStat.S1);
        SetupTooltipTrigger(_s2Button.gameObject, C_Enums.SpecialStat.S2);
        SetupTooltipTrigger(_s3Button.gameObject, C_Enums.SpecialStat.S3);
        SetupTooltipTrigger(_s4Button.gameObject, C_Enums.SpecialStat.S4);
        SetupTooltipTrigger(_s5Button.gameObject, C_Enums.SpecialStat.S5);

        RefreshSpecial();
    }

    private void SetupTooltipTrigger(GameObject buttonObj, C_Enums.SpecialStat stat)
    {
        SpecialStatTooltipTrigger trigger = buttonObj.GetComponent<SpecialStatTooltipTrigger>();
        if (trigger == null) trigger = buttonObj.AddComponent<SpecialStatTooltipTrigger>();

        trigger.Setup(ShowTooltip,HideTooltip, stat);
    }

    public void InvestStat(C_Enums.SpecialStat stat)
    {
        if (_special.TryInvestPont(stat))
        {
            Debug.Log($"{stat}에 포인트 투자 성공");
            RefreshSpecial();

            // 🌟 실시간 피드백: 버튼을 누르는 순간 오픈된 툴팁 내용도 스탯 수치에 맞게 바로 동적 새로고침
            ShowTooltip(stat);
        }
        else
        {
            Debug.Log("포인트 투자 실패");
        }
    }

    public void ResetStat()
    {
        _special.ResetSpecialStat();
        RefreshSpecial();
    }

    public void RefreshSpecial()
    {
        remainPointText.text = $"{_special._remainPoint}P";

        _statS1Text.text = $"{_special._currentStatState[SpecialStat.S1]} 포인트";
        _statS2Text.text = $"{_special._currentStatState[SpecialStat.S2]} 포인트";
        _statS3Text.text = $"{_special._currentStatState[SpecialStat.S3]} 포인트";
        _statS4Text.text = $"{_special._currentStatState[SpecialStat.S4]} 포인트";
        _statS5Text.text = $"{_special._currentStatState[SpecialStat.S5]} 포인트";
    }

    public void ShowTooltip(C_Enums.SpecialStat stat)
    {
        if (_tooltipUI == null) return;

        // 1. 헤더에 표시할 순수 특성명 백업
        string statName = GetStatName(stat);

        // 2. 현재 저장된 특성 포인트 데이터 백업
        int investedPoints = _special._currentStatState[stat];

        // 4. 스크린샷과 동일한 연두색 다이아몬드 불릿 및 수치 강조 스타일링 (#1DDB16)
        string bullet = "<color=#1DDB16>◆</color> ";
        string description = "";

        // 특성별 고유 텍스트 분기 처리
        switch (stat)
        {
            case SpecialStat.S1: // 분노
                description += $"{bullet}공격력이 <color=#1DDB16>{investedPoints * 3f:F2}%</color> 증가합니다.\n";
                break;

            case SpecialStat.S2: // 민첩
                description += $"{bullet}이동 속도가 <color=#1DDB16>{investedPoints * 2f:F2}%</color> 증가합니다.\n";
                description += $"{bullet}공격 속도가 <color=#1DDB16>{investedPoints * 4f:F2}%</color> 증가합니다.\n";
                break;

            case SpecialStat.S3: // 분쇄
                description += $"{bullet}무력화 피해가 <color=#1DDB16>{investedPoints * 5f:F2}%</color> 증가합니다.\n";
                break;

            case SpecialStat.S4: // 행운
                description += $"{bullet}치명타 확률이 <color=#1DDB16>{investedPoints * 2f:F2}%</color> 증가합니다.\n";
                description += $"{bullet}치명타 피해가 <color=#1DDB16>{investedPoints * 5f:F2}%</color> 증가합니다.\n";
                break;

            case SpecialStat.S5: // 강인
                description += $"{bullet}최대 체력이 <color=#1DDB16>{investedPoints * 5f:F2}%</color> 영구 보정됩니다.\n";
                description += $"{bullet}받는 모든 피해가 <color=#1DDB16>{investedPoints * 1f:F2}%</color> 감소합니다.\n";
                break;
        }

        // 5. 정돈된 타이틀과 본문을 최종 전송합니다.
        _tooltipUI.Show(statName, description);
    }

    public void HideTooltip()
    {
        if (_tooltipUI != null) _tooltipUI.Hide();
    }

    private string GetStatName(C_Enums.SpecialStat stat)
    {
        switch (stat)
        {
            case SpecialStat.S1: return "분노";
            case SpecialStat.S2: return "민첩";
            case SpecialStat.S3: return "분쇄";
            case SpecialStat.S4: return "행운";
            case SpecialStat.S5: return "강인";
            default: return "미확인 특성";
        }
    }
}
