using System.Collections;
using UnityEngine;
using UnityEngine.AI;

enum EState
{
    Idle, // 대기 상태 - 아무것도 하지 않는 상태
    Patrol, // 순찰 상태 - 정해진 범위 내에서 돌아다니는 상태
    Chase, // 추적 상태 - 플레이어를 인식하여, 추적하는 상태
    Attack, // 공격 상태 - 플레이어가 공격 범위 안에 들어와서 공격하는 상태
    Return // 귀환 상태 - 몬스터가 스폰 포인트로부터 너무 멀어져서 or 플레이어가 몬스터로부터 너무 멀어져서 스폰포인트로 귀환하는 상태
}

public class EnemyModel : MonoBehaviour
{
    [Header("적 기본 설정")]
    public EnemyStatSO statSO;


    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    private Animator anim;
    public Animator Anim => anim;

    private E_Stat stat;
    private E_Stat Stat => stat;

    private StateMachine stateMachine;
    private EState curState;

    private void Awake()
    {
        stat = new E_Stat(this, statSO);
        stateMachine = new StateMachine(this);

        curState = EState.Idle;
        stateMachine.ChangeState(new IdleState(this));
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

    }

    private void Update()
    {
        bool canChase = Physics.CheckSphere(transform.position, Stat.Stat.detactRange, LayerMask.GetMask("Player"));
        bool canAttack = Physics.CheckSphere(transform.position, Stat.Stat.attackRange, LayerMask.GetMask("Player"));

        switch (curState)
        {
            case EState.Idle:
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
                    if (stateMachine.CurState is IdleState idle && idle.canPatrol)
                    {
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
                break;
            case EState.Chase:
                // 추적 상태에서의 행동
                break;
            case EState.Attack:
                // 공격 상태에서의 행동
                break;
            case EState.Return:
                // 귀환 상태에서의 행동
                break;
        }


        stateMachine.UpdateState();
    }

    public void Damaged(float damage)
    {
        stat.Damaged(damage);
        if (stat.Stat.curHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        stat.Heal(healAmount);
    }

    public void Die()
    {
        // 적 사망 처리
        Debug.Log($"{stat.Stat.enemyName}이(가) 사망했습니다.");
    }
}
