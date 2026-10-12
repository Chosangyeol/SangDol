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

    private int equipmentUpdateDepth;

    private void NotifyStatChanged()
    {
        if (equipmentUpdateDepth == 0) GameEvent.OnStatChange?.Invoke(Stat);
    }

    internal void UpdateEquipmentStats(Action update)
    {
        if (update == null) return;
        stat.BeginEquipmentUpdate();
        equipmentUpdateDepth++;
        try { update(); }
        finally
        {
            stat.EndEquipmentUpdate();
            equipmentUpdateDepth--;
            if (equipmentUpdateDepth == 0) GameEvent.OnStatChange?.Invoke(Stat);
        }
    }

    public void Damaged(float damage,bool isPercent)
    {
        if (owner != null && (owner.isDie || (owner.Buff != null && owner.Buff.isInvincibility))) return;
        stat.Damaged(damage,isPercent);
        NotifyStatChanged();
    }

    public bool GetCritical()
    {
        return stat.GetCritical();
    }

    public void Heal(float amount)
    {
        stat.Heal(amount);
        NotifyStatChanged();
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

        NotifyStatChanged();
    }

    public void LevelUp()
    {
        stat.LevelUp();
        NotifyStatChanged();
    }

    public void GainGold(int amount)
    {
        stat.GainGold(amount);
        NotifyStatChanged();
    }

    public void UseGold(int amount)
    {
        stat.UseGold(amount);
        NotifyStatChanged();
    }

    #region Add Stat Methods
    public void AddMaxHp(bool isPercent, float value)
    {
        stat.AddMaxHp(isPercent, value);
        NotifyStatChanged();

    }

    public void AddAttackDamage(bool isPercent, float value)
    {
        stat.AddAttackDamage(isPercent, value);
        NotifyStatChanged();

    }

    public void AddMoveSpeed(bool isPercent, float value)
    {
        stat.AddMoveSpeed(isPercent, value);
        NotifyStatChanged();
    }

    public void AddAttackSpeed(bool isPercent, float value)
    {
        stat.AddAttackSpeed(isPercent, value);
        NotifyStatChanged();
    }

    public void AddCirticalChance(float value)
    {
        stat.AddCirticalChance(value);
        NotifyStatChanged();
    }

    public void AddCirticalDamage(float value)
    {
        stat.AddCirticalDamage(value);
        NotifyStatChanged();
    }
    #endregion

    #region Remove Stat Methods
    public void RemoveMaxHp(bool isPercent, float value)
    {
        stat.RemoveMaxHp(isPercent, value);
        NotifyStatChanged();
    }

    public void RemoveAttackDamage(bool isPercent, float value)
    {
        stat.RemoveAttackDamage(isPercent, value);
        NotifyStatChanged();
    }

    public void RemoveMoveSpeed(bool isPercent, float value)
    {
        stat.RemoveMoveSpeed(isPercent, value);
        NotifyStatChanged();
    }

    public void RemoveAttackSpeed(bool isPercent, float value)
    {
        stat.RemoveAttackSpeed(isPercent, value);
        NotifyStatChanged();
    }

    public void RemoveCirticalChance(float value)
    {
        stat.RemoveCirticalChance(value);
        NotifyStatChanged();
    }

    public void RemoveCirticalDamage(float value)
    {
        stat.RemoveCirticalDamage(value);
        NotifyStatChanged();
    }
    #endregion

    #region Special Stat Methods
    // 특수 스탯
    public void AddDownPower(float value)
    {
        stat.AddDownPower(value);
        NotifyStatChanged();
    }

    public void AddIdenBonus(float value)
    {
        stat.AddIdenBonus(value);
        NotifyStatChanged();
    }

    public void AddCooldownReduction(float value)
    {
        stat.AddCooldownReduction(value);
        NotifyStatChanged();
    }

    public void AddTakeMultiplier(float value)
    {
        stat.AddTakeMultiplier(value);
        NotifyStatChanged();
    }

    public void AddDodgeCooldownReduction(float value)
    {
        stat.AddDodgeCooldownReduction(value);
        NotifyStatChanged();
    }

    public void RemoveDownPower(float value)
    {
        stat.RemoveDownPower(value);
        NotifyStatChanged();
    }

    public void RemoveIdenBonus(float value)
    {
        stat.RemoveIdenBonus(value);
        NotifyStatChanged();
    }

    public void RemoveCooldownReduction(float value)
    {
        stat.RemoveCooldownReduction(value);
        NotifyStatChanged();
    }

    public void RemoveTakeMultiplier(float value)
    {
        stat.RemoveTakeMultiplier(value);
        NotifyStatChanged();
    }

    public void RemoveDodgeCooldownReduction(float value)
    {
        stat.RemoveDodgeCooldownReduction(value);
        NotifyStatChanged();
    }
    #endregion
}
