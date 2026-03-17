using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [SerializeField] List<EquipmentSlot> equipmentSlots;

    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text expText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] TMP_Text defenseText;
    [SerializeField] TMP_Text criticalChanceText;
    [SerializeField] TMP_Text criticalDamageText;

    [SerializeField] TMP_Text totalPointText;
    [SerializeField] TMP_Text remainPointText;
    [SerializeField] TMP_Text usedPointText;


    private C_Stat _stat;
    private C_Equipment _equipment;
    private C_SpecialStat _special;

    public void Init(C_Stat stat, C_Equipment equipment, C_SpecialStat special)
    {
        _stat = stat;
        _equipment = equipment;
        _special = special;

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Init(equipment, equipmentSlots[i].equipType);
        }

        BindStatusEvents();
        RefreshAll();
    }

    private void BindStatusEvents()
    {
        //_equipment.OnEquipItem += OnStatusChange;
        //_equipment.OnUnequipItem += OnStatusChange;
        _stat.OnStatChange += RefreshStatus;
    }


    private void OnStatusChange(EquipItemBase _)
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        foreach (var slot in equipmentSlots)
            slot.Refresh();

        RefreshStatus();
    }

    public void RefreshStatus()
    {  
        hpText.text = $"HP: {_stat.Stat.curHp} / {_stat.Stat.maxHp.GetValue()}";
        levelText.text = $"Level: {_stat.Stat.currentLevel}";
        expText.text = $"EXP: {_stat.Stat.currentExp} / {_stat.Stat.maxExp}";
        attackText.text = $"Attack: {_stat.Stat.attackDamage.GetValue()}";
        defenseText.text = $"Defense: {_stat.Stat.defense.GetValue()}";
        criticalChanceText.text = $"Critical Chance: {_stat.Stat.criticalChance.GetValue() * 100}%";
        criticalDamageText.text = $"Critical Damage: {_stat.Stat.criticalDamage.GetValue() * 100}%";

        totalPointText.text = $"전체 스텟 포인트 : {_special._totalPoint}";
        remainPointText.text = $"남은 스텟 포인트 : {_special._remainPoint}";
        usedPointText.text = $"사용한 스텟 포인트 : {_special._usedPoint}";
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        RefreshStatus();
    }
}
