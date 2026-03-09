using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[Serializable]
public class StatValue
{
    public float baseValue;
    private float finalValue;
    public float FinalValue => finalValue;

    private float flatBonus;
    private float percentBonus;

    public StatValue(float baseValue)
    {
        this.baseValue = baseValue;
        Recalculate();
    }

    public void AddFlat(float value)
    {
        flatBonus += value;
        Recalculate();
    }

    public void AddPercent(float value)
    {
        percentBonus += value;
        Recalculate();
    }

    public void RemoveFlat(float value)
    {
        flatBonus -= value;
        Recalculate();
    }

    public void RemovePercent(float value)
    {
        percentBonus -= value;
        Recalculate();
    }

    public void ClearAll()
    {
        flatBonus = 0;
        percentBonus = 0;
    }

    private void Recalculate()
    {
        finalValue = (baseValue + flatBonus) * (1f + percentBonus);
    }

    public float GetValue()
    {
        Recalculate();
        return FinalValue;
    }

}
public class CharacterStat
{
    public string characterName;
    public int currentLevel;
    public float maxExp;
    public float currentExp;

    public StatValue maxHp;
    public float curHp;

    public StatValue attackDamage;
    public StatValue defense;

    public StatValue moveSpeed;
    public StatValue attackSpeed;
    public StatValue downPower;

    public StatValue cirticalChance;
    public StatValue cirticalDamage;

    public CharacterStat(CharacterStatSO statSO, string name = "Å×½ºÆ®")
    {
        this.characterName = name;
        this.currentLevel = 1;
        this.maxExp = 100;

        this.maxHp = new StatValue(statSO.maxHp);
        this.curHp = maxHp.FinalValue;

        this.attackDamage = new StatValue(statSO.attackDamage);
        this.defense = new StatValue(statSO.defense);

        this.moveSpeed = new StatValue(statSO.moveSpeed);
        this.attackSpeed = new StatValue(statSO.attackSpeed);
        this.downPower = new StatValue(statSO.downPower);

        this.cirticalChance = new StatValue(statSO.cirticalChance);
        this.cirticalDamage = new StatValue(statSO.cirticalDamage);
    }

    public void Damaged(float damage)
    {
        curHp -= damage;
        if (curHp <= 0)
            curHp = 0;

    }

    public void Heal(float amount)
    {
        curHp += amount;
        if (curHp > maxHp.FinalValue)
            curHp = maxHp.FinalValue;
    }


    #region Add Stat Methods
    public void AddMaxHp(bool isFlat, float value)
    {
        if (isFlat)
            maxHp.AddFlat(value);
        else
            maxHp.AddPercent(value);
    }

    public void AddAttackDamage(bool isFlat, float value)
    {
        if (isFlat)
            attackDamage.AddFlat(value);
        else
            attackDamage.AddPercent(value);
    }

    public void AddDefense(bool isFlat, float value)
    {
        if (isFlat)
            defense.AddFlat(value);
        else
            defense.AddPercent(value);
    }

    public void AddMoveSpeed(float value)
    {
        moveSpeed.AddFlat(value);
    }

    public void AddAttackSpeed(float value)
    {
        attackSpeed.AddFlat(value);
    }

    public void AddDownPower(float value)
    {
        downPower.AddFlat(value);
    }

    public void AddCirticalChance(float value)
    {
        cirticalChance.AddFlat(value);
    }

    public void AddCirticalDamage(float value)
    {
        cirticalDamage.AddFlat(value);
    }
    #endregion

    #region Remove Stat Methods
    public void RemoveMaxHp(bool isFlat, float value)
    {
        if (isFlat)
            maxHp.RemoveFlat(value);
        else
            maxHp.RemovePercent(value);
    }

    public void RemoveAttackDamage(bool isFlat, float value)
    {
        if (isFlat)
            attackDamage.RemoveFlat(value);
        else
            attackDamage.RemovePercent(value);
    }

    public void RemoveDefense(bool isFlat, float value)
    {
        if (isFlat)
            defense.RemoveFlat(value);
        else
            defense.RemovePercent(value);
    }

    public void RemoveMoveSpeed(float value)
    {
        moveSpeed.RemoveFlat(value);
    }

    public void RemoveAttackSpeed(float value)
    {
        attackSpeed.RemoveFlat(value);
    }

    public void RemoveDownPower(float value)
    {
        downPower.RemoveFlat(value);
    }

    public void RemoveCirticalChance(float value)
    {
        cirticalChance.RemoveFlat(value);
    }

    public void RemoveCirticalDamage(float value)
    {
        cirticalDamage.RemoveFlat(value);
    }

    #endregion


}
