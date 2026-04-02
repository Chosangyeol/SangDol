using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static C_Enums;

public class CharacterModel : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera mainCam;
    public LayerMask groundLayer;

    [Header("캐릭터 기본 설정")]
    public CharacterStatSO characterStatSO;
    public int inventorySlotSize = 30;
    public LayerMask interactableLayer;
    public float interactableDistance = 3f;

    private Animator anim;
    public Animator Anim => anim;

    [SerializeField]
    public List<Item> testItems;

    public C_Stat Stat => stat;
    private C_Stat stat;
    public C_SpecialStat SpecialStat => specialStat;
    private C_SpecialStat specialStat;

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

        anim = GetComponent<Animator>();

        stat = new C_Stat(this, characterStatSO);
        specialStat = new C_SpecialStat(this);
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

        GameEvent.OnStatChange?.Invoke(stat.Stat);
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

    public void OnComboStart()
    {
        if (playerController.nextAttackReady)
            playerController.StartAttackCombo();
    }

    public void OnAttackEnd()
    {
        playerController.isAttacking = false;
        playerController.canMove = false;
        playerController.currentCombo = 0;
    }

    public void OnAttackHit()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, 4f);

        foreach (Collider target in targets)
        {
            if (target.TryGetComponent<EnemyModel>(out EnemyModel enemy))
            {
                Vector3 dir = (enemy.transform.position - transform.position).normalized;

                dir.y = 0;
                Vector3 myForward = transform.forward;
                myForward.y = 0;

                float angle = Vector3.Angle(myForward, dir);

                if (angle <= 90 / 2f)
                    enemy.Damaged(GetCritical(Stat.Stat.attackDamage.FinalValue), gameObject);
                else
                    enemy.Damaged(Stat.Stat.attackDamage.FinalValue, gameObject);
            }
        }
        playerController.canMove = true;
    }

    #region 캐릭터 스탯 관리
    public void Damaged(float damage)
    {
        Stat.Damaged(damage);
        if (stat.Stat.curHp <= 0)
        {
            // 캐릭터 사망 처리
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("캐릭터가 사망했습니다.");
    }

    public void Heal(float healAmount)
    {
        Stat.Heal(healAmount);
    }

    public void GainExp(float amount)
    {
        Stat.GainExp(amount);
    }

    public void GainGold(int amount)
    {
        Stat.GainGold(amount);
    }

    public float GetCritical(float baseDamage)
    {
        return (Stat.GetCritical(baseDamage));
    }

    public void AddStat(C_Enums.CharacterStat statType,bool isFlat, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHp)
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
        else if (statType == C_Enums.CharacterStat.CriticalChance)
        {
            Stat.AddCirticalChance(value);
        }
        else if (statType == C_Enums.CharacterStat.CriticalDamage)
        {
            Stat.AddCirticalDamage(value);
        }
    }

    public void RemoveStat(C_Enums.CharacterStat statType, bool isFlat, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHp)
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
        else if (statType == C_Enums.CharacterStat.CriticalChance)
        {
            Stat.RemoveCirticalChance(value);
        }
        else if (statType == C_Enums.CharacterStat.CriticalDamage)
        {
            Stat.RemoveCirticalDamage(value);
        }
    }

    #endregion

    #region 상호작용
    public void TryInteract()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, interactableDistance, interactableLayer);

        if (targets.Length <= 0)
        {
            Debug.Log("상호작용 오브젝트 없음");
            return;
        }

        Collider target = null;
        float closest = 999;

        for (int i = 0; i < targets.Length; i++)
        {
            float dis = Vector3.Distance(this.transform.position, targets[i].transform.position);
            if (dis < closest)
            {
                closest = dis;
                target = targets[i];
            }
        }

        if (target != null)
            target.gameObject.GetComponent<IInteractable>().Interact(this.transform);
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 기즈모가 그려질 기준 위치 (발 밑 기준이면 transform.position, 가슴 높이면 약간 올림)
        Vector3 origin = transform.position;// + Vector3.up; // 필요에 따라 높이 조절

        // 1. 부채꼴의 왼쪽과 오른쪽 경계선 방향(Vector3)을 계산합니다.
        // transform.forward(정면)를 Y축 기준으로 총 각도의 절반만큼 좌우로 회전시킵니다.
        Vector3 leftBoundary = Quaternion.Euler(0, -90 / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 90 / 2f, 0) * transform.forward;

        // 2. 부채꼴 내부를 반투명하게 색칠합니다. (선택 사항, 시인성이 아주 좋아짐)
        Handles.color = new Color(1f, 0f, 0f, 0.2f); // 빨간색, 투명도 20%
        Handles.DrawSolidArc(origin, Vector3.up, leftBoundary, 90, 90);

        // 3. 부채꼴의 테두리 선을 그립니다.
        Handles.color = Color.yellow;
        Handles.DrawWireArc(origin, Vector3.up, leftBoundary, 90, 90);
        Handles.DrawLine(origin, origin + leftBoundary * 90);
        Handles.DrawLine(origin, origin + rightBoundary * 90);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 4f);   
    }
#endif
}
