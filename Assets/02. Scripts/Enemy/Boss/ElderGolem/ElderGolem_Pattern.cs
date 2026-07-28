using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class ElderGolem_Pattern : MonoBehaviour
{
    
}

public class ElderGolem_Pattern1 : BossPatternBase
{
    public GameObject stoneSpear;
    public GameObject warning1;
    public GameObject warning2;
    public GameObject effect;

    public float handOffsetX = 8f;     // 보스 중앙 기준 양손의 좌우 거리
    public float handOffsetZ = 5.0f;     // 보스 기준 손이 떨어지는 앞쪽 거리
    public float smashRadius = 10f;     // 손 하나당 피격 반경
    public float smashFillTime = 1.5f;   // 1타 경고판 차오르는 시간

    [Header("2타: 십자 바위 송곳 설정")]
    public float spearDistance = 10f;   // 찍은 위치에서 동서남북으로 떨어질 거리
    public float spearRadius = 10f;     // 송곳 1개당 피격 반경 (넓게 덮어서 중앙만 살게 유도)
    public float spearExplodeTime = 3f;

    public ElderGolem_Pattern1(GameObject warning1, GameObject warning2, GameObject stoneSpear, GameObject effect)
    {
        patternName = "Normal1";
        cooldown = 10f;
        weight = 30f;
        range = 5f;

        this.warning1 = warning1;
        this.warning2 = warning2;
        this.stoneSpear = stoneSpear;
        this.effect = effect;
    }

    public override void Execute(BossModel boss)
    {
        lastUsedTime = Time.time;
        boss.StartCoroutine(PatternRoutine(boss));
    }

    private IEnumerator PatternRoutine(BossModel boss)
    {
        boss.Anim.SetTrigger("Pattern1");

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.Elder_N1);

        boss.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Vector3 groundPos = boss.transform.position;
        groundPos.y += 0.1f;

        Vector3 center = groundPos + boss.transform.forward * handOffsetZ;
        Vector3 leftHandPos = center - boss.transform.right * handOffsetX;
        Vector3 rightHandPos = center + boss.transform.right * handOffsetX;

        Vector3 leftWarningPos = leftHandPos + new Vector3(0, 0.4f, 0);
        Vector3 rightWarningPos = rightHandPos + new Vector3(0, 0.4f, 0);

        GameObject leftWarn = GameObject.Instantiate(warning1, leftWarningPos, Quaternion.identity);
        GameObject rightWarn = GameObject.Instantiate(warning1, rightWarningPos, Quaternion.identity);

        boss.patternObjects.Add(leftWarn);
        boss.patternObjects.Add(rightWarn);

        Transform leftInner = leftWarn.transform.Find("InnerDecal"); // 프리팹 자식 이름과 똑같이 맞춰주세요!
        Transform rightInner = rightWarn.transform.Find("InnerDecal");

        if (leftInner != null) boss.StartCoroutine(FillWarning(leftInner, smashFillTime));
        if (rightInner != null) boss.StartCoroutine(FillWarning(rightInner, smashFillTime));

        yield return new WaitForSeconds(smashFillTime + 0.5f);

        boss.patternObjects.Remove(leftWarn); boss.patternObjects.Remove(rightWarn);
        GameObject.Destroy(leftWarn); GameObject.Destroy(rightWarn);

        if (leftWarn != null) GameObject.Destroy(leftWarn);
        if (rightWarn != null) GameObject.Destroy(rightWarn);

        yield return new WaitForSeconds(0.2f);

        GameObject effect1 = GameObject.Instantiate(effect, leftWarningPos, Quaternion.identity);
        boss.patternObjects.Add(effect1);
        boss.StartCoroutine(DestroyEffect(effect1,1f));
        GameObject effect2 = GameObject.Instantiate(effect, rightWarningPos,Quaternion.identity);
        boss.patternObjects.Add(effect2);
        boss.StartCoroutine(DestroyEffect(effect2, 1f));

        Vector3 playerPos = boss.Target.transform.position;
        
        if (Vector3.Distance(leftHandPos, playerPos) <= smashRadius &&
            Vector3.Distance(rightHandPos, playerPos) <= smashRadius)
        {
            boss.Target.Damaged(1.6f, true);
        }
        else if (Vector3.Distance(leftHandPos, playerPos) <= smashRadius ||
            Vector3.Distance(rightHandPos, playerPos) <= smashRadius)
        {
            boss.Target.Damaged(0.8f, true);
        }

