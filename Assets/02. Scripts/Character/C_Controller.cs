using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class C_Controller
{
    private readonly CharacterModel _model;

    private readonly Transform tr;
    private readonly NavMeshAgent agent;

    private bool isRotating;
    private Quaternion rotateTarget;
    private readonly float rotateSpeed = 360f;


    [Header("공격")]
    public int currentCombo = 0;
    public float lastAttackTime = 0f;
    public float comboResetTime = 1f;
    public bool isAttacking = false;
    public bool nextAttackReady = false;
    public bool canMove = false;
    public Vector3 attackDir;

    public C_Controller(CharacterModel model)
    {
        _model = model;
        tr = _model.transform;

        agent = _model.GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        return;
    }

    public void Tick()
    {
        // 1. 회전 로직 (기존 코드 유지)
        if (isRotating)
        {
            tr.rotation = Quaternion.RotateTowards(
                tr.rotation,
                rotateTarget,
                rotateSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(tr.rotation, rotateTarget) < 0.5f)
            {
                tr.rotation = rotateTarget;
                isRotating = false;
            }
        }

        // 2. NavMeshAgent 도착 여부 체크 로직 추가
        if (agent != null && !agent.pathPending) // 경로 계산이 끝났고
        {
            if (agent.remainingDistance <= agent.stoppingDistance) // 목적지에 도달했거나 멈출 거리에 진입했다면
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) // 경로가 없거나 속도가 0이라면 (완전히 멈춤)
                {
                    // 이동 애니메이션이 켜져 있을 때만 꺼주기 (매 프레임 불필요한 호출 방지)
                    if (_model.Anim.GetBool("Move"))
                    {
                        StopMove();
                    }
                }
            }
        }
    }

    public void RequestMove(Vector3 dest)
    {
        if (isAttacking)
        {
            if (canMove)
                CancelAttack();
            else
                return;
        }

        FaceTo(dest);

        currentCombo = 0;

        if (agent != null)
        {
            agent.SetDestination(dest);
        }

        if (_model.Anim != null)
        {
            _model.Anim.SetBool("Move", true);
        }
    }

    public void RequestInteract()
    {
        _model.TryInteract();
    }

    public void RequestBasicAttack(Vector3 dest)
    {
        StopMove();
        attackDir = dest;

        if (Time.time - lastAttackTime > comboResetTime)
            currentCombo = 0;

        if (!isAttacking)
            StartAttackCombo();
        else
            nextAttackReady = true;
       
    }

    public void StartAttackCombo()
    {
        isAttacking = true;
        nextAttackReady = false;
        canMove = false;
        lastAttackTime = Time.time;

        currentCombo++;

        if (currentCombo > 4) currentCombo = 1;

        if (_model.Anim != null)
        {
            FaceTo(attackDir);

            _model.Anim.SetInteger("Combo", currentCombo);
            _model.Anim.SetTrigger("Attack");
        }
    }

    private void CancelAttack()
    {
        isAttacking = false;
        canMove = false;
        nextAttackReady = false;
        currentCombo = 0;

        if (_model.Anim != null)
            _model.Anim.ResetTrigger("Attack");
    }

    public void RequsetSkill(C_Enums.SkillSlot skillSlot, Vector3 dest)
    {
        StopMove();

        FaceTo(dest);

        Debug.Log("스킬 " + skillSlot + " 사용 시도");
        _model.SkillSystem.UseSkill(skillSlot, dest);
    }

    public void RequestUseItem(C_Enums.UseSlot useSlot)
    {
        Debug.Log("아이템 " + useSlot + " 사용 시도");
        _model.Inventory.UseItem(useSlot);
    }

    public void RequestUI(C_Enums.UIList ui)
    {
        StopMove();

        UIManager.Instance.ToggleUI(ui);
    }

    private void StopMove()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    
        if (_model.Anim != null)
        {
            _model.Anim.SetBool("Move", false);
        }
    }

    private void RotateTo(Vector3 target)
    {
        Vector3 dir = target - tr.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        rotateTarget = Quaternion.LookRotation(dir);
        isRotating = true;
    }

    private void FaceTo(Vector3 target)
    {
        Vector3 dir = target - tr.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        tr.rotation = Quaternion.LookRotation(dir);
    }
}
