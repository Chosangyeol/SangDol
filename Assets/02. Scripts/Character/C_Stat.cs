using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class C_Stat
{
    private CharacterModel owner;
    public CharacterModel Owner => owner;

    private CharacterStatSO statSO;

    private CharacterStat stat;
    public CharacterStat Stat => stat;

    public C_Stat(CharacterModel model, CharacterStatSO statSO)
    {
        this.owner = model;
        this.statSO = statSO;
        this.stat = new CharacterStat(this.statSO);

        return;
    }

    public void Damaged(float damage,bool isPercent)
    {
        stat.Damaged(damage,isPercent);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public bool GetCritical()
    {
        return stat.GetCritical();
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void GainIden(float amount)
    {
        Stat.GainIden(amount);
    }

    public void ResetIden()
    {
        Stat.ResetIden();
    }

    public void GainExp(float amount)
    {
        stat.GainExp(amount);
        if (stat.currentExp >= stat.maxExp)
            LevelUp();

        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void LevelUp()
    {
        stat.LevelUp();
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void GainGold(int amount)
    {
        stat.GainGold(amount);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void UseGold(int amount)
    {
        stat.UseGold(amount);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    #region Add Stat Methods
    public void AddMaxHp(bool isPercent, float value)
    {
        stat.AddMaxHp(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);

    }

    public void AddAttackDamage(bool isPercent, float value)
    {
        stat.AddAttackDamage(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);

    }

    public void AddMoveSpeed(bool isPercent, float value)
    {
        stat.AddMoveSpeed(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddAttackSpeed(bool isPercent, float value)
    {
        stat.AddAttackSpeed(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddCirticalChance(float value)
    {
        stat.AddCirticalChance(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddCirticalDamage(float value)
    {
        stat.AddCirticalDamage(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }
    #endregion

    #region Remove Stat Methods
    public void RemoveMaxHp(bool isPercent, float value)
    {
        stat.RemoveMaxHp(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveAttackDamage(bool isPercent, float value)
    {
        stat.RemoveAttackDamage(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveMoveSpeed(bool isPercent, float value)
    {
        stat.RemoveMoveSpeed(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveAttackSpeed(bool isPercent, float value)
    {
        stat.RemoveAttackSpeed(isPercent, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveCirticalChance(float value)
    {
        stat.RemoveCirticalChance(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveCirticalDamage(float value)
    {
        stat.RemoveCirticalDamage(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }
    #endregion

    #region Special Stat Methods
    // 특수 스탯
    public void AddDownPower(float value)
    {
        stat.AddDownPower(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddIdenBonus(float value)
    {
        stat.AddIdenBonus(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddCooldownReduction(float value)
    {
        stat.AddCooldownReduction(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddTakeMultiplier(float value)
    {
        stat.AddTakeMultiplier(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddDodgeCooldownReduction(float value)
    {
        stat.AddDodgeCooldownReduction(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveDownPower(float value)
    {
        stat.RemoveDownPower(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveIdenBonus(float value)
    {
        stat.RemoveIdenBonus(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveCooldownReduction(float value)
    {
        stat.RemoveCooldownReduction(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveTakeMultiplier(float value)
    {
        stat.RemoveTakeMultiplier(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveDodgeCooldownReduction(float value)
    {
        stat.RemoveDodgeCooldownReduction(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }
    #endregion
}