        if (boss.Target.isDie) yield break;

        List<Vector3> spearPositions = new List<Vector3>(); // 데미지 판정용 위치 모음
        List<GameObject> spears = new List<GameObject>();   // 애니메이션(상하 이동)용 오브젝트 모음

        SpawnStoneSpears(boss, leftHandPos, true, false, spearPositions, spears);
        SpawnStoneSpears(boss, rightHandPos, false, false, spearPositions, spears);

        float upTime = 0.2f;
        float t = 0f;
        while (t < upTime)
        {
            t += Time.deltaTime;
            foreach (var s in spears)
                if (s != null) s.transform.position += Vector3.up * (6f / upTime) * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(spearExplodeTime);



        CheckSpearDamage(boss, spearPositions, center);

        if (boss.Target.isDie) yield break;

        float downTime = 2f;
        t = 0f;
        while (t < downTime)
        {
            t += Time.deltaTime;
            foreach (var s in spears)
                if (s != null) s.transform.position += Vector3.down * (8f / downTime) * Time.deltaTime;
            yield return null;
        }

        foreach (var s in spears) { boss.patternObjects.Remove(s); GameObject.Destroy(s); }

        yield return new WaitForSeconds(3f);

        boss.patternObjects.Clear();
        boss.OnPatternEnd();
    }

    private void SpawnStoneSpears(BossModel boss, Vector3 pivotPos, bool isLeft, bool isCenter, List<Vector3> posList, List<GameObject> spearList)
    {
        List<Vector3> directions = new List<Vector3>();

        if (isCenter)
        {
            directions.Add(Vector3.zero); // 중앙일 때는 방향 없이 제자리
        }
        else
        {
            // 보스가 바라보는 방향 기준으로 앞, 뒤 설정
            directions.Add(boss.transform.forward);
            directions.Add(-boss.transform.forward);

            // 왼손이면 왼쪽, 오른손이면 오른쪽 추가
            if (isLeft) directions.Add(-boss.transform.right);
            else directions.Add(boss.transform.right);
        }

        foreach (var dir in directions)
        {
            Vector3 spawnPos = pivotPos + dir * (isCenter ? 0 : spearDistance);
            posList.Add(spawnPos); // 데미지 판정 리스트에 등록

            // 1. 경고판 생성 및 채우기 코루틴 실행
            GameObject warning = GameObject.Instantiate(warning2, spawnPos + new Vector3(0, 0.4f, 0), Quaternion.identity);
            boss.patternObjects.Add(warning);

            Transform spearWarning = warning.transform.Find("InnerDecal");
            if (spearWarning != null)
                boss.StartCoroutine(FillWarning(spearWarning, spearExplodeTime));

            // 경고판은 터질 때 알아서 지워지도록 삭제 예약 코루틴 호출
            boss.StartCoroutine(DestroyAfter(boss, warning, spearExplodeTime));

            // 2. 바위 송곳 생성 (처음엔 땅 아래 -6 위치)
            Vector3 startPos = spawnPos + Vector3.down * 6f;
            GameObject spear = GameObject.Instantiate(stoneSpear, startPos, Quaternion.identity);
            boss.patternObjects.Add(spear);
            spearList.Add(spear); // 상승/하강 연출을 위해 리스트에 등록
        }
    }

    private IEnumerator DestroyAfter(BossModel boss, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        boss.patternObjects.Remove(obj);
        GameObject.Destroy(obj);
    }

    private IEnumerator FillWarning(Transform innerDecal, float fillTime)
    {
        DecalProjector projector = innerDecal.GetComponent<DecalProjector>();

        Vector3 targetSize = projector.size;

        Vector3 startSize = new Vector3(0f, 0f, targetSize.z);
        projector.size = startSize;

        float t = 0f;
        while (t < fillTime)
        {
            t += Time.deltaTime;
            float progress = t / fillTime;
            projector.size = Vector3.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        projector.size = targetSize;
    }

    private IEnumerator DestroyEffect(GameObject effect, float remainTime)
    {
        yield return new WaitForSeconds(remainTime);
        GameObject.Destroy(effect);
    }

    private void CheckSpearDamage(BossModel boss, List<Vector3> spearPositions, Vector3 pushCenter)
    {
        if (boss.Target == null || boss.Target.isDie) return;

        Vector3 playerPos = boss.Target.transform.position;
        bool hitBySpear = false;

        foreach (Vector3 pos in spearPositions)
        {
            if (Vector3.Distance(pos, playerPos) <= spearRadius)
            {
                hitBySpear = true;
                break;
            }
        }

        foreach(Vector3 pos in spearPositions)
        {
            GameObject expEffect = GameObject.Instantiate(effect, pos, Quaternion.identity);
            boss.patternObjects.Add(expEffect);
            boss.StartCoroutine(DestroyEffect(expEffect, 1f));
        }

        if (hitBySpear)
        {
            boss.Target.Damaged(0.3f, true); // 30% 피해

        }
    }
}

public class ElderGolem_Pattern2 : BossPatternBase
{
    [Header("패턴 프리팹 세팅")]
    public GameObject rockPrefab;          // 생성될 바위 프리팹
    public GameObject rockExplodeEffect;   // 바위가 바닥에 부딪히거나 플레이어에 맞았을 때 터질 이펙트

