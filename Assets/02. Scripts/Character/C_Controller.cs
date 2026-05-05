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
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                if (agent.remainingDistance < 0.5f)
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
        }
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
        if (_model.Buff.isStun) return;

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
        if (_model.Buff.isStun) return;

        _model.TryInteract();
    }

    public void RequestBasicAttack(bool isHeld, Vector3 dest)
    {
        if (_model.Buff.isStun) return;

        isAttackHeld = isHeld;

        // 🌟 핵심 방어 로직: 수라결 강제 종료 후 마우스를 뗄 때까지 입력 무시
        if (_model.isWaitingForRelease)
        {
            // 마침내 손을 뗐을 때 비로소 상태 완전 초기화
            if (!isAttackHeld)
            {
                _model.OnAttackEnd();
            }
            prevAttackHeld = isAttackHeld;
            return; // 🌟 아래에 있는 일반 공격 로직이 절대 실행되지 않게 막음
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
        _model.canMove = false;
        lastAttackTime = Time.time;

        currentCombo++;

        if (currentCombo > 4) currentCombo = 0;

        if (_model.Anim != null)
        {
            FaceTo(attackDir);

            _model.Anim.SetInteger("Combo", currentCombo);
            _model.Anim.SetTrigger("Attack");

            if (currentCombo < 3)
                AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack1);
            else if (currentCombo == 3)
                AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack2);
            else if (currentCombo == 4)
                AudioManager.instance.PlaySFX(C_Enums.SFX_List.Player_Attack4);

        }
    }

    private void CancelAttack()
    {
        if (_model.Buff.isStun) return;

        isAttacking = false;
        _model.canMove = false;
        nextAttackReady = false;
        currentCombo = 0;

        if (_model.Anim != null)
            _model.Anim.ResetTrigger("Attack");
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
        if (_model.Navmesh.enabled && _model.Navmesh.isOnNavMesh)
        {
            _model.Navmesh.isStopped = true;
            _model.Navmesh.ResetPath();
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
        Vector3 dir = target - tr.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        tr.rotation = Quaternion.LookRotation(dir);
    }
}
