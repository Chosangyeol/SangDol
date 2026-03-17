using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class C_Controller
{
    private readonly CharacterModel _model;

    private readonly Transform tr;
    private readonly NavMeshAgent agent;
    private readonly Animator anim;

    private bool isRotating;
    private Quaternion rotateTarget;
    private readonly float rotateSpeed = 360f;
    public C_Controller(CharacterModel model)
    {
        _model = model;
        tr = _model.transform;

        agent = _model.GetComponent<NavMeshAgent>();
        anim = _model.GetComponentInChildren<Animator>();

        agent.updateRotation = false;
        return;
    }

    public void Tick()
    {
        if (!isRotating) return;

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

    public void RequestMove(Vector3 dest)
    {
        RotateTo(dest);

        if (agent != null)
        {
            agent.isStopped = true;
            agent.isStopped = false;
            agent.velocity = Vector3.zero;

            agent.SetDestination(dest);
        }

        if (anim != null)
        {
            anim.SetBool("Move", true);
        }
    }

    public void RequestInteract()
    {
        _model.TryInteract();
    }

    public void RequestBasicAttack(Vector3 dest)
    {
        StopMove();

        FaceTo(dest);

        Collider[] targets = Physics.OverlapSphere(_model.transform.position, 4f);

        foreach (Collider target in targets)
        {
            if (target.TryGetComponent<EnemyModel>(out EnemyModel enemy))
            {
                Vector3 dir = (enemy.transform.position - _model.transform.position).normalized;

                dir.y = 0;
                Vector3 myForward = _model.transform.forward;
                myForward.y = 0;

                float angle = Vector3.Angle(myForward, dir);

                if (angle <= 90 / 2f)
                    enemy.Damaged(_model.GetCritical(_model.Stat.Stat.attackDamage.FinalValue), _model.gameObject);
                else
                    enemy.Damaged(_model.Stat.Stat.attackDamage.FinalValue, _model.gameObject);
            }
        }    
    }

    public void RequsetSkill(C_Enums.SkillSlot skillSlot, Vector3 dest)
    {
        StopMove();

        FaceTo(dest);

        Debug.Log("스킬 " + skillSlot + " 사용 시도");
        _model.SkillSystem.UseSkill(skillSlot, dest);
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
    
        if (anim != null)
        {
            anim.SetBool("Move", false);
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