    [Header("원형 배치 세팅")]
    public float circleRadius = 30f;       // 바위가 생성될 원형 테두리 반지름
    public float rockScale = 5f;           // 바위 크기

    [Header("발사 속도 및 타이밍 세팅")]
    public float spawnInterval = 0.2f;     // 바위가 하나씩 스폰되는 간격 (동시 생성이면 0)
    public float readyTime = 1.5f;         // 8개 배치 완료 후 발사 전까지 대기 시간
    public float launchInterval = 1f;    // 바위가 한 발씩 날아가는 시간 간격 (엇박 연출용)
    public float rockSpeed = 30f;          // 바위가 날아가는 속도
    public float rockDamagePercent = 0.4f; // 바위 피격 시 체력 40% 피해
    public float rockLifetime = 3.0f;

    public ElderGolem_Pattern2(GameObject rockPrefab)
    {
        patternName = "Normal2";
        cooldown = 40f;
        weight = 25f;
        range = 35f;

        this.rockPrefab = rockPrefab;
    }

    public override void Execute(BossModel boss)
    {
        lastUsedTime = Time.time;
        boss.StartCoroutine(PatternRoutine(boss));
    }

    private IEnumerator PatternRoutine(BossModel boss)
    {

        boss.Agent.enabled = false;
        boss.Anim.SetBool("Pattern2", true);

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.Elder_N2);

        Vector3 centerPos = boss.bossSpawnPoint.position; // 맵 중앙 좌표
        centerPos.y = boss.transform.position.y;
        boss.transform.position = centerPos;

        List<GameObject> rocks = new List<GameObject>();
        
        for (int i = 0; i<8;i++)
        {
            float angle = i * 45f;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * circleRadius;
            Vector3 spawnPos = centerPos + offset;
            spawnPos.y = centerPos.y + 0.5f;

            GameObject rock = GameObject.Instantiate(rockPrefab, spawnPos, Quaternion.identity);
            rock.transform.localScale = Vector3.zero;

            boss.patternObjects.Add(rock);
            rocks.Add(rock);

            boss.StartCoroutine(ScaleUpRock(rock, rockScale, 0.5f));
            
            if (spawnInterval > 0f) yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(readyTime);

        List<int> launchOrder = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

        for (int i = launchOrder.Count -1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            int temp = launchOrder[i];
            launchOrder[i] = launchOrder[rnd];
            launchOrder[rnd] = temp;
        }

        foreach (int index in launchOrder)
        {
            GameObject rock = rocks[index];
            
            if (rock == null || boss.Target == null || boss.Target.isDie) continue;

            boss.StartCoroutine(FlyRockRoutine(boss, rock, boss.Target));

            yield return new WaitForSeconds(launchInterval);
        }

        boss.Anim.SetBool("Pattern2", false);


        yield return new WaitForSeconds(4.0f);

        boss.Agent.enabled = true;
        boss.OnPatternEnd();
    }

