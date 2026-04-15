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
        Option,
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

    public enum SFX_List
    {
        Player_Die,
        Player_Attack1,
        Player_Attack2,
        Player_Attack3,
        Player_Attack4,
        Player_Iden,
        Player_Space,
        Player_Skill1,
        Player_Skill2,
        Player_Skill3,
        Player_Skill4,
        UI_Click,
        D1_Final_Box,
        D1_Final_Landing,
        D1_Final_Shot,
        D1_Final_N1,
        D1_Final_N2,
        D1_Final_N3,
        D1_Final_N4,
        D1_Final_N5,
        D1_Final_S1,
        D1_Final_S2,
        D1_Final_S3,
        D1_Final_S4,
        D1_Final_S5,
        D1_Final_Enter
    }

    public enum BGM_List
    {
        Title,
        D1_Final_BGM1,
        D1_Final_BGM2,
        D1_Final_BGM3
    }
}
