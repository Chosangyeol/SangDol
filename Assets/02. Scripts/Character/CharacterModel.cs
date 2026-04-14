using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static C_Enums;
using static UnityEngine.EventSystems.EventTrigger;

public class CharacterModel : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera mainCam;
    public LayerMask groundLayer;
    public GameObject camContainer;
    public CinemachineVirtualCamera[] cams;
    public Transform textPos;

    [Header("캐릭터 기본 설정")]
    public CharacterStatSO characterStatSO;
    public int inventorySlotSize = 30;
    public LayerMask interactableLayer;
    public float interactableDistance = 3f;

    [Header("스킬 설정")]
    public Skill_ZSO skill_ZSO;
    public Skill_SpaceSO skill_SpaceSO;

    [Header("캐릭터 상태")]
    public bool canMove = true;
    public bool canUse = true;
    public bool isDie = false;

    private Animator anim;
    public Animator Anim => anim;

    private NavMeshAgent navMesh;
    public NavMeshAgent Navmesh => navMesh;

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

    private C_Buff buff;
    public C_Buff Buff => buff;


    private void Awake()
    {
        if (mainCam == null) mainCam = Camera.main;

        navMesh = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        stat = new C_Stat(this, characterStatSO);
        specialStat = new C_SpecialStat(this);
        inventory = new C_Inventory(this, inventorySlotSize);
        equipment = new C_Equipment(this);
        playerController = new C_Controller(this);
        playerInput = new C_Input(this, playerController);
        skillSystem = new C_SkillSystem(this);
        buff = new C_Buff(this);

        DontDestroyOnLoad(this.gameObject);

        DontDestroyOnLoad(camContainer);

        ChangeCam(0);
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
        if (isDie) return;

        playerController.Tick();
        buff.UpdateBuff(Time.deltaTime);
        skillSystem.UpdateSkills(Time.deltaTime);

        if (Buff.isStun)
        {
            playerController.StopMove();
            return;
        }  
    }

    public void SkillCorutaine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    public void SetCanMove()
    {
        canMove = true;
    }

    public void ChangeCam(int index)
    {
        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].Priority = 0;
            if (i == index)
                cams[i].Priority = 99;
        }
    }

    #region 일반 공격
    public void OnComboStart()
    {
        if (playerController.nextAttackReady || playerController.isAttackHeld)
            playerController.StartAttackCombo();
    }

    public void OnAttackEnd()
    {
        if (playerController.isAttackHeld)
        {
            playerController.currentCombo = 0; // 콤보 초기화
            playerController.StartAttackCombo();
        }
        else
        {
            // 마우스를 뗐다면 깔끔하게 공격 상태 완전 종료
            playerController.isAttacking = false;
            canMove = true;
            playerController.nextAttackReady = false;
            playerController.currentCombo = 0;
        }
    }

    public void OnAttackHit()
    {
        float hitRadius = 3f;
        float hitAngle = 90f;
        float damageMultiplier = 1f;

        switch (playerController.currentCombo)
        {
            case 1:
                hitRadius = 3f;
                hitAngle = 90f;
                damageMultiplier = 1f; // 첫 번째 공격은 기본 데미지
                break;
            case 2:
                hitRadius = 3f;
                hitAngle = 90f;
                damageMultiplier = 1f; // 두 번째 공격은 20% 증가
                break;
            case 3:
                hitRadius = 4f;
                hitAngle = 90f;
                damageMultiplier = 1.2f; // 세 번째 공격은 50% 증가
                break;
            default:
                damageMultiplier = 1.5f;
                HandleBasicAttack4(damageMultiplier);
                return;
        }

        Collider[] targets = Physics.OverlapSphere(transform.position, hitRadius);

        float baseDmg = Stat.Stat.attackDamage.FinalValue * damageMultiplier;
        bool isCritical = GetCritical();

        if (isCritical)
            baseDmg *= Stat.Stat.criticalDamage.FinalValue;

        SDamageInfo info = new SDamageInfo()
        {
            damage = baseDmg,
            source = this.gameObject,
            knockDownPower = 1,
            isCounterable = true,
            isCritical = isCritical,
            isHeadattack = false,
            isBackattack = true
        };

        foreach (Collider target in targets)
        {
            if (target.TryGetComponent<EnemyBase>(out EnemyBase enemy))
            {
                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                dir.y = 0;
                Vector3 myForward = transform.forward;
                myForward.y = 0;

                float angle = Vector3.Angle(myForward, dir);

                if (angle <= hitAngle / 2f)
                {
                    enemy.Damaged(info);
                    if (target.TryGetComponent<BossModel>(out BossModel boss))
                    {
                        GameEvent.OnBossStateChange?.Invoke(boss);
                    }
                }
            }

            if (target.TryGetComponent<ICounterable>(out ICounterable counterable))
            {
                // (선택 사항) 플레이어가 허공에 칼을 휘둘렀는데 뒤에 있던 룩이 카운터 맞는 것을 방지하려면,
                // 카운터도 공격 범위(부채꼴) 안에 있을 때만 발동하도록 각도 검사를 해주는 것이 좋습니다.
                Vector3 dir = (target.transform.position - transform.position).normalized;
                dir.y = 0;
                float angle = Vector3.Angle(transform.forward, dir);

                if (angle <= hitAngle / 2f)
                {
                    counterable.OnCounterSuccess(info);
                }
            }
        }

       
        canMove = true;

    }

    private void HandleBasicAttack4(float damageMultiplier)
    {
        Vector3 size = new Vector3(2f, 2f, 3f);

        Vector3 center = transform.position + transform.forward * size.z;
        center.y += 0.5f;

        Collider[] targets = Physics.OverlapBox(center, size, transform.rotation);

        float baseDmg = Stat.Stat.attackDamage.FinalValue * damageMultiplier;

        bool isCritical = GetCritical();

        if (isCritical)
            baseDmg *= Stat.Stat.criticalDamage.FinalValue;

        SDamageInfo info = new SDamageInfo()
        {
            damage = baseDmg,
            source = this.gameObject,
            knockDownPower = 1,
            isCounterable = true,
            isCritical = isCritical,
            isHeadattack = true,
            isBackattack = false
        };

        foreach (Collider target in targets)
        {
            if (target.TryGetComponent<EnemyBase>(out EnemyBase enemy))
            {
                enemy.Damaged(info);
                if (target.TryGetComponent<BossModel>(out BossModel boss))
                {
                    GameEvent.OnBossStateChange?.Invoke(boss);
                }
            }

            if (target.TryGetComponent<ICounterable>(out ICounterable counterable))
            {
                counterable.OnCounterSuccess(info);
            }
        }
    }
    #endregion

    #region 캐릭터 상태 및 상태이상
    public void StunEnable()
    {
        Buff.StunEnable();
        Anim.SetBool("isStun", true);
    }

    public void StunDisable()
    {
        Buff.StunDisable();
        Anim.SetBool("isStun", false);
    }

    public void ImmunityEnable()
    {
        Buff.ImmunityEnable();
    }

    public void ImmunityDisable()
    {
        Buff.ImmunityDisable();
    }

    public void PanicEnable()
    {
        Buff.PanicEnable();
        GameEvent.OnPlayerPanic?.Invoke(buff.isPanic);
    }

    public void PanicDisable()
    {
        Buff.PanicDisable();
        GameEvent.OnPlayerPanic?.Invoke(buff.isPanic);
    }
    #endregion

    #region 캐릭터 스탯 관리
    public void Damaged(float damage,bool isPercent)
    {
        Stat.Damaged(damage,isPercent);
        
        if (DamageTextManager.Instance != null)
        {
            float finalDamage = Stat.Stat.maxHp.FinalValue * damage;

            DamageTextManager.Instance.SpawnDamageText(textPos.position, finalDamage, false, true);
        }

        if (stat.Stat.curHp <= 0 && !isDie)
        {
            // 캐릭터 사망 처리
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("캐릭터가 사망했습니다.");

        canMove = false;
        isDie = true;

        skillSystem.ResetSkillCooldown();
        buff.RemoveAllBuff();

        GameEvent.OnPlayerDie?.Invoke();
    }

    public void Heal(float healAmount)
    {
        Stat.Heal(healAmount);
    }

    public void Revive()
    {
        Heal(Stat.Stat.maxHp.FinalValue);
        isDie = false;
        canMove = true;

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
        Stat.GainExp(amount);
    }

    public void GainGold(int amount)
    {
        Stat.GainGold(amount);
    }

    public bool GetCritical()
    {
        return (Stat.GetCritical());
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
            Stat.AddMoveSpeed(isFlat,value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.AddAttackSpeed(isFlat,value);
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

        navMesh.speed = Stat.Stat.moveSpeed.FinalValue;
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
            Stat.RemoveMoveSpeed(isFlat, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.RemoveAttackSpeed(isFlat, value);
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

        navMesh.speed = Stat.Stat.moveSpeed.FinalValue;

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
        Gizmos.DrawWireSphere(transform.position, 3f); 
        
        Gizmos.DrawCube(transform.position + transform.forward * 3f, new Vector3(2f, 2f, 6f));
    }
#endif
}
