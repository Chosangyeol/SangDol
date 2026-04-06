using UnityEngine;

public class C_Enums
{
    public enum SkillSlot
    {
        Z,
        Q,
        W,
        E,
        R,
        Space,
        V
    }

    public enum UseSlot
    {
        None,
        Slot_1,
        Slot_2,
        Slot_3,
        Slot_4
    }

    public enum  UIList
    {
        Inventory,
        SkillTree,
        Status,
        Quest
    }

    public enum CharacterStat
    {
        None,
        MaxHp,
        AttackDamage,
        Defense,
        MoveSpeed,
        AttackSpeed,
        DownPower,
        CriticalChance,
        CriticalDamage
    }

    public enum SpecialStat
    {
        S1,
        S2,
        S3,
        S4,
        S5
         
    }
    public enum EnemyType
    {
        Normal,
        Elite,
        Boss
    }
}
