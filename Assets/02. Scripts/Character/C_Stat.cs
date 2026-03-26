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

    public void Damaged(float damage)
    {
        stat.Damaged(damage);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public float GetCritical(float baseDamage)
    {
        return stat.GetCritical(baseDamage);
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
        GameEvent.OnStatChange?.Invoke(Stat);
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

    public void AddMoveSpeed(float value)
    {
        stat.AddMoveSpeed(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void AddAttackSpeed(float value)
    {
        stat.AddAttackSpeed(value);
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

    public void RemoveMoveSpeed(float value)
    {
        stat.RemoveMoveSpeed(value);
        GameEvent.OnStatChange?.Invoke(Stat);
    }

    public void RemoveAttackSpeed(float value)
    {
        stat.RemoveAttackSpeed(value);
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
