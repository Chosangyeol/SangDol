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

    [Header("성흔 설정")]
    public BuffSO lv5ABuffSO;
    public BuffSO lv5BBuffSO;
    public BuffSO lv6ABuffSO;
    public BuffSO lv10ABuffSO;
    public BuffSO stunSO;
    public GameObject clonePrefeb;

    [Header("아덴 설정")]
    public float attackTick = 0.2f;
    public float damageMultiplier = 0.5f;
    private Coroutine attackCoroutine;
    public PoolableMono idenActiveEffect;
    public PoolableMono idenEnableEffect;
    private PoolableMono idenEffectObject;
    public PoolableMono idenAttack1Effect;
    public PoolableMono idenChargeEffect;
    public PoolableMono idenAttack2Effect;

    [Header("캐릭터 상태")]
    public bool canMove = true;
    public bool canUse = true;
    public bool canAttack = true;
    public bool canSkill = true;
    public bool isDie = false;
    public bool isIdenOn = false;
    public bool isWaitingForRelease = false;

    private Animator anim;
    public Animator Anim => anim;

    private NavMeshAgent navMesh;
    public NavMeshAgent Navmesh => navMesh;

    private Coroutine _interactionRoutine;
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

    private C_Stigma stigma;
    public C_Stigma Stigma => stigma; 

    /// <summary>
    /// 캐릭터 이벤트
    /// </summary>
    public event Action<CharacterModel, float, bool, EnemyBase> OnHitTarget;
    public event Action<float> OnTakeDamage;
    public event Action OnSkillUsed;
    public event Action OnDodgeUsed;

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
        stigma = new C_Stigma(this, lv5ABuffSO, lv5BBuffSO,lv6ABuffSO,lv10ABuffSO, stunSO, clonePrefeb);

        GameEvent.OnStatChange += UpdateAttackSpeed;
    }

    private void Start()
    {

        cams = camContainer.GetComponentsInChildren<CinemachineVirtualCamera>(true);

        foreach (var cam in cams)
        {
            cam.Follow = this.transform;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitGameUIs();
        }

        ChangeCam(0, true);

        GameEvent.OnStatChange?.Invoke(stat.Stat);
    }

    private void Update()
    {
        if (isDie) return;

        playerController.Tick();
        buff?.UpdateBuff(Time.deltaTime);
        skillSystem?.UpdateSkills(Time.deltaTime);
        stigma?.UpdateStigma(Time.deltaTime);

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

    

    public void UseDodge()
    {
        OnDodgeUsed?.Invoke();
    }

    public void ChangeCam(int index, bool isCut = false)
    {
        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].Priority = 0;
            if (i == index)
                cams[i].Priority = 99;
        }

        if (isCut)
        {
            cams[index].gameObject.SetActive(false);
            cams[index].gameObject.SetActive(true);
        }
    }

    public void SetControlable(bool canControl)
    {
        navMesh.enabled = canControl;
        canAttack = canControl;
        canMove = canControl;
        canSkill = canControl;
        canUse = canControl;
    }

    #region 일반 공격
    public void OnComboStart()
    {
        if (isIdenOn)
        {
            if (attackCoroutine == null)
            {
                playerController.isAttacking = true;
                canMove = false; // 연타 중 이동 불가 처리

                // 연타 루프 애니메이션 실행 (Trigger가 아닌 Bool 사용)
                Anim.SetBool("IsIden", true);

                attackCoroutine = StartCoroutine(RapidAttackRoutine());
            }
        }
        // 2. 일반 상태일 때 -> 기존 콤보 공격 시작
        else
        {
            if (playerController.nextAttackReady || playerController.isAttackHeld)
                playerController.StartAttackCombo();
        }
    }

    public void OnAttackEnd()
    {
        if (isIdenOn || attackCoroutine != null || isWaitingForRelease)
        {
            playerController.isAttacking = false;
            canMove = true;

            Anim.SetBool("IsIden", false);

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (isWaitingForRelease)
            {
                isWaitingForRelease = false;
                RemoveIdenAura();            
            }
        }
        else
        {
            // (일반 공격 처리 로직 동일)
            if (playerController.isAttackHeld)
            {
                playerController.currentCombo = 0;
                playerController.StartAttackCombo();
            }
            else
            {
                playerController.isAttacking = false;
                playerController.nextAttackReady = false;
                playerController.currentCombo = 0;
            }
        }
    }

    private IEnumerator RapidAttackRoutine()
    {
        while (true) // 마우스를 떼서 OnAttackEnd가 호출될 때까지 무한 반복
        {
            PerformRapidHit(); // 데미지 판정
            yield return new WaitForSeconds(attackTick); // 틱 주기만큼 대기
        }
    }

    private void PerformRapidHit()
    {
        // 수라결의 히트박스 (원형 또는 박스형 등 기획에 맞게 수정)
        float hitRadius = 3.5f;
        float hitAngle = 120f; // 수라결은 범위가 좀 더 넓게 설정

        StartCoroutine(IdenRapidEffect());

        Collider[] targets = Physics.OverlapSphere(transform.position, hitRadius);

        float baseDmg = Stat.Stat.attackDamage.FinalValue * damageMultiplier;
        bool isCritical = GetCritical();
        if (isCritical) baseDmg *= Stat.Stat.criticalDamage.FinalValue;

        SDamageInfo info = new SDamageInfo()
        {
            damage = baseDmg,
            source = this.gameObject,
            knockDownPower = 1,
            isCounterable = true,
            isCritical = isCritical,
            isHeadattack = true, // 수라결은 보통 헤드어택
            isBackattack = false
        };

        HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

        foreach (Collider target in targets)
        {
            EnemyBase enemy = target.GetComponentInParent<EnemyBase>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                dir.y = 0;
                Vector3 myForward = transform.forward;
                myForward.y = 0;

                if (Vector3.Angle(myForward, dir) <= hitAngle / 2f)
                {
                    hitEnemies.Add(enemy); // 중복 타격 방지
                    enemy.Damaged(info);

                    if (target.TryGetComponent<BossModel>(out BossModel boss))
                        GameEvent.OnBossStateChange?.Invoke(boss);

                    OnHitTarget?.Invoke(this, info.damage, true, enemy);
                }
            }
        }
    }

    IEnumerator IdenRapidEffect()
    {
        PoolableMono effect = PoolManager.Instance.Pop(idenAttack1Effect.name);
        effect.transform.position = transform.position + transform.forward * 2f; // 예시로 캐릭터 머리 위에 위치
        effect.transform.rotation = Quaternion.LookRotation(transform.forward);
        yield return new WaitForSeconds(attackTick);
        PoolManager.Instance.Push(effect);
    }

    public void IdenFinalAttack()
    {
        StartCoroutine(IdenFinalAttackEffect());
    }

    IEnumerator IdenFinalAttackEffect()
    {
        PoolableMono effect = PoolManager.Instance.Pop(idenAttack2Effect.name);
        effect.transform.position = transform.position + transform.forward;
        effect.transform.rotation = Quaternion.LookRotation(transform.forward);
        yield return new WaitForSeconds(1f);
        PoolManager.Instance.Push(effect);
    }

    private void RemoveIdenAura()
    {
        if (isIdenOn) return;

        if (idenEffectObject != null)
        {
            idenEffectObject.transform.SetParent(null);
            PoolManager.Instance.Push(idenEffectObject);
            idenEffectObject = null;
        }
    }

    public void OnAttackHit()
    {
        float hitRadius = 3f;
        float hitAngle = 90f;
        float damageMultiplier = 1f;

        switch (playerController.currentCombo)
        {
            case 0:
                hitRadius = 3f;
                hitAngle = 90f;
                damageMultiplier = 1f; // 첫 번째 공격은 기본 데미지
                break;
            case 1:
                hitRadius = 3f;
                hitAngle = 90f;
                damageMultiplier = 1f; // 두 번째 공격은 20% 증가
                break;
            case 2:
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

        HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

        foreach (Collider target in targets)
        {
            EnemyBase enemy = target.GetComponentInParent<EnemyBase>();

            if (enemy != null && !hitEnemies.Contains(enemy))
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
                    OnHitTarget?.Invoke(this, info.damage, true, enemy);
                }
            }

            if (target.TryGetComponent<ICounterable>(out ICounterable counterable))
            {
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

        HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

        foreach (Collider target in targets)
        {
            EnemyBase enemy = target.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Damaged(info);
                if (target.TryGetComponent<BossModel>(out BossModel boss))
                {
                    GameEvent.OnBossStateChange?.Invoke(boss);
                }
                OnHitTarget?.Invoke(this, info.damage, true, enemy);

                ICounterable counterable = enemy.GetComponentInParent<ICounterable>();
                if (counterable != null)
                {
                    counterable.OnCounterSuccess(info);
                }
            }        
        }
        

        canMove = true;
    }


    public void PlayAttackSound(int num)
    {
        if (num < 2)
            AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack1);
        else if (num == 2)
            AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack2);
        else if (num == 3)
            AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack4);
    }
    #endregion

    #region 캐릭터 상태 및 상태이상
    public void ControlEnable()
    {
        SetCanAttack();
        SetCanMove();
        SetCanSkill();
    }

    public void ControlDisable()
    {
        SetCantAttack();
        SetCantMove();
        SetCantSkill();
    }

    public void SetCanMove()
    {
        canMove = true;
    }

    public void SetCantMove()
    {
        canMove = false;
    }

    public void SetCanAttack()
    {
        canAttack = true;
    }

    public void SetCantAttack()
    {
        canAttack = false;
    }

    public void SetCanSkill()
    {
        canSkill = true;
    }

    public void SetCantSkill()
    {
        canSkill = false;
    }

    public void StunEnable()
    {
        Buff.StunEnable();
        Anim.SetBool("IsStun", true);
    }

    public void StunDisable()
    {
        Buff.StunDisable();
        Anim.SetBool("IsStun", false);
    }

    public void ImmunityEnable()
    {
        Buff.ImmunityEnable();
    }

    public void ImmunityDisable()
    {
        Buff.ImmunityDisable();
    }

    public void InvincibilityEnable()
    {
        Buff.InvincibilityEnable();
    }

    public void InvincibilityDisable()
    {
        Buff.InvincibilityDisable();
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

    public void IdenEnable()
    {
        canMove = false;
        canAttack = false;
        Anim.SetTrigger("IdenOn");
        isIdenOn = true;

        idenEffectObject = PoolManager.Instance.Pop(idenEnableEffect.name);

        idenEffectObject.transform.SetParent(this.transform);
        idenEffectObject.transform.localPosition = new Vector3(0, 0, 0);

    }

    public void IdenDisable()
    {
        isIdenOn = false;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            Anim.SetBool("IsIden", false);

            Anim.SetTrigger("IdenFinish"); // 막타 애니메이션 실행

            isWaitingForRelease = true; // 막타 치는 중이니 마우스 뗄 때까지 기다리라고 상태 변경
            playerController.isAttacking = true;

        }
        else
        {
            // 2. 가만히 서있거나 이동 중에 지속시간이 끝났다면
            canMove = true;
            canAttack = true;

            RemoveIdenAura();
        }
    }

    #endregion

    #region 캐릭터 스탯 관리
    public void Damaged(float damage,bool isPercent)
    {
        CancelInteraction();

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

        anim.SetTrigger("Die");

        if (DungeonManager.instance != null)
            DungeonManager.instance.ReplacePlayer();
        GameEvent.OnPlayerDie?.Invoke();
        GameEvent.OnBossStateChange?.Invoke(null);
    }

    public void Heal(float healAmount)
    {
        Stat.Heal(healAmount);
    }

    public void Revive()
    {
        Heal(Stat.Stat.maxHp.FinalValue);

        Anim.SetTrigger("Revive");

        isDie = false;
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
    
    public void UseGold(int amount)
    {
        Stat.UseGold(amount);
    }

    public bool GetCritical()
    {
        return (Stat.GetCritical());
    }

    public void UpdateAttackSpeed(CharacterStat stat)
    {
        if (anim == null || stat == null) return;

        float currentAttackSpeed = stat.attackSpeed.FinalValue;

        anim.SetFloat("AttackSpeed", currentAttackSpeed);
    }

    public void AddStat(C_Enums.CharacterStat statType,bool isPercent, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHp)
        {
            Stat.AddMaxHp(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackDamage)
        {
            Stat.AddAttackDamage(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.MoveSpeed)
        {
            Stat.AddMoveSpeed(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.AddAttackSpeed(isPercent, value);
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
        else if (statType == C_Enums.CharacterStat.IdenBonus)
        {
            Stat.AddIdenBonus(value);
        }
        else if (statType == C_Enums.CharacterStat.CooldownReduction)
        {
            Stat.AddCooldownReduction(value);
        }
        else if (statType == C_Enums.CharacterStat.DamageTakeMultiplier)
        {
            Stat.AddTakeMultiplier(value);
        }
        else if (statType == C_Enums.CharacterStat.DodgeCooldownReduction)
        {
            Stat.AddDodgeCooldownReduction(value);
        }

        navMesh.speed = Stat.Stat.moveSpeed.FinalValue;
    }

    public void RemoveStat(C_Enums.CharacterStat statType, bool isPercent, float value)
    {
        if (statType == C_Enums.CharacterStat.MaxHp)
        {
            Stat.RemoveMaxHp(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackDamage)
        {
            Stat.RemoveAttackDamage(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.MoveSpeed)
        {
            Stat.RemoveMoveSpeed(isPercent, value);
        }
        else if (statType == C_Enums.CharacterStat.AttackSpeed)
        {
            Stat.RemoveAttackSpeed(isPercent, value);
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
        else if (statType == C_Enums.CharacterStat.IdenBonus)
        {
            Stat.RemoveIdenBonus(value);
        }
        else if (statType == C_Enums.CharacterStat.CooldownReduction)
        {
            Stat.RemoveCooldownReduction(value);
        }
        else if (statType == C_Enums.CharacterStat.DamageTakeMultiplier)
        {
            Stat.RemoveTakeMultiplier(value);
        }
        else if (statType == C_Enums.CharacterStat.DodgeCooldownReduction)
        {
            Stat.RemoveDodgeCooldownReduction(value);
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

        if (target.gameObject.TryGetComponent<IInteractable>(out var interactable))
        {
            if (interactable.isLocked) return;

            // 홀딩형이거나 자동 진행형인 경우 코루틴 실행
            if (interactable.isHoldInteraction || interactable.isAutoProgress)
            {
                StartInteraction(interactable);
            }
            else // 일반 클릭형
            {
                if (interactable.Interact(this.transform))
                    HandlePlayerReaction(interactable.interactType);
            }
        }
    }

    public void StartInteraction(IInteractable interactable)
    {
        // 이미 진행 중인게 있다면 취소하고 새로 시작
        CancelInteraction();
        _interactionRoutine = StartCoroutine(InteractionProcessRoutine(interactable));
    }

    private IEnumerator InteractionProcessRoutine(IInteractable interactable)
    {
        float timer = 0f;
        Vector3 startPos = transform.position;

        // 1. 게이지 UI 시작
        UIManager.Instance.mainUI.SetGaugeUI(true, interactable.interactName, 0f);

        playerController.StopMove();
        HandlePlayerReaction(interactable.interactType);

        while (timer < interactable.holdTime)
        {
            // --- 취소 조건 체크 ---

            // A. 홀딩형일 때만 키를 뗐는지 검사 (AutoProgress는 이 검사를 건너뜀)
            if (interactable.isHoldInteraction && !Input.GetKey(KeyCode.G))
            {
                Debug.Log("홀딩 중단으로 취소");
                break;
            }

            // --- 시간 업데이트 ---
            timer += Time.deltaTime;

            // 2. 게이지 실시간 업데이트
            float progress = Mathf.Clamp01(timer / interactable.holdTime); // 0~1 사이 값 고정
            UIManager.Instance.mainUI.SetGaugeUI(true, interactable.interactName, progress);

            yield return null;
        }

        // 3. 결과 처리
        if (timer >= interactable.holdTime)
        {
            // 성공 시 최종 게이지 100% 한 번 더 갱신 (시각적 안정감)
            UIManager.Instance.mainUI.SetGaugeUI(true, interactable.interactName, 1f);
            interactable.Interact(this.transform);
        }
        else
        {
            // 실패 시 처리
            interactable.OnInteractCancel();
            Anim.SetTrigger("Interact_Cancel");
        }

        // 약간의 딜레이 후 게이지 끄기 (로아처럼 완료 직후 바로 사라지면 심심하니까요)
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.mainUI.SetGaugeUI(false);

        _interactionRoutine = null;
    }

    public void CancelInteraction()
    {
        if (_interactionRoutine != null)
        {
            StopCoroutine(_interactionRoutine);
            _interactionRoutine = null;

            // UI 끄기 및 내부 상태 초기화
            UIManager.Instance.mainUI.SetGaugeUI(false);
            // 필요하다면 Anim.SetTrigger("Interact_Cancel"); 호출
            Debug.Log("상호작용이 다른 액션에 의해 취소되었습니다.");
        }
    }

    private void HandlePlayerReaction(InteractType type)
    {
        // NPC는 플레이어 애니메이션을 재생하지 않음
        if (type == InteractType.NPC)
        {
            Debug.Log("NPC와 대화를 시작합니다.");
            playerController.StopMove(); // 대화 중 이동만 정지
            return;
        }

        if (Navmesh.enabled)
        {
            playerController.StopMove();
        }

        // 상호작용 오브젝트 종류별 애니메이션 분기
        playerController.StopMove(); // 오브젝트 상호작용 시 이동 정지

        switch (type)
        {
            case InteractType.Lever:
                Anim.SetTrigger("Interact_Lever"); // 레버 당기는 모션
                Debug.Log("레버 작동 애니메이션 재생");
                break;

            case InteractType.Gathering:
                Anim.SetTrigger("Interact_Gather"); // 허리 숙여 줍는 모션
                Debug.Log("채집 애니메이션 재생");
                break;

            case InteractType.Portal:
                // 포탈은 보통 애니메이션 없이 이펙트나 씬 전환
                Debug.Log("포탈 진입");
                break;

            case InteractType.Jump:
                Anim.SetTrigger("Jump");
                break;
            default:
                break;
        }
    }

    public void EndJump()
    {
        Navmesh.enabled = true;

        Anim.SetTrigger("JumpEnd");
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
