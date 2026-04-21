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
    public void AddMaxHp(bool isFlat, float value)
    {
        stat.AddMaxHp(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);

    }

    public void AddAttackDamage(bool isFlat, float value)
    {
        stat.AddAttackDamage(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);

    }

    public void AddDefense(bool isFlat, float value)
    {
        stat.AddDefense(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);

    }

    public void AddMoveSpeed(bool isFlat, float value)
    {
        stat.AddMoveSpeed(isFlat,value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddAttackSpeed(bool isFlat, float value)
    {
        stat.AddAttackSpeed(isFlat,value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddDownPower(float value)
    {
        stat.AddDownPower(value);
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
    public void RemoveMaxHp(bool isFlat, float value)
    {
        stat.RemoveMaxHp(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveAttackDamage(bool isFlat, float value)
    {
        stat.RemoveAttackDamage(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveDefense(bool isFlat, float value)
    {
        stat.RemoveDefense(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveMoveSpeed(bool isFlat, float value)
    {
        stat.RemoveMoveSpeed(isFlat,value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveAttackSpeed(bool isFlat, float value)
    {
        stat.RemoveAttackSpeed(isFlat, value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveDownPower(float value)
    {
        stat.RemoveDownPower(value);
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
}
