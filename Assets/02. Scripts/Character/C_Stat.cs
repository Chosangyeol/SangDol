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

    public event Action OnStatChange;

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
        OnStatChange?.Invoke();
    }

    public float GetCritical(float baseDamage)
    {
        return stat.GetCritical(baseDamage);
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
        OnStatChange?.Invoke();
    }

    public void GainExp(float amount)
    {
        stat.GainExp(amount);
        if (stat.currentExp >= stat.maxExp)
            LevelUp();

        OnStatChange?.Invoke();
    }

    public void LevelUp()
    {
        stat.LevelUp();
        OnStatChange?.Invoke();
    }

    public void GainGold(int amount)
    {
        stat.GainGold(amount);
        OnStatChange?.Invoke();
    }

    #region Add Stat Methods
    public void AddMaxHp(bool isFlat, float value)
    {
        stat.AddMaxHp(isFlat, value);
        OnStatChange?.Invoke();

    }

    public void AddAttackDamage(bool isFlat, float value)
    {
        stat.AddAttackDamage(isFlat, value);
        OnStatChange?.Invoke();

    }

    public void AddDefense(bool isFlat, float value)
    {
        stat.AddDefense(isFlat, value);
        OnStatChange?.Invoke();

    }

    public void AddMoveSpeed(float value)
    {
        stat.AddMoveSpeed(value);
        OnStatChange?.Invoke();
    }

    public void AddAttackSpeed(float value)
    {
        stat.AddAttackSpeed(value);
        OnStatChange?.Invoke();
    }

    public void AddDownPower(float value)
    {
        stat.AddDownPower(value);
        OnStatChange?.Invoke();
    }

    public void AddCirticalChance(float value)
    {
        stat.AddCirticalChance(value);
        OnStatChange?.Invoke();
    }

    public void AddCirticalDamage(float value)
    {
        stat.AddCirticalDamage(value);
        OnStatChange?.Invoke();
    }
    #endregion

    #region Remove Stat Methods
    public void RemoveMaxHp(bool isFlat, float value)
    {
        stat.RemoveMaxHp(isFlat, value);
        OnStatChange?.Invoke();
    }

    public void RemoveAttackDamage(bool isFlat, float value)
    {
        stat.RemoveAttackDamage(isFlat, value);
        OnStatChange?.Invoke();
    }

    public void RemoveDefense(bool isFlat, float value)
    {
        stat.RemoveDefense(isFlat, value);
        OnStatChange?.Invoke();
    }

    public void RemoveMoveSpeed(float value)
    {
        stat.RemoveMoveSpeed(value);
        OnStatChange?.Invoke();
    }

    public void RemoveAttackSpeed(float value)
    {
        stat.RemoveAttackSpeed(value);
        OnStatChange?.Invoke();
    }

    public void RemoveDownPower(float value)
    {
        stat.RemoveDownPower(value);
        OnStatChange?.Invoke();
    }

    public void RemoveCirticalChance(float value)
    {
        stat.RemoveCirticalChance(value);
        OnStatChange?.Invoke();
    }

    public void RemoveCirticalDamage(float value)
    {
        stat.RemoveCirticalDamage(value);
        OnStatChange?.Invoke();
    }
    #endregion
}
