using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class BossSpecialPattern
{
    public string patternName;
    [Range(0, 1)]
    public float hpPercent;
    public bool hasDone;
}

public class BossModel : EnemyBase
{
    [Header("기믹 / 무력화 / 카운터 상태")]
    public bool isStatic = false;
    public bool isDoingSpecial = false;
    public bool isKnockDown = false;
    public bool canCounter = false;

    [Header("특수 기믹 체력확인")]
    public List<BossSpecialPattern> specialPatterns;

    protected List<BossPatternBase> normalPatterns = new List<BossPatternBase>();
    protected BossPatternBase currentPattern = null;

    private NavMeshAgent _agent;
    public NavMeshAgent Agent => _agent;

    protected override void Start()
    {
        base.Start();
        _agent = GetComponent<NavMeshAgent>();

    }

    private void Update()
    {
        if (isKnockDown) return;

        HandleCheckSpecial();

        if (!isDoingSpecial && currentPattern == null)
        {
            bool hasPatternToUse = SelectNextNormalPattern();

            if (hasPatternToUse)
            {
                StopChase();
            }
            else
            {
                Chase();
            }
        }
    }

    private void HandleCheckSpecial()
    {
        if (isDoingSpecial) return;

        float curHpPercent = Stat.curHp / Stat.maxHp;

        foreach (var pattern in specialPatterns)
        {
            if (!pattern.hasDone && curHpPercent <= pattern.hpPercent)
            {
                pattern.hasDone = true;
                StartSpecialPattern(pattern);
                break;
            }
        }
    }

    protected virtual void StartSpecialPattern(BossSpecialPattern pattern)
    {

    }

    private bool SelectNextNormalPattern()
    {
        List<BossPatternBase> activePatterns = new List<BossPatternBase>();
        float totalWeight = 0f;

        // 사거리에 있고, 쿨타임이 지난 패턴 종합
        foreach (var pattern in normalPatterns)
        {
            if (pattern.IsReady(this,Target.transform))
            {
                activePatterns.Add(pattern);
                totalWeight += pattern.weight;
            }
        }

        if (activePatterns.Count == 0) return false;

        float random = Random.Range(0, totalWeight);
        float currentWeight = 0f;

        foreach(var pattern in activePatterns)
        {
            currentWeight += pattern.weight;

            if (random <= currentWeight)
            {
                currentPattern = pattern;
                currentPattern.Execute(this);
                return true;
            }
        }
        return false;
    }

    private void Chase()
    {
        if (Agent == null || !Agent.isOnNavMesh) return;

        if (Agent.isStopped)
        {
            Agent.isStopped = false;
            Anim.SetBool("Move", true);
        }

        Agent.SetDestination(Target.transform.position);
    }

    private void StopChase()
    {
        if (Agent == null || !Agent.isOnNavMesh) return;

        if (!Agent.isStopped)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero; // 얼음판처럼 미끄러지는 현상 방지
            Anim.SetBool("Move", false);   // 걷는 애니메이션 끄기
        }
    }

    public void OnPatternEnd()
    {
        currentPattern = null;
    }

    public void EnableCounter()
    {
        canCounter = true;

    }

    public void DisableCounter()
    {
        canCounter = false;
    }

    public void OnCounterSuccess()
    {
        canCounter = false;

        ForceStopCurrentAction();

        StartCoroutine(Counter());
    }

    IEnumerator Counter()
    {
        isKnockDown = true;

        yield return new WaitForSeconds(5f);

        isKnockDown = false;
    }

    public void ForceStopCurrentAction()
    {
        StopAllCoroutines();

        currentPattern = null;
        canCounter = false;
        Anim.Play("Idle");
        Anim.SetBool("Move", false);

    }
}
