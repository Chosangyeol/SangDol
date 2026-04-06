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

    public void Recalculate()
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

    public float idenMax;
    public float idenCurrent;

    public StatValue maxHp;
    public float curHp;

    public StatValue attackDamage;
    public StatValue defense;

    public StatValue moveSpeed;
    public StatValue attackSpeed;

    public StatValue idenBonus;
    public StatValue downPower;

    public StatValue criticalChance;
    public StatValue criticalDamage;

    public int gold;
    public int statPoint;

    public CharacterStat(CharacterStatSO statSO, string name = "테스트")
    {
        this.characterName = name;
        this.currentLevel = 1;
        this.maxExp = 100;

        this.idenMax = 100;
        this.idenCurrent = 100;

        this.maxHp = new StatValue(statSO.maxHp);
        this.curHp = maxHp.FinalValue;

        this.attackDamage = new StatValue(statSO.attackDamage);
        this.defense = new StatValue(statSO.defense);

        this.moveSpeed = new StatValue(statSO.moveSpeed);
        this.attackSpeed = new StatValue(statSO.attackSpeed);

        this.idenBonus = new StatValue(statSO.idenBonus);
        this.downPower = new StatValue(statSO.downPower);

        this.criticalChance = new StatValue(statSO.criticalChance);
        this.criticalDamage = new StatValue(statSO.criticalDamage);

        statPoint = 0;
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

    public void GainIden(float amount)
    {
        idenCurrent += amount * idenBonus.FinalValue;
        if (idenCurrent >= idenMax)
            idenCurrent = idenMax;
    }

    public void ResetIden()
    {
        idenCurrent = 0;
    }

    public void GainGold(int amount)
    {
        gold += amount;
        Debug.Log($"{gold}원 보유");
    }

    public void UseGold(int amount)
    {
        gold -= amount;
    }

    public float GetCritical(float baseDamage)
    {
        int random = UnityEngine.Random.Range(1, 101);
        if (random <= criticalChance.FinalValue * 100)
            return baseDamage * criticalDamage.FinalValue;
        else
            return baseDamage;
    }

    #region Level & Exp
    public void GainExp(float amount)
    {
        currentExp += amount;
    }

    public void LevelUp()
    {
        if (currentExp < maxExp) return;

        currentExp -= maxExp;
        currentLevel++;
        statPoint++;
        Debug.Log($"레벨업, 레벨 : {currentLevel} / 경험치 : {currentExp} / 스탯포인트 : {statPoint}");
        // 캐릭터 기본 스텟 추가

        // 최대 경험치량 증가 -> csv 파일로 받아올 예정


        if (currentExp >= maxExp)
            LevelUp();

    }

    #endregion

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
        criticalChance.AddFlat(value);
    }

    public void AddCirticalDamage(float value)
    {
        criticalDamage.AddFlat(value);
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
        criticalChance.RemoveFlat(value);
    }

    public void RemoveCirticalDamage(float value)
    {
        criticalDamage.RemoveFlat(value);
    }

    #endregion

    #region Special Stat Methods

    #endregion

    public void RecalculateAll()
    {
        maxHp.Recalculate();
        attackDamage.Recalculate();
        defense.Recalculate();
        moveSpeed.Recalculate();
        attackSpeed.Recalculate();
        downPower.Recalculate();
        criticalChance.Recalculate();
        criticalDamage.Recalculate();
    }


}
