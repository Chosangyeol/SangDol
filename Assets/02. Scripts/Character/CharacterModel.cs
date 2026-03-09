using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera mainCam;
    public LayerMask groundLayer;

    [Header("캐릭터 기본 설정")]
    public CharacterStatSO characterStatSO;
    public int inventorySlotSize = 30;

    [SerializeField]
    public List<Item> testItems;

    public C_Stat Stat => stat;
    private C_Stat stat;
    public C_Inventory Inventory => inventory;
    private C_Inventory inventory;
    public C_Equipment Equipment => equipment;
    private C_Equipment equipment;
    public C_Input PlayerInput => playerInput;
    private C_Input playerInput;
    public C_Controller PlayerController => playerController;
    private C_Controller playerController;
    public C_SkillSystem SkillSystem => skillSystem;
    private C_SkillSystem skillSystem;

    private void Awake()
    {
        if (mainCam == null) mainCam = Camera.main;

        stat = new C_Stat(this, characterStatSO);
        inventory = new C_Inventory(this, inventorySlotSize);
        equipment = new C_Equipment(this);
        playerController = new C_Controller(this);
        playerInput = new C_Input(this, playerController);
        skillSystem = new C_SkillSystem(this);

    }

    private void Start()
    {
        for (int i = 0; i < testItems.Count; i++)
        {
            Inventory.AddItem(testItems[i].GetItem());
        }
    }

    private void Update()
    {
        playerController.Tick();
        skillSystem.UpdateSkills(Time.deltaTime);
    }

    public void SkillCorutaine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    public void Damaged(float damage)
    {
        Stat.Damaged(damage);
        if (stat.Stat.curHp <= 0)
        {
            // 캐릭터 사망 처리
            Debug.Log("캐릭터가 사망했습니다.");
        }
    }

    public void Heal(float healAmount)
    {
        Stat.Heal(healAmount);
    }

    public void AddStat(C_Enums.CharacterStat statType,bool isFlat, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHP)
        {
            Stat.AddMaxHp(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackDamage)
        {
            Stat.AddAttackDamage(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.Defense)
        {
            Stat.AddDefense(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.MoveSpeed)
        {
            Stat.AddMoveSpeed(value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.AddAttackSpeed(value);
        }
        else if (statType == C_Enums.CharacterStat.DownPower)
        {
            Stat.AddDownPower(value);
        }
        else if (statType == C_Enums.CharacterStat.CirticalChance)
        {
            Stat.AddCirticalChance(value);
        }
        else if (statType == C_Enums.CharacterStat.CirticalDamage)
        {
            Stat.AddCirticalDamage(value);
        }
    }

    public void RemoveStat(C_Enums.CharacterStat statType, bool isFlat, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHP)
        {
            Stat.RemoveMaxHp(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackDamage)
        {
            Stat.RemoveAttackDamage(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.Defense)
        {
            Stat.RemoveDefense(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.MoveSpeed)
        {
            Stat.RemoveMoveSpeed(value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.RemoveAttackSpeed(value);
        }
        else if (statType == C_Enums.CharacterStat.DownPower)
        {
            Stat.RemoveDownPower(value);
        }
        else if (statType == C_Enums.CharacterStat.CirticalChance)
        {
            Stat.RemoveCirticalChance(value);
        }
        else if (statType == C_Enums.CharacterStat.CirticalDamage)
        {
            Stat.RemoveCirticalDamage(value);
        }
    }
}
