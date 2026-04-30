using System.Collections;
using UnityEngine;
using UnityEngine.AI;

enum EState
{
    Idle, // 대기 상태 - 아무것도 하지 않는 상태
    Patrol, // 순찰 상태 - 정해진 범위 내에서 돌아다니는 상태
    Chase, // 추적 상태 - 플레이어를 인식하여, 추적하는 상태
    Attack, // 공격 상태 - 플레이어가 공격 범위 안에 들어와서 공격하는 상태
    Return, // 귀환 상태 - 몬스터가 스폰 포인트로부터 너무 멀어져서 or 플레이어가 몬스터로부터 너무 멀어져서 스폰포인트로 귀환하는 상태
    Die
}

public class EnemyModel : EnemyBase
{
    [SerializeField] private Transform spawnPoint;
    public Transform SpawnPoint => spawnPoint;

    private NavMeshAgent _agent;
    public NavMeshAgent Agent => _agent;

    private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    private EState curState;

    public bool isAggressive = false;

    public bool canAttack;

    protected override void Awake()
    {
        base.Awake();

        stateMachine = new StateMachine(this);

        curState = EState.Idle;
        stateMachine.ChangeState(new IdleState(this));
    }

    protected override void Start()
    {
        base.Start();

        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _stat.moveSpeed;
    }

    private void Update()
    {
        if (_isDead)
        {
            stateMachine.UpdateState();
            return;
        }

        float dist = Vector3.Distance(transform.position, _target.transform.position);

        bool canChase = dist <= _stat.detactRange;
        bool canAttack = dist <= _stat.attackRange;
        bool canReturn = dist >= _stat.detactRange + 5f;

        switch (curState)
        {
            case EState.Idle:
                if (isAggressive)
                {
                    if (canAttack)
                    {
                        curState = EState.Attack;
                        stateMachine.ChangeState(new AttackState(this));
                        return;
                    }
                    else if (canChase)
                    {
                        curState = EState.Chase;
                        stateMachine.ChangeState(new ChaseState(this));
                        return;
                    }
                }
                if (stateMachine.CurState is IdleState idle && idle.canPatrol)
                {
                    curState = EState.Patrol;
                    stateMachine.ChangeState(new PatrolState(this));
                    return;
                }
                break;
            case EState.Patrol:
                if (canAttack && canChase && isAggressive)
                {
                    curState = EState.Attack;
                    stateMachine.ChangeState(new AttackState(this));
                    return;
                }
                else if (canChase && !canAttack && isAggressive)
                {
                    curState = EState.Chase;
                    stateMachine.ChangeState(new ChaseState(this));
                    return;
                }
                else if (!canChase && !canAttack)
                {
                    if (stateMachine.CurState is PatrolState patrol && !patrol.isPatrolling)
                    {
                        curState = EState.Idle;
                        stateMachine.ChangeState(new IdleState(this));
                        return;
                    }
                }
                break;
            case EState.Chase:
                if (canAttack)
                {
                    curState = EState.Attack;
                    stateMachine.ChangeState(new AttackState(this));
                }
                else if (canReturn)
                {
                    curState = EState.Return;
                    stateMachine.ChangeState(new ReturnState(this));
                }
                // 추적 상태에서의 행동
                break;
            case EState.Attack:
                if (stateMachine.CurState is AttackState attack && attack.changeState)
                {
                    if (canChase)
                    {
                        curState = EState.Chase;
                        stateMachine.ChangeState(new ChaseState(this));
                    }
                    else if (canAttack)
                    {
                        curState = EState.Attack;
                        stateMachine.ChangeState(new AttackState(this));
                    }
                    else if (canReturn)
                    {
                        curState = EState.Return;
                        stateMachine.ChangeState(new ReturnState(this));
                    }
                }
                // 공격 상태에서의 행동
                break;
            case EState.Return:
                // 스폰 포인트로 복귀 후 Idle 상태로 자동 전환
                if (stateMachine.CurState is ReturnState returnState && !returnState.isReturning)
                {
                    curState = EState.Idle;
                    stateMachine.ChangeState(new IdleState(this));
                }
                break;
        }


        stateMachine.UpdateState();
    }

    public override void Reset()
    {
        base.Reset();

        isAggressive = statSO.isAggressive;
        curState = EState.Idle;

        if (stateMachine != null)
        {
            stateMachine.ChangeState(new IdleState(this));
        }

        if (_agent != null)
        {
            _agent.speed = Stat.moveSpeed;
            _agent.enabled = true;
            _agent.isStopped = false;
            _agent.velocity = Vector3.zero;

            if (_agent.isOnNavMesh) _agent.ResetPath();
        }

        
    }

    public override void Damaged(SDamageInfo info)
    {
        base.Damaged(info);

        isAggressive = true;

        if (curState == EState.Idle || curState == EState.Patrol)
        {
            curState = EState.Chase;
            stateMachine.ChangeState(new ChaseState(this));
        }
    }

    public void SetSpawnPoint(Transform pos)
    {
        spawnPoint = pos;
    }

    public virtual void Attack()
    {
        Debug.Log("공격 실행");
    }

    public virtual void AttackEnd()
    {
        canAttack = true;
    }


    public void Heal(int healAmount)
    {
        if (_isDead) return;

        _stat.Heal(healAmount);
    }

    protected override void Die(GameObject source)
    {
        if (_isDead) return;

        _isDead = true;

        Anim.SetTrigger("Die");

        GameEvent.OnMonsterKill?.Invoke(statSO.enemyID);

        curState = EState.Die;
        stateMachine.ChangeState(new DieState(this));

        if (source.TryGetComponent<CharacterModel>(out CharacterModel character))
        {
            character.Stat.GainExp(statSO.expAmount);
            character.Stat.GainGold(statSO.goldAmount);

            if (statSO.dropTableSO == null) return;

            foreach(DropItem dropItem in statSO.dropTableSO.dropItems)
            {
                int dropChance = Random.Range(0, 100);

                if (dropChance <= dropItem.dropPercent)
                {
                    ItemBaseSO dropItemSO = ItemManager.Instance.GetItemBaseSO(dropItem.itemID);

                    PoolableMono item = PoolManager.Instance.Pop("DropItem");
                    
                    if (item.GetComponent<DropItemModel>() != null)
                    {
                        DropItemModel dropItemModel = item.GetComponent<DropItemModel>();
                        dropItemModel.InitItem(dropItemSO,dropItem.amount);
                        dropItemModel.transform.position = transform.position + Vector3.up * 0.5f;
                    }
                }
            }
        }
        
    }

    public void ReturnPool()
    {
        OnReturnToPool?.Invoke(this);
        PoolManager.Instance.Push(this);
    }

    private void OnDrawGizmos()
    {
        if (_stat == null) return;

        // 추적 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _stat.detactRange);

        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stat.attackRange);
    }
}
