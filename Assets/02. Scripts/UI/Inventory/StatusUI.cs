using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    private C_Stat _stat;
    private C_Equipment _equipment;

    public void Init(C_Stat stat, C_Equipment equipment)
    {
        _stat = stat;
        _equipment = equipment;

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].Init(equipment, equipmentSlots[i].equipType);
        }

        BindStatusEvents();
        RefreshAll();
    }

    private void BindStatusEvents()
    {
        _equipment.OnEquipItem += OnStatusChange;
        _equipment.OnUnequipItem += OnStatusChange;
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
        criticalChanceText.text = $"Critical Chance: {_stat.Stat.cirticalChance.GetValue() * 100}%";
        criticalDamageText.text = $"Critical Damage: {_stat.Stat.cirticalDamage.GetValue() * 100}%";
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
