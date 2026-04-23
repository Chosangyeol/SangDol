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

    // 일반 스탯
    public StatValue maxHp;
    public float curHp;

    public StatValue attackDamage; 

    public StatValue moveSpeed;
    public StatValue attackSpeed;

    public StatValue criticalChance;
    public StatValue criticalDamage;

    // 특수 스탯
    /// <summary>
    /// 아이덴티티 획득량 증가 %
    /// </summary>
    public StatValue idenBonus;
    /// <summary>
    /// 무력화 피해 증가 %
    /// </summary>
    public StatValue downPower;
    /// <summary>
    /// 쿨타임 감소 %
    /// </summary>
    public StatValue cooldownReduction;
    /// <summary>
    /// 받는 피해 배율 %
    /// </summary>
    public StatValue damageTakeMultiplier;
    public float dodgeCooldownReduction;

    public int gold;
    public int statPoint;

    public CharacterStat(CharacterStatSO statSO, string name = "테스트")
    {
        // 고정값
        this.characterName = name;
        this.currentLevel = 1;
        this.maxExp = 100;
        this.idenMax = 100;
        this.idenCurrent = 100;

        // 일반 스탯
        this.maxHp = new StatValue(statSO.maxHp);
        this.curHp = maxHp.FinalValue;

        this.attackDamage = new StatValue(statSO.attackDamage);

        this.moveSpeed = new StatValue(statSO.moveSpeed);
        this.attackSpeed = new StatValue(statSO.attackSpeed);

        this.criticalChance = new StatValue(statSO.criticalChance);
        this.criticalDamage = new StatValue(statSO.criticalDamage);

        // 특수 스탯
        this.idenBonus = new StatValue(statSO.idenBonus);
        this.downPower = new StatValue(statSO.downPower);
        this.cooldownReduction = new StatValue(1.0f);
        this.damageTakeMultiplier = new StatValue(1.0f);
        this.dodgeCooldownReduction = 0f;

        gold = 1000;
        statPoint = 0;
    }

    public void Damaged(float damage, bool isPercent)
    {
        int finalDamage = 0;

        if (!isPercent)
        {
            finalDamage = Mathf.RoundToInt(damage);
        }
        else
        {
            finalDamage = Mathf.RoundToInt(maxHp.FinalValue * damage);
        }

        // 받는 피해 비율 체크
        finalDamage = Mathf.RoundToInt(finalDamage * damageTakeMultiplier.FinalValue);

        curHp -= finalDamage;

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
        Debug.Log($"{gold}원 보유");
    }

    public bool GetCritical()
    {
        int random = UnityEngine.Random.Range(1, 101);
        if (random <= criticalChance.FinalValue * 100)
            return true;
        else
            return false;
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
    public void AddMaxHp(bool isPercent, float value)
    {
        if (isPercent)
            maxHp.AddPercent(value);
        else
            maxHp.AddFlat(value);
    }

    public void AddAttackDamage(bool isPercent, float value)
    {
        if (isPercent)
            attackDamage.AddPercent(value);
        else
            attackDamage.AddFlat(value);
    }

    public void AddMoveSpeed(bool isPercent, float value)
    {
        if (isPercent)
            moveSpeed.AddPercent(value);
        else
            moveSpeed.AddFlat(value);
    }

    public void AddAttackSpeed(bool isPercent, float value)
    {
        if (isPercent)
            attackSpeed.AddPercent(value);
        else
            attackSpeed.AddFlat(value);
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
    public void RemoveMaxHp(bool isPercent, float value)
    {
        if (isPercent)
            maxHp.RemoveFlat(value);
        else
            maxHp.RemovePercent(value);
    }

    public void RemoveAttackDamage(bool isPercent, float value)
    {
        if (isPercent)
            attackDamage.RemoveFlat(value);
        else
            attackDamage.RemovePercent(value);
    }

    public void RemoveMoveSpeed(bool isPercent, float value)
    {
        if (isPercent)
            moveSpeed.RemovePercent(value);
        else
            moveSpeed.RemoveFlat(value);
    }

    public void RemoveAttackSpeed(bool isPercent, float value)
    {
        if (isPercent)
            attackSpeed.RemovePercent(value);
        else
            attackSpeed.RemoveFlat(value);
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
    // 특수 스탯
    public void AddDownPower(float value)
    {
        downPower.AddFlat(value);
    }

    public void AddIdenBonus(float value)
    {
        idenBonus.AddFlat(value);
    }

    public void AddCooldownReduction(float value)
    {
        cooldownReduction.AddFlat(value);
    }

    public void AddTakeMultiplier(float value)
    {
        damageTakeMultiplier.AddFlat(value);
    }

    public void AddDodgeCooldownReduction(float value)
    {
        dodgeCooldownReduction += value;
    }

    public void RemoveDownPower(float value)
    {
        downPower.RemoveFlat(value);
    }

    public void RemoveIdenBonus(float value)
    {
        idenBonus.RemoveFlat(value);
    }

    public void RemoveCooldownReduction(float value)
    {
        cooldownReduction.RemoveFlat(value);
    }

    public void RemoveTakeMultiplier(float value)
    {
        damageTakeMultiplier.RemoveFlat(value);
    }

    public void RemoveDodgeCooldownReduction(float value)
    {
        dodgeCooldownReduction -= value;
    }
    #endregion

    public void RecalculateAll()
    {
        maxHp.Recalculate();
        attackDamage.Recalculate();
        moveSpeed.Recalculate();
        attackSpeed.Recalculate();
        downPower.Recalculate();
        criticalChance.Recalculate();
        criticalDamage.Recalculate();
    }


}
