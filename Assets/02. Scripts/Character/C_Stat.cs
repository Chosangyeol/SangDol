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
    }

    public float GetCritical(float baseDamage)
    {
        return stat.GetCritical(baseDamage);
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
    }

    public void GainExp(float amount)
    {
        stat.GainExp(amount);
    }

    public void GainGold(int amount)
    {
        stat.GainGold(amount);
    }

    #region Add Stat Methods
    public void AddMaxHp(bool isFlat, float value)
    {
        stat.AddMaxHp(isFlat, value);
    }

    public void AddAttackDamage(bool isFlat, float value)
    {
        stat.AddAttackDamage(isFlat, value);
    }

    public void AddDefense(bool isFlat, float value)
    {
        stat.AddDefense(isFlat, value);
    }

    public void AddMoveSpeed(float value)
    {
        stat.AddMoveSpeed(value);
    }

    public void AddAttackSpeed(float value)
    {
        stat.AddAttackSpeed(value);
    }

    public void AddDownPower(float value)
    {
        stat.AddDownPower(value);
    }

    public void AddCirticalChance(float value)
    {
        stat.AddCirticalChance(value);
    }

    public void AddCirticalDamage(float value)
    {
        stat.AddCirticalDamage(value);
    }
    #endregion

    #region Remove Stat Methods
    public void RemoveMaxHp(bool isFlat, float value)
    {
        stat.RemoveMaxHp(isFlat, value);
    }

    public void RemoveAttackDamage(bool isFlat, float value)
    {
        stat.RemoveAttackDamage(isFlat, value);
    }

    public void RemoveDefense(bool isFlat, float value)
    {
        stat.RemoveDefense(isFlat, value);
    }

    public void RemoveMoveSpeed(float value)
    {
        stat.RemoveMoveSpeed(value);
    }

    public void RemoveAttackSpeed(float value)
    {
        stat.RemoveAttackSpeed(value);
    }

    public void RemoveDownPower(float value)
    {
        stat.RemoveDownPower(value);
    }

    public void RemoveCirticalChance(float value)
    {
        stat.RemoveCirticalChance(value);
    }

    public void RemoveCirticalDamage(float value)
    {
        stat.RemoveCirticalDamage(value);
    }
    #endregion
}
