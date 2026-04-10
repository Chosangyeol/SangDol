using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public List<GameObject> patternObjects = new List<GameObject>();


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

    #region 피격 & 백헤드
    public override void Damaged(SDamageInfo info)
    {
        if (_isDead) return;

        if (canCounter && info.isCounterable)
        {
            if (CheckAttackDir() == 1)
            {
                OnCounterSuccess(5f);
            }
        }

        base.Damaged(info);
    }

    public int CheckAttackDir()
    {
        Vector3 dir = (Target.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dir);

        if (dot > 0.6f)
        {
            Debug.Log("헤드");
            return 1;
        }
        else if (dot < -0.6f)
        {
            Debug.Log("백");
            return 2;
        }

        return 0;
    }

    #endregion

    private void HandleCheckSpecial()
    {
        if (isDoingSpecial) return;

        float curHpPercent = Stat.curHp / Stat.maxHp;

        foreach (var pattern in specialPatterns)
        {
            if (!pattern.hasDone && curHpPercent <= pattern.hpPercent)
            {
                pattern.hasDone = true;
                isDoingSpecial = true;
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

    public void OnCounterSuccess(float duration)
    {
        Debug.Log("카운터 성공");
        canCounter = false;

        ForceStopCurrentAction();

        StartCoroutine(Counter(duration));
    }

    public IEnumerator Counter(float duration)
    {
        isKnockDown = true;

        yield return new WaitForSeconds(duration);

        isKnockDown = false;
    }

    public void ForceStopCurrentAction()
    {
        StopAllCoroutines();

        currentPattern = null;
        canCounter = false;

        foreach (GameObject obj in patternObjects)
        {
            if (obj != null) Destroy(obj);
        }

        patternObjects.Clear();

        Anim.Play("Idle");
        Anim.SetBool("Move", false);

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 1. 보스의 현재 위치와 전방 벡터 확보
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;

        // --- 헤드 (전방) 영역 시각화 ---
        // 임계값 0.85를 각도로 변환
        float headAngle = Mathf.Acos(0.6f) * Mathf.Rad2Deg;

        // 초록색 계열의 반투명한 부채꼴 그리기
        Handles.color = new Color(0f, 1f, 0f, 0.5f);
        // 매개변수: 시작위치, 회전축, 그리기시작방향, 총각도, 반지름
        Handles.DrawSolidArc(
            pos,
            up,
            Quaternion.AngleAxis(-headAngle, up) * forward, // 좌측 경계선
            headAngle * 2f, // 총 각도
            3f
        );

        // --- 백 (후방) 영역 시각화 ---
        // 임계값 -0.85를 전방 기준 각도로 변환
        float backBoundaryAngle = Mathf.Acos(-0.6f) * Mathf.Rad2Deg;
        // 정후면 기준 실제 부채꼴 반반 각도
        float backHalfAngle = 180f - backBoundaryAngle;

        // 빨간색 계열의 반투명한 부채꼴 그리기
        Handles.color = new Color(1f, 0f, 0f, 0.5f);
        Handles.DrawSolidArc(
            pos,
            up,
            Quaternion.AngleAxis(180f - backHalfAngle, up) * forward, // 좌측 경계선
            backHalfAngle * 2f, // 총 각도
            3f
        );

        // (기본) 보스 전방 벡터 와이어 기즈모 추가
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(pos, forward * 3f * 1.2f);
    }
#endif
}
