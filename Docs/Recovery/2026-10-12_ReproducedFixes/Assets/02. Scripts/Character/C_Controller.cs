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
    private Vector3 lastMoveDestination;
    private bool hasMoveDestination;
    private float nextRepathTime;
    private const float RepathInterval = 0.08f;
    private const float DestinationThreshold = 0.1f;
    private Vector3 bufferedMoveDestination;
    private bool hasBufferedMoveDestination;


    [Header("공격")]
    public int currentCombo = 0;
    public float lastAttackTime = 0f;
    public float comboResetTime = 2.5f;
    public bool isAttacking = false;
    public bool nextAttackReady = false;
    public Vector3 attackDir;
    public bool isAttackHeld = false;

    private bool prevAttackHeld = false;

    public C_Controller(CharacterModel model)
    {
        _model = model;
        tr = _model.transform;

        agent = _model.Navmesh;

        agent.updateRotation = false;
        return;
    }

    public void Tick()
    {
        if (hasBufferedMoveDestination)
        {
            if (_model.isDie || _model.Buff.isStun)
            {
                hasBufferedMoveDestination = false;
            }
            else if (!isAttacking && _model.canMove)
            {
                Vector3 destination = bufferedMoveDestination;
                hasBufferedMoveDestination = false;
                RequestMove(destination);
            }
        }

        if (CanNavigate && !agent.isStopped && _model.canMove && !isAttacking)
        {
            Vector3 direction = agent.desiredVelocity;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                RotateTo(tr.position + direction);
        }
        // 1. 회전 로직 (기존 코드 유지)
        if (isRotating)
        {
            tr.rotation = Quaternion.RotateTowards(
                tr.rotation,
                rotateTarget,
                rotateSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(tr.rotation, rotateTarget) < 0.01f)
            {
                isRotating = false;
            }
        }

        bool destinationReached = agent.hasPath
            && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.1f)
            && agent.velocity.sqrMagnitude < 0.01f;
        bool pathFailed = !agent.hasPath && Time.time >= nextRepathTime;
        if (CanNavigate && !agent.isStopped && !agent.pathPending && (pathFailed || destinationReached))
            StopMove();
    }

    public void TeleportTo(Vector3 dest)
    {
        StopMove();

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.Warp(dest);
        }
        else
        {
            tr.position = dest;
        }
    }

    public void RequestMove(Vector3 dest)
    {
        if (_model.isDie || _model.Buff.isStun) return;

        if (isAttacking && !_model.canMove)
        {
            bufferedMoveDestination = dest;
            hasBufferedMoveDestination = true;
            return;
        }

        if (!CanNavigate) return;

        if (isAttacking)
        {
            if (_model.canMove)
                CancelAttack();
            else
                return;
        }
        else
        {
            if (!_model.canMove)
                return;
        }

        bool restarting = agent.isStopped || !hasMoveDestination;
        if (!restarting && ((dest - lastMoveDestination).sqrMagnitude <
            DestinationThreshold * DestinationThreshold || Time.time < nextRepathTime))
            return;

        if (!NavMesh.SamplePosition(dest, out NavMeshHit hit, 1f, agent.areaMask)) return;
        if ((hit.position - tr.position).sqrMagnitude <=
            Mathf.Pow(Mathf.Max(agent.stoppingDistance, 0.1f), 2f))
        {
            StopMove();
            return;
        }
        if (!agent.SetDestination(hit.position)) return;

        agent.isStopped = false;
        currentCombo = 0;
        lastMoveDestination = dest;
        hasMoveDestination = true;
        nextRepathTime = Time.time + RepathInterval;
        if (_model.Anim != null) _model.Anim.SetBool("Move", true);
    }

    private bool CanNavigate => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;

    public void RequestInteract()
    {
        if (_model.Buff.isStun) return;

        if (DialogManager.Instance != null && DialogManager.Instance.IsDialogueActive())
        {
            DialogManager.Instance.TryClickNextButton();
            return;
        }

        if (!_model.isInteracting)
            _model.TryInteract();
    }

    public void RequestBasicAttack(bool isHeld, Vector3 dest)
    {
        if (_model.Buff.isStun) return;

        isAttackHeld = isHeld;

        if (_model.isWaitingForRelease)
        {
            if (!isAttackHeld)
            {
                _model.OnAttackEnd();
            }
            prevAttackHeld = isAttackHeld;
            return;
        }

        // 1. 마우스를 꾹 누르고 있는 상태 (또는 방금 누른 순간)
        if (isAttackHeld)
        {
            attackDir = dest;

            if (_model.isIdenOn)
            {
                if (!isAttacking)
                {
                    StopMove();
                    FaceTo(dest);
                    _model.OnComboStart();
                }
                else
                {
                    FaceTo(dest);
                }
            }
            else
            {
                if (!isAttacking)
                {
                    StopMove();
                    FaceTo(dest);
                    StartAttackCombo();
                }
            }
        }
        // 2. 마우스를 방금 뗐을 때
        else if (prevAttackHeld && !isAttackHeld)
        {
            // 상태 상관없이 무조건 OnAttackEnd를 호출해 깔끔하게 끊어줍니다.
            _model.OnAttackEnd();
        }

        prevAttackHeld = isAttackHeld;
    }

    public void StartAttackCombo()
    {
        if (_model.Buff.isStun) return;

        isAttacking = true;
        nextAttackReady = false;
        lastAttackTime = Time.time;

        currentCombo++;

        if (currentCombo > 4) currentCombo = 0;

        if (_model.Anim != null)
        {
            FaceTo(attackDir);

            _model.Anim.SetInteger("Combo", currentCombo);
            _model.Anim.SetTrigger("Attack");
        }
    }

    private void CancelAttack()
    {
        if (_model.Buff.isStun) return;

        isAttacking = false;
        nextAttackReady = false;
        currentCombo = 0;

        if (_model.Anim != null)
            _model.Anim.ResetTrigger("Attack");
    }

    internal void InterruptAttackForSkill()
    {
        isAttacking = false;
        nextAttackReady = false;
        currentCombo = 0;
        if (_model.Anim != null) _model.Anim.ResetTrigger("Attack");
    }

    public void RequestSkillKeyDown(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        // C_SkillSystem의 기존 스킬 사용 로직 (차징 시작 또는 즉발)
        _model.SkillSystem.UseSkill(slot, targetPos);
    }

    // 뗄 때
    public void RequestSkillKeyUp(C_Enums.SkillSlot slot, Vector3 targetPos)
    {
        // C_SkillSystem에 새로 만든 손 뗌 로직 (차징 종료 및 타격 발동)
        _model.SkillSystem.ReleaseSkill(slot, targetPos); // ※ C_SkillSystem에 이 함수를 추가해야 합니다!
    }

    public void RequestUseItem(C_Enums.UseSlot useSlot)
    {
        if (_model.Buff.isStun) return;

        Debug.Log("아이템 " + useSlot + " 사용 시도");
        _model.Inventory.UseItem(useSlot);
    }

    public void RequestUI(C_Enums.UIList ui)
    {
        StopMove();

        UIManager.Instance.ToggleUI(ui);
    }

    public void StopMove()
    {
        isRotating = false;
        hasMoveDestination = false;
        hasBufferedMoveDestination = false;
        if (CanNavigate)
        {
            _model.Navmesh.ResetPath();
            _model.Navmesh.isStopped = true;
        }

        if (_model.Anim != null)
        {
            _model.Anim.SetBool("Move", false);
        }
    }

    public void RotateTo(Vector3 target)
    {
        Vector3 dir = target - tr.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        rotateTarget = Quaternion.LookRotation(dir);
        isRotating = true;
    }

    public void FaceTo(Vector3 target)
    {
        isRotating = false;
        Vector3 dir = target - tr.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        tr.rotation = Quaternion.LookRotation(dir);
    }
}