    private IEnumerator ScaleUpRock(GameObject rock, float targetScale, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (rock == null) yield break;
            t += Time.deltaTime;
            rock.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * targetScale, t / duration);
            yield return null;
        }
        if (rock != null) rock.transform.localScale = Vector3.one * targetScale;
    }

    private IEnumerator FlyRockRoutine(BossModel boss, GameObject rock, CharacterModel target)
    {
        if (rock == null || target == null) yield break;

        Vector3 startPos = rock.transform.position;
        Vector3 targetPos = target.transform.position;
        targetPos.y = startPos.y; // 수평으로만 날아가도록 Y 고정

        Vector3 flyDirection = (targetPos - startPos).normalized;

        // 바위가 날아갈 정면을 바라보게 회전
        rock.transform.LookAt(rock.transform.position + flyDirection);

        float t = 0f;

        while (t < rockLifetime)
        {
            if (rock == null || target == null || target.isDie) yield break;

            t += Time.deltaTime;

            rock.transform.position += flyDirection * rockSpeed * Time.deltaTime;

            yield return null;
        }

        if (rock != null)
        {
            boss.patternObjects.Remove(rock);
            GameObject.Destroy(rock);
        }
    }

    private IEnumerator FillWarning(Transform innerDecal, float fillTime)
    {
        DecalProjector projector = innerDecal.GetComponent<DecalProjector>();

        Vector3 targetSize = projector.size;

        Vector3 startSize = new Vector3(0f, 0f, targetSize.z);
        projector.size = startSize;

        float t = 0f;
        while (t < fillTime)
        {
            t += Time.deltaTime;
            float progress = t / fillTime;
            projector.size = Vector3.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        projector.size = targetSize;
    }
}

public class ElderGolem_Pattern3 : BossPatternBase
{
    [Header("패턴 프리팹 세팅")]
    public GameObject centerAoePrefab; // 가운데 70씩 닳는 장판
    public GameObject lightOrbPrefab;  // 광명의 구 프리팹
    public GameObject warning;
    public GameObject effect;

    [Header("이동 및 궤도 세팅")]
    public float outerRadius = 18f;    // 바깥쪽 구 궤도 반지름
    public float innerRadius = 9f;     // 안쪽 구 궤도 반지름
    public float bigOrbRadius = 16f;     // 합쳐진 큰 구가 도는 궤도 반지름 (중간 지점)
    public float phase1Duration = 4f;  // 6시에서 12시까지 가는 시간
    public float phase2Duration = 10f;  // 큰 구가 한 바퀴(360도) 도는 시간

    [Header("크기 세팅")]
    public float aoeScale = 5f;       // 장판 스케일
    public float smallOrbScale = 3f;
    public float smallOrbScale2 = 5f;
    public float bigOrbScale = 8f;
    public float bigOrbSpinSpeed = 720.0f;

    public ElderGolem_Pattern3(GameObject centerAoe, GameObject lightOrb, GameObject warning, GameObject effect)
    {
        patternName = "Normal3";
        cooldown = 60f;
        weight = 20f;
        range = 30f;

        this.centerAoePrefab = centerAoe;
        this.lightOrbPrefab = lightOrb;
        this.warning = warning;
        this.effect = effect;
    }

    public override void Execute(BossModel boss)
    {
        lastUsedTime = Time.time;
        boss.StartCoroutine(PatternRoutine(boss));
    }

    private IEnumerator PatternRoutine(BossModel boss)
    {
        boss.Agent.enabled = false;
        boss.Anim.SetBool("Pattern3", true);

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.Elder_N3);


        Vector3 centerPos = boss.bossSpawnPoint.position; // 맵 중앙 좌표
        centerPos.y = boss.transform.position.y;
        boss.transform.position = centerPos;

        // 1. 보스 상승
        float upTime = 2f;
        float t = 0;
        Vector3 startPos = boss.transform.position;
        Vector3 endPos = startPos + Vector3.up * 8f; // 8만큼 상승

        while (t < upTime)
        {
            t += Time.deltaTime;
            boss.transform.position = Vector3.Lerp(startPos, endPos, t / upTime);
            yield return null;
        }

        // 2. 중앙 장판 소환 및 커지기 연출
        GameObject centerAoe = GameObject.Instantiate(centerAoePrefab, centerPos, Quaternion.identity);
        boss.patternObjects.Add(centerAoe);

