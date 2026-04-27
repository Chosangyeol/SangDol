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
    [SerializeField] TMP_Text criticalChanceText;
    [SerializeField] TMP_Text criticalDamageText;

    

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

        ChangeStatusTap(0);

        BindStatusEvents();
        RefreshAll();
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
        //criticalChanceText.text = $"치명타 확률: {stat.criticalChance.GetValue() * 100}%";
        //criticalDamageText.text = $"치명타 피해량: {stat.criticalDamage.GetValue() * 100}%";
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
    
}
