using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static C_Enums;

public class StatusUI : MonoBehaviour
{
    [Header("장비 슬룻")]
    [SerializeField] List<EquipmentSlot> equipmentSlots;

    [Header("툴팁 UI")]
    [SerializeField] ItemTooltip tooltip;

    [Header("기본 스텟 UI")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text attackSpeedText;
    [SerializeField] TMP_Text moveSpeedText;
    [SerializeField] TMP_Text criticalChanceText;
    [SerializeField] TMP_Text criticalDamageText;

    [Header("특수스텟 UI")]
    [SerializeField] TMP_Text s1Amount;
    [SerializeField] TMP_Text s2Amount;
    [SerializeField] TMP_Text s3Amount;
    [SerializeField] TMP_Text s4Amount;
    [SerializeField] TMP_Text s5Amount;
    [SerializeField] SpecialPopup specialPopup;

    [Header("캐릭터 정보 각 UI")]
    [SerializeField] GameObject equip;
    [SerializeField] StigmaUI stigma;
    [SerializeField] SpecialUI special;

    private CharacterModel _model;
    private C_Stat _stat;
    private C_Equipment _equipment;
    private C_SpecialStat _special;

    public void Init(CharacterModel model,C_Stat stat, C_Equipment equipment, C_SpecialStat special)
    {
        _model = model;
        _stat = stat;
        _equipment = equipment;
        _special = special;

        stigma.Init(_model);
        this.special.Init(_special);

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Init(equipment, equipmentSlots[i].equipType, tooltip);
        }

        SetupTextTooltipTrigger(s1Amount.gameObject, C_Enums.SpecialStat.S1);
        SetupTextTooltipTrigger(s2Amount.gameObject, C_Enums.SpecialStat.S2);
        SetupTextTooltipTrigger(s3Amount.gameObject, C_Enums.SpecialStat.S3);
        SetupTextTooltipTrigger(s4Amount.gameObject, C_Enums.SpecialStat.S4);
        SetupTextTooltipTrigger(s5Amount.gameObject, C_Enums.SpecialStat.S5);

        ChangeStatusTap(0);

        BindStatusEvents();
        RefreshAll();
    }

    private void SetupTextTooltipTrigger(GameObject textObj, C_Enums.SpecialStat stat)
    {
        SpecialStatTooltipTrigger trigger = textObj.GetComponent<SpecialStatTooltipTrigger>();
        if (trigger == null) trigger = textObj.AddComponent<SpecialStatTooltipTrigger>();

        // 델리게이트 구조를 이용해 내부에 정의된 ShowTooltip, HideTooltip을 트리거에 위임합니다.
        trigger.Setup(ShowTooltip, HideTooltip, stat);
    }

    private void BindStatusEvents()
    {
        GameEvent.OnStatChange += RefreshStatus;
    }

    public void RefreshAll()
    {
        foreach (var slot in equipmentSlots)
            slot.Refresh();

        RefreshStatus(_stat.Stat);
    }

    public void RefreshStatus(CharacterStat stat)
    {
        nameText.text = $"{stat.characterName}";
        hpText.text = $"{stat.maxHp.GetValue()}";
        levelText.text = $"Lv.{stat.currentLevel}";
        attackText.text = $"{stat.attackDamage.GetValue()}";
        attackSpeedText.text = $"{stat.attackSpeed.GetValue() * 100}%";
        moveSpeedText.text = $"{stat.moveSpeed.GetValue()}";
        criticalChanceText.text = $"{stat.criticalChance.GetValue() * 100}%";
        criticalDamageText.text = $"{stat.criticalDamage.GetValue() * 100}%";

        if (_special != null)
        {
            s1Amount.text = $"{_special._currentStatState[SpecialStat.S1]}";
            s2Amount.text = $"{_special._currentStatState[SpecialStat.S2]}";
            s3Amount.text = $"{_special._currentStatState[SpecialStat.S3]}";
            s4Amount.text = $"{_special._currentStatState[SpecialStat.S4]}";
            s5Amount.text = $"{_special._currentStatState[SpecialStat.S5]}";
        }
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void ChangeStatusTap(int index)
    {
        switch(index)
        {
            case 0:
                equip.SetActive(true);
                stigma.gameObject.SetActive(false);
                special.gameObject.SetActive(false);
                RefreshAll();
                break;
            case 1:
                equip.SetActive(false);
                stigma.gameObject.SetActive(true);
                special.gameObject.SetActive(false);
                stigma.RefreshAllNodes();
                break;
            case 2:
                equip.SetActive(false);
                stigma.gameObject.SetActive(false);
                special.gameObject.SetActive(true);
                special.RefreshSpecial();
                break;
        }
    }

    public void ShowTooltip(C_Enums.SpecialStat stat)
    {
        if (specialPopup == null) return;

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
                description += $"{bullet}공격 속도가 <color=#1DDB16>{investedPoints * 2f:F2}%</color> 증가합니다.\n";
                description += $"{bullet}이동 속도가 <color=#1DDB16>{investedPoints * 4f:F2}%</color> 증가합니다.\n";
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
        specialPopup.Show(statName, description);
    }

    public void HideTooltip()
    {
        if (specialPopup != null) specialPopup.Hide();
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