        t = 0;
        while (t < 1.5f)
        {
            centerAoe.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(1f, 0f, 1f) * aoeScale, t / 1.5f);
            t += Time.deltaTime;
            yield return null;
        }

        Vector3 forward = Vector3.forward; // 맵 기준 12시 방향 
        Vector3 right = Vector3.right;

        // 6시 방향(-forward) 궤도 위에 구 생성
        Vector3 startOffset = -forward;
        Vector3 outerStartPos = centerPos + startOffset * outerRadius;
        Vector3 innerStartPos = centerPos + startOffset * innerRadius;

        GameObject outerOrb = GameObject.Instantiate(lightOrbPrefab, outerStartPos, Quaternion.identity);
        GameObject innerOrb = GameObject.Instantiate(lightOrbPrefab, innerStartPos, Quaternion.identity);

        boss.patternObjects.Add(outerOrb);
        boss.patternObjects.Add(innerOrb);

        outerOrb.transform.localScale = Vector3.zero;
        innerOrb.transform.localScale = Vector3.zero;

        // 3. 제자리(6시)에서 1.5초 동안 서서히 커짐
        t = 0;
        while (t < 1.5f)
        {
            outerOrb.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * smallOrbScale2, t / 1.5f);
            innerOrb.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * smallOrbScale, t / 1.5f);
            t += Time.deltaTime;
            yield return null;
        }

        outerOrb.transform.localScale = Vector3.one * smallOrbScale2;
        innerOrb.transform.localScale = Vector3.one * smallOrbScale;

        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // 🌟 Phase 1: 6시에서 12시로 이동
        // =========================================================================
        t = 0f;
        float startAngle = 180f;
        float endOuterAngle = 540f;
        float endInnerAngle = -180f;

        while (t < phase1Duration)
        {
            t += Time.deltaTime;
            float progress = t / phase1Duration;

            float currentOuterAngle = Mathf.Lerp(startAngle, endOuterAngle, progress);
            float currentInnerAngle = Mathf.Lerp(startAngle, endInnerAngle, progress);

            Vector3 outerOffset = (right * Mathf.Sin(currentOuterAngle * Mathf.Deg2Rad)) +
                                  (forward * Mathf.Cos(currentOuterAngle * Mathf.Deg2Rad));

            Vector3 innerOffset = (right * Mathf.Sin(currentInnerAngle * Mathf.Deg2Rad)) +
                                  (forward * Mathf.Cos(currentInnerAngle * Mathf.Deg2Rad));

            outerOrb.transform.position = centerPos + outerOffset * outerRadius;
            innerOrb.transform.position = centerPos + innerOffset * innerRadius;

            yield return null;
        }

        // =========================================================================
        // 🌟 Phase 1.5 (신규 추가): 두 구가 사이로 모이며 합쳐지는 연출 (Merge)
        // =========================================================================

        // 합쳐질 궤도 위치 계산 (바깥 궤도와 안쪽 궤도의 정중앙)
        Vector3 mergePos = centerPos - forward * bigOrbRadius; // 12시 방향의 사이 지점

        Vector3 outerStartMerge = outerOrb.transform.position;
        Vector3 innerStartMerge = innerOrb.transform.position;

        t = 0f;
        float mergeTime = 0.5f; // 합쳐지는 데 걸리는 시간
        while (t < mergeTime)
        {
            t += Time.deltaTime;
            float progress = t / mergeTime;

            // 1) 위치 이동: 두 구가 모두 중간 지점(mergePos)으로 끌려감
            outerOrb.transform.position = Vector3.Lerp(outerStartMerge, mergePos, progress);
            innerOrb.transform.position = Vector3.Lerp(innerStartMerge, mergePos, progress);

            // 2) 크기 변화: 바깥 구는 거대해지고, 안쪽 구는 작아지며 흡수되는 느낌을 줌
            outerOrb.transform.localScale = Vector3.Lerp(Vector3.one * smallOrbScale2, Vector3.one * bigOrbScale, progress);
            innerOrb.transform.localScale = Vector3.Lerp(Vector3.one * smallOrbScale, Vector3.zero, progress);

            yield return null;
        }

        // 완벽히 합쳐진 후 흡수당한 안쪽 구는 리스트에서 지우고 파괴
        boss.patternObjects.Remove(innerOrb);
        GameObject.Destroy(innerOrb);

        // 남은 바깥 구를 거대한 구로 사용
        GameObject bigOrb = outerOrb;
        bigOrb.transform.position = mergePos;
        bigOrb.transform.localScale = Vector3.one * bigOrbScale;

        // =========================================================================
        // 🌟 Phase 2: 거대해진 구가 한 바퀴 회전
        // =========================================================================
        t = 0f;
        float phase2StartAngle = 540f;
        float phase2EndAngle = 0f;

        while (t < phase2Duration)
        {
            t += Time.deltaTime;
            float progress = t / phase2Duration;

            // 🌟 핵심 수정 1: Mathf.SmoothStep을 사용하여 부드러운 S자 곡선 생성
            // 0% ~ 50% 구간은 점점 빨라지고(가속), 50% ~ 100% 구간은 점점 느려지며(감속) 멈춥니다.
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            float currentBigAngle = Mathf.Lerp(phase2StartAngle, phase2EndAngle, smoothProgress);

            Vector3 bigOffset = (right * Mathf.Sin(currentBigAngle * Mathf.Deg2Rad)) +
                                (forward * Mathf.Cos(currentBigAngle * Mathf.Deg2Rad));

            bigOrb.transform.position = centerPos + bigOffset * bigOrbRadius;

            yield return null;
        }

        Vector3 finalExplosionPos = centerPos + forward * bigOrbRadius;
        bigOrb.transform.position = finalExplosionPos;

        if (warning != null)
        {
            Vector3 warnPos = bigOrb.transform.position;
            warnPos.y = centerPos.y + 0.8f;

            GameObject expWarn = GameObject.Instantiate(warning, warnPos, Quaternion.identity);
            boss.patternObjects.Add(expWarn);

            Transform innerDecal = expWarn.transform.Find("InnerDecal");
            DecalProjector projector = innerDecal != null ? innerDecal.GetComponent<DecalProjector>() : null;

            Vector3 targetSize = Vector3.zero;
            Vector3 startSize = Vector3.zero;

            if (projector != null)
            {
                targetSize = projector.size;
                startSize = new Vector3(0f, 0f, targetSize.z);
                projector.size = startSize;
            }

            // 4초 동안 동시 진행 루프
            float warningTime = 4f;
            t = 0f;
            while (t < warningTime)
            {
                t += Time.deltaTime;
                float progress = t / warningTime;

                // 1) 바닥 경고 데칼 Size 늘리기
                if (projector != null)
                {
                    projector.size = Vector3.Lerp(startSize, targetSize, progress);
                }

                // 2) 멈춰있는 구체를 Y축(Vector3.up) 기준으로 매 프레임 회전시킴
                if (bigOrb != null)
                {
                    bigOrb.transform.Rotate(Vector3.up * bigOrbSpinSpeed * Time.deltaTime);
                }

                yield return null;
            }

            // 최종 크기 오차 보정
            if (projector != null) projector.size = targetSize;

            boss.patternObjects.Remove(expWarn);
            GameObject.Destroy(expWarn);
        }
        else
        {
            // 경고 데칼 프리팹이 할당되지 않았을 때도 구체는 자전하도록 방어 코드 추가
            float warningTime = 4f;
            t = 0f;
            while (t < warningTime)
            {
                t += Time.deltaTime;
                if (bigOrb != null) bigOrb.transform.Rotate(Vector3.up * bigOrbSpinSpeed * Time.deltaTime);
                yield return null;
            }
        }

        if (Vector3.Distance(bigOrb.transform.position, boss.Target.transform.position) < 36)
        {
            boss.Target.Damaged(0.9f, true); // 90% 퍼센트 대미지
            
        }

        GameObject effect = GameObject.Instantiate(this.effect, finalExplosionPos, Quaternion.identity);
        boss.patternObjects.Add(effect);
        boss.StartCoroutine(DestroyEffect(effect,2f));

        // =========================================================================
        // 🌟 종료 연출
        // =========================================================================
        boss.patternObjects.Remove(bigOrb);
        GameObject.Destroy(bigOrb);

        t = 0f;
        Vector3 aoeStart = centerAoe.transform.position;
        Vector3 aoeEnd = aoeStart + Vector3.down * 5f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            centerAoe.transform.position = Vector3.Lerp(aoeStart, aoeEnd, t / 1f);
            yield return null;
        }

        boss.patternObjects.Remove(centerAoe);
        GameObject.Destroy(centerAoe);

        t = 0f;
        while (t < upTime)
        {
            t += Time.deltaTime;
            boss.transform.position = Vector3.Lerp(endPos, startPos, t / upTime);
            yield return null;
        }

        boss.Anim.SetBool("Pattern3", false);

        yield return new WaitForSeconds(3f);

        boss.Agent.enabled = true;
        boss.OnPatternEnd();
    }

    private IEnumerator FillWarning(Transform innerDecal, float fillTime)
    {
        DecalProjector projector = innerDecal.GetComponent<DecalProjector>();

        Vector3 targetSize = projector.size;

        Vector3 startSize = new Vector3(0f, 0f, targetSize.z);
        projector.size = startSize;

        float t = 0f;
        while (t < fillTime)
        {
            t += Time.deltaTime;
            float progress = t / fillTime;
            projector.size = Vector3.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        projector.size = targetSize;
    }

    private IEnumerator DestroyEffect(GameObject effect, float remainTime)
    {
        yield return new WaitForSeconds(remainTime);
        GameObject.Destroy(effect);
    }
}

