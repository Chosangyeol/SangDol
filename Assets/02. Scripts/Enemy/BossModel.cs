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

public class BossModel : EnemyBase, ICounterable
{
    public Transform bossSpawnPoint;

    [Header("기믹 / 무력화 / 카운터 상태")]
    public bool isStatic = false;
    public bool isImmunity = false;
    public bool isDoingSpecial = false;
    public bool isKnockDown = false;
    public bool isCutsceneFinished = false;
    public bool isInField = false;

    public bool isCombatStarted = false;

    [Header("카운터 시각 연출")]
    public GameObject counterEffectPrefab;   // 카운터 타이밍에 띄울 파티클/이펙트 프리팹
    public Transform counterEffectSpawnPos;  // 이펙트가 생성될 위치 (가슴팍이나 머리 위 등. 비워두면 보스 발밑 기본 transform)
    private GameObject _currentCounterEffect;// 현재 생성되어 있는 이펙트 추적용

    private bool canCounter = false;
    public bool CanCounter => canCounter;
    public float couterDuration = 5f;

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
        GameEvent.OnPlayerDie += ResetBossState;

        if (isInField && bossSpawnPoint == null)
        {
            GameObject spawnAnchor = new GameObject($"{gameObject.name}_SpawnPoint");
            spawnAnchor.transform.position = transform.position;
            spawnAnchor.transform.rotation = transform.rotation;
            bossSpawnPoint = spawnAnchor.transform;
        }

        // 필드 보스가 아니라면(던전 보스) 만나자마자 바로 전투 시작
        if (!isInField)
        {
            isCombatStarted = true;
        }
    }

    private void Update()
    {
        if (isKnockDown) return;


        if (Target == null)
        {
            _target = FindAnyObjectByType<CharacterModel>();
        }

        if (isInField && !isCombatStarted) return;

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

        if (isImmunity) return;

        if (isInField && !isCombatStarted)
        {
            isCombatStarted = true;
            Debug.Log("필드 보스가 공격을 받고 전투를 시작합니다!");
        }

        base.Damaged(info);

        GameEvent.OnBossStateChange?.Invoke(this);
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

    protected override void Die(GameObject source = null)
    {
        if (_isDead) return;
        _isDead = true;

        GameEvent.OnBossStateChange(null);
        GameEvent.OnMonsterKill?.Invoke(statSO.enemyID);

        PoolManager.Instance.Push(this);
    }

    #endregion

    private void HandleCheckSpecial()
    {
        if (isDoingSpecial) return;

        if (currentPattern != null) return;

        float curHpPercent = (float)Stat.curHp / Stat.maxHp;

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

        if (counterEffectPrefab != null && _currentCounterEffect == null)
        {
            // 지정된 위치가 없으면 보스의 루트(기본 transform)를 사용
            Transform spawnTarget = counterEffectSpawnPos != null ? counterEffectSpawnPos : transform;

            // 보스를 부모로 삼아서 보스가 움직일 때 이펙트도 따라가게 만듦
            _currentCounterEffect = Instantiate(counterEffectPrefab, spawnTarget.position, spawnTarget.rotation, spawnTarget);
        }
    }

    public void DisableCounter()
    {
        canCounter = false;

        if (_currentCounterEffect != null)
        {
            Destroy(_currentCounterEffect);
            _currentCounterEffect = null;
        }
    }

    public void OnCounterSuccess(SDamageInfo info)
    {
        // 1. 몬스터가 카운터 가능한 상태가 아니거나, 공격이 카운터 속성이 아니면 즉시 취소!
        if (!canCounter || !info.isCounterable)
        {
            return;
        }

        // 2. 공격 방향이 헤드(1)가 아니면 즉시 취소!
        if (CheckAttackDir() != 1)
        {
            return;
        }

        DisableCounter();

        // --- 여기까지 무사히 넘어왔다면 진짜 카운터 성공 ---
        Debug.Log("카운터 성공");
        canCounter = false;
        ForceStopCurrentAction();
        StartCoroutine(KnockDown(4f, false));
    }

    public IEnumerator KnockDown(float duration,bool isReset)
    {
        isKnockDown = true;

        GameEvent.OnBossStateChange?.Invoke(this);

        Anim.SetTrigger("KnockDown");

        yield return new WaitForSeconds(duration);

        Anim.SetTrigger("StandUp");

        yield return new WaitForSeconds(4f);

        isKnockDown = false;

        if (isReset)
            _stat.curDown = _stat.maxDown;

        GameEvent.OnBossStateChange?.Invoke(this);
    }

    public void ForceStopCurrentAction()
    {
        // 1. 진행 중인 모든 코루틴 정지
        StopAllCoroutines();

        // 2. 상태 초기화
        currentPattern = null;
        canCounter = false;
        isDoingSpecial = false; // 특수 패턴 중 죽었을 때를 대비해 초기화
        isKnockDown = false;
        isStatic = false;
        isImmunity = false;     // 무적 상태 강제 해제

        if (_currentCounterEffect != null)
        {
            Destroy(_currentCounterEffect);
            _currentCounterEffect = null;
        }

        // 3. 스폰된 패턴(장판, 투사체 등) 파괴
        foreach (GameObject obj in patternObjects)
        {
            if (obj != null) Destroy(obj);
        }
        patternObjects.Clear();

        // 4. 네비메시 에이전트 상태 강제 초기화
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    public void SetImmunity(bool immunity)
    {
        isImmunity = immunity;

        GameEvent.OnBossStateChange?.Invoke(this);
    }

    public virtual void ResetBossState()
    {
        Debug.Log("플레이어 사망. 대청소 시작");

        // 1. 모든 행동과 찌꺼기 싹 정리
        ForceStopCurrentAction();

        // 2. 특수 패턴(기믹) 발동 여부 리셋
        for (int i = 0; i < specialPatterns.Count; i++)
        {
            specialPatterns[i].hasDone = false;
        }

        // 3. 체력 초기화 (Stat 시스템에 맞게 호출, 보통은 최대 체력으로 복구)
        _stat.curHp = _stat.maxHp;
        _stat.curDown = _stat.maxDown;

        GameEvent.OnBossStateChange?.Invoke(this);

        // 보스는 더 이상 기다리지 않고 즉시 풀로 반환되도록 변경
        if (isInField)
        {
            isCombatStarted = false;

            // 스폰 위치로 즉시 복귀 (걸어가지 않고 워프)
            if (bossSpawnPoint != null && _agent != null && _agent.isOnNavMesh)
            {
                _agent.Warp(bossSpawnPoint.position);
                transform.rotation = bossSpawnPoint.rotation;
            }

            Debug.Log("필드 보스가 초기 위치로 돌아가 다시 대기합니다.");
        }
        else
        {
            PoolManager.Instance.Push(this);

        }
    }

    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolManager.Instance.Push(this);
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
