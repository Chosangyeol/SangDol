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
    [SerializeField] TMP_Text defenseText;
    [SerializeField] TMP_Text criticalChanceText;
    [SerializeField] TMP_Text criticalDamageText;

    [Header("스텟 투자 UI")]
    [SerializeField] GameObject specialStat;

    [SerializeField] TMP_Text totalPointText;
    [SerializeField] TMP_Text remainPointText;
    [SerializeField] TMP_Text usedPointText;

    [SerializeField] Button _s1Button;
    [SerializeField] Button _s2Button;
    [SerializeField] Button _s3Button;
    [SerializeField] Button _s4Button;
    [SerializeField] Button _s5Button;

    [SerializeField] TMP_Text _statS1Text;
    [SerializeField] TMP_Text _statS2Text;
    [SerializeField] TMP_Text _statS3Text;
    [SerializeField] TMP_Text _statS4Text;
    [SerializeField] TMP_Text _statS5Text;


    private C_Stat _stat;
    private C_Equipment _equipment;
    private C_SpecialStat _special;

    public void Init(C_Stat stat, C_Equipment equipment, C_SpecialStat special)
    {
        _stat = stat;
        _equipment = equipment;
        _special = special;

        specialStat.SetActive(false);

        _s1Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S1));
        _s2Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S2));
        _s3Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S3));
        _s4Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S4));
        _s5Button.onClick.AddListener(() => InvestStat(C_Enums.SpecialStat.S5));

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Init(equipment, equipmentSlots[i].equipType, tooltip);
        }

        BindStatusEvents();
        RefreshAll();
    }

    private void BindStatusEvents()
    {
        GameEvent.OnStatChange += RefreshStatus;
    }


    private void OnStatusChange(EquipItemBase _)
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        foreach (var slot in equipmentSlots)
            slot.Refresh();

        RefreshStatus(_stat.Stat);
    }

    public void RefreshStatus(CharacterStat stat)
    {  
        hpText.text = $"체력: {stat.curHp} / {stat.maxHp.GetValue()}";
        levelText.text = $"레벨: {stat.currentLevel}";
        attackText.text = $"공격력: {stat.attackDamage.GetValue()}";
        defenseText.text = $"방어력: {stat.defense.GetValue()}";
        criticalChanceText.text = $"치명타 확률: {stat.criticalChance.GetValue() * 100}%";
        criticalDamageText.text = $"치명타 피해량: {stat.criticalDamage.GetValue() * 100}%";

        totalPointText.text = $"전체 스텟 포인트 : {_special._totalPoint}";
        remainPointText.text = $"남은 스텟 포인트 : {_special._remainPoint}";
        usedPointText.text = $"사용한 스텟 포인트 : {_special._usedPoint}";

        _statS1Text.text = $"{_special._currentStatState[SpecialStat.S1]} 포인트";
        _statS2Text.text = $"{_special._currentStatState[SpecialStat.S2]} 포인트";
        _statS3Text.text = $"{_special._currentStatState[SpecialStat.S3]} 포인트";
        _statS4Text.text = $"{_special._currentStatState[SpecialStat.S4]} 포인트";
        _statS5Text.text = $"{_special._currentStatState[SpecialStat.S5]} 포인트";
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            specialStat.SetActive(false);
            return;
        }

        gameObject.SetActive(!gameObject.activeSelf);
        specialStat.SetActive(false);
        RefreshStatus(_stat.Stat);
    }

    public void SpecialToggle()
    {
        specialStat.SetActive(!specialStat.activeSelf);
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
}
