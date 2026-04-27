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
    

    private C_SpecialStat _special;

    public void Init(C_SpecialStat special)
    {
        _special = special;

        _s1Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S1));
        _s2Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S2));
        _s3Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S3));
        _s4Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S4));
        _s5Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S5));

        RefreshSpecial();
    }

    public void InvestStat(C_Enums.SpecialStat stat)
    {
        if (_special.TryInvestPont(stat))
        {
            Debug.Log($"{stat}에 포인트 투자 성공");
        }
        else
        {
            Debug.Log("포인트 투자 실패");
        }
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
}
