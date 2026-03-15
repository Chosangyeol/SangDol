using UnityEngine;
using UnityEngine.AI;

public class PatrolState : State
{
    public bool isPatrolling = false;
    private Vector3 targetPoint;

    public PatrolState(EnemyModel owner) : base(owner)
    {

    }

    public override void EnterState()
    {
        if (SetRandomPatrolPoint(out targetPoint))
        {
            _owner.Agent.SetDestination(targetPoint);
            isPatrolling = true;
        }

        Debug.Log("순찰 상태로 진입");
    }

    public override void UpdateState()
    {
        if (isPatrolling)
        {
            if (_owner.Agent.remainingDistance <= _owner.Agent.stoppingDistance)
            {
                _owner.Agent.isStopped = true;
                _owner.Agent.velocity = Vector3.zero;
                isPatrolling = false;
            }
        }
        Debug.Log("순찰 상태 업데이트");
    }

    public override void ExitState()
    {
        _owner.Agent.isStopped = false;
        Debug.Log("순찰 상태에서 벗어남");
    }

    private bool SetRandomPatrolPoint(out Vector3 target)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 circle = Random.insideUnitCircle * 15f;
            Vector3 patrolPoint = _owner.SpawnPoint.position + new Vector3(circle.x, 0, circle.y);

            if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                target = hit.position;
                return true;
            }
        }

        target = _owner.transform.position;
        return false;
    }
}
