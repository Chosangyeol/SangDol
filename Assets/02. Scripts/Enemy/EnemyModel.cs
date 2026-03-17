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

public class EnemyModel : MonoBehaviour
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;
    [SerializeField] private LayerMask _playerLayer;

    [SerializeField] private Transform spawnPoint;
    public Transform SpawnPoint => spawnPoint;



    private bool _isDead = false;

    private CharacterModel _target;
    public CharacterModel Target => _target;

    private NavMeshAgent _agent;
    public NavMeshAgent Agent => _agent;

    private Animator _anim;
    public Animator Anim => _anim;

    private EnemyStat _stat;
    public EnemyStat Stat => _stat;

    private StateMachine stateMachine;
    private EState curState;

    private void Awake()
    {
        _stat = new EnemyStat(statSO);
        stateMachine = new StateMachine(this);

        curState = EState.Idle;
        stateMachine.ChangeState(new IdleState(this));
    }

    private void Start()
    {
        _target = FindAnyObjectByType<CharacterModel>();

        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _stat.moveSpeed;

        _anim = GetComponent<Animator>();

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

        Debug.Log($"canChase: {canChase}, canAttack: {canAttack}, curState: {curState}");

        switch (curState)
        {
            case EState.Idle:
                if (canAttack)
                {
                    curState = EState.Attack;
                    stateMachine.ChangeState(new AttackState(this));
                    return;
                }                
                else if (canChase && !canAttack)
                {
                    curState = EState.Chase;
                    stateMachine.ChangeState(new ChaseState(this));
                    return;
                }
                else if (!canChase && !canAttack)
                {
                    if (stateMachine.CurState is IdleState idle && idle.canPatrol)
                    {
                        curState = EState.Patrol;
                        stateMachine.ChangeState(new PatrolState(this));
                        return;
                    }
                }
                break;
            case EState.Patrol:
                if (canAttack && canChase)
                {
                    curState = EState.Attack;
                    stateMachine.ChangeState(new AttackState(this));
                    return;
                }
                else if (canChase && !canAttack)
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
                if (stateMachine.CurState is AttackState attack && attack.CanAttack)
                {
                    if (canChase)
                    {
                        curState = EState.Chase;
                        stateMachine.ChangeState(new ChaseState(this));
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

    public void Attack()
    {
        // 자식 클래스에서 공격 구현
        Debug.Log("공격 실행");
    }


    public void Damaged(float damage, GameObject source = null)
    {
        _stat.Damaged(damage);
        if (_stat.down <= 0)
        {
            Debug.Log("무력화");
        }

        if (_stat.curHp <= 0)
        {
            Die(source);
        }
    }

    public void Heal(float healAmount)
    {
        _stat.Heal(healAmount);
    }

    public void Die(GameObject source)
    {
        _isDead = true;

        curState = EState.Die;
        stateMachine.ChangeState(new DieState(this));

        if (source.TryGetComponent<CharacterModel>(out CharacterModel character))
        {
            character.Stat.GainExp(statSO.expAmount);
            character.Stat.GainGold(statSO.goldAmount);
            // 아이템 드랍 처리
        }
        // 적 사망 처리
        Debug.Log($"{_stat.enemyName}이(가) 사망했습니다.");
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
