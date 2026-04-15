using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine;

[System.Serializable]
public class D1_Final_Normal1Data
{
    public GameObject prefab;
    public LayerMask groundLayer;
    public int boxCount = 3;
    public float spawnRadius = 30f;
    public BuffSO stunDebuffSO;
    public float stunDuration = 1.5f;
}

[System.Serializable]
public class D1_Final_Normal2Data
{
    public GameObject prefab;
    public BuffSO slowDebuffSO;
    public int knifeCount = 20;
    public float damagePercent = 0.1f; 
    public float slowPercent = 0.2f;
    public float slowDuration = 5f;
}

[System.Serializable]
public class D1_Final_Normal3Data
{
    public GameObject swingPrefab;
    public GameObject warning1;
    public GameObject warning2;
    public AnimationCurve jumpCurve;
    public float damagePercent = 0.2f;
}

[System.Serializable]
public class D1_Final_Normal4Data
{
    public float damagePercent = 0.2f;
    public GameObject warning1;
    public GameObject warning2;
}

[System.Serializable]
public class D1_Final_Normal5Data
{
    public GameObject hat;
    public GameObject bulletPrefab;
    public GameObject warningPrefab;
    public float damagePercent;
}

[System.Serializable]
public class D1_Final_Special1Data
{
    public GameObject warning1;
    public GameObject warning2;
    public float damagePercent;
}

[System.Serializable]
public class D1_Final_Special2Data
{
    public GameObject prefab;
}

[System.Serializable]
public class D1_Final_Special3Data
{
    public GameObject prefab;
}

[System.Serializable]
public class D1_Final_Special4Data
{
    public GameObject yabawiPrefab;
    public int shuffleCount = 5;
    public float swapDuration = 1.5f;
    public GameObject crownPrefab;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public BuffSO panicBuffSO;
    public BuffSO stunBuffSO;
    public GameObject crownSpawnPos;
    public GameObject swingPrefab;
    public GameObject ballPrefab;
}

[System.Serializable]
public class D1_Final_Special5Data
{
    public D1_Chess prefab;
    public GameObject cutSceneObj;
}

public class D1_FinalBoss : BossModel
{
    [Header("일반 패턴 공용 변수")]
    public Transform center;
    public Transform playerStartPos;


    [Header("각 일반 패턴 변수")]
    public D1_Final_Normal1Data pattern1;
    public D1_Final_Normal2Data pattern2;
    public D1_Final_Normal3Data pattern3;
    public D1_Final_Normal4Data pattern4;
    public D1_Final_Normal5Data pattern5;

    [Header("각 특수 패턴 변수")]
    public D1_Final_Special1Data Special1;
    public D1_Final_Special2Data Special2;
    public D1_Final_Special3Data Special3;
    public D1_Final_Special4Data Special4;
    public D1_Final_Special5Data Special5;

    public SkinnedMeshRenderer[] bossMeshs;

    protected override void Start()
    {
        base.Start();

        center = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;
        playerStartPos = GameObject.FindGameObjectWithTag("PlayerStart").transform;
        bossSpawnPoint = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;

        pattern5.hat = GameObject.FindGameObjectWithTag("D1_Final_N5");

        Special5.prefab = GameObject.FindGameObjectWithTag("D1_Final_S5").GetComponent<D1_Chess>();
        Special5.cutSceneObj = GameObject.FindGameObjectWithTag("D1_Final_Cut2");

        bossMeshs = GetComponentsInChildren<SkinnedMeshRenderer>();

        normalPatterns.Add(new D1_Final_Normal1(pattern1, center));
        normalPatterns.Add(new D1_Final_Normal2(pattern2, center));
        normalPatterns.Add(new D1_Final_Normal3(pattern3));
        normalPatterns.Add(new D1_Final_Normal4(pattern4));
        normalPatterns.Add(new D1_Final_Normal5(pattern5));

        AudioManager.instance.PlayBGM(C_Enums.BGM_List.D1_Final_BGM2);
        AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_Enter);

    }

    protected override void StartSpecialPattern(BossSpecialPattern pattern)
    {
        Debug.Log($"🚨 [기믹 발동] {pattern.patternName} 시작!");

        if (pattern.patternName == "쇼타임")
            StartCoroutine(Special_ShowTime());
        else if (pattern.patternName == "운명의 점")
            StartCoroutine(Special_Aracna());
        else if (pattern.patternName == "칩막기")
            StartCoroutine(Special_Chip());
        else if (pattern.patternName == "야바위")
            StartCoroutine(Special_Mix());
        else if (pattern.patternName == "체크메이트")
            StartCoroutine(Special_Chess());
    }

    #region 특수 패턴

    IEnumerator ReadyForSpecial()
    {
        Agent.enabled = false;

        Vector3 playerPos = Target.transform.position;

        playerPos.x = 0;
        playerPos.z = 0;

        transform.Rotate(playerPos);

        GameObject swing = GameObject.Instantiate(
            pattern3.swingPrefab,
            transform.position + (transform.forward * 3f) + new Vector3(0f, 60f, 0), // 위치 수정
            transform.rotation
            );

        patternObjects.Add(swing);

        while (swing.transform.position.y > 2)
        {
            swing.transform.Translate(Vector3.down * 30 * Time.deltaTime, Space.World);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = swing.transform.position + Vector3.up * 2.5f;

        float time = 0f;

        Anim.SetTrigger("Jump");

        while (time < 1.5f)
        {
            time += Time.deltaTime;

            // 1. 진행도 (0.0 ~ 1.0)
            float t = time / 1.5f;

            // 2. 앞으로 나아가는 '속도' 조절 (이징)
            float curveT = pattern3.jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 2.5f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (transform.position.y < 20)
        {
            transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            swing.transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            yield return null;
        }

        foreach (SkinnedMeshRenderer mesh in bossMeshs)
        {
            mesh.enabled = false;
        }

        Destroy(swing);

        GameEvent.OnBossStateChange?.Invoke(null);
    }

    public void EndSpecialPattern()
    {
        StartCoroutine(EndSpecial());
    }

    IEnumerator EndSpecial()
    {
        foreach (SkinnedMeshRenderer mesh in bossMeshs)
        {
            mesh.enabled = true;
        }

        GameEvent.OnBossStateChange?.Invoke(this);

        GameObject swing = GameObject.Instantiate(
            pattern3.swingPrefab,
            center.transform.position + new Vector3(0f, 20f, 3f), // 위치 수정
            Quaternion.identity
            );

        patternObjects.Add(swing);

        transform.position = swing.transform.position + Vector3.up * 2.5f;

        Vector3 centerPos = center.transform.position;

        centerPos.x = 0;
        centerPos.z = 0;

        transform.Rotate(centerPos);

        yield return new WaitForSeconds(0.5f);

        while (swing.transform.position.y > 3)
        {
            transform.Translate(Vector3.down * 40 * Time.deltaTime, Space.World);
            swing.transform.Translate(Vector3.down * 40 * Time.deltaTime, Space.World);
            yield return null;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = center.transform.position;
        targetPos.y = 0;

        float time = 0f;

        Anim.SetTrigger("Jump");

        while (time < 1.5f)
        {
            time += Time.deltaTime;

            // 1. 진행도 (0.0 ~ 1.0)
            float t = time / 1.5f;

            // 2. 앞으로 나아가는 '속도' 조절 (이징)
            float curveT = pattern3.jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 2.5f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Agent.enabled = true;

        isDoingSpecial = false;

        while (swing.transform.position.y < 20)
        {
            swing.transform.Translate(Vector3.up * 40 * Time.deltaTime, Space.World);
            yield return null;
        }

        SetImmunity(false);

        Destroy(swing);


    }

    IEnumerator Special_ShowTime()
    {
        yield return new WaitForSeconds(2f);

        GameObject warning1 = Instantiate(Special1.warning1, center);
        patternObjects.Add(warning1);

        yield return new WaitForSeconds(2f);   

        Destroy(warning1);

        yield return new WaitForSeconds(0.2f);

        Collider[] targets = Physics.OverlapSphere(center.transform.position, 20f, LayerMask.GetMask("Player"));

        foreach (Collider target in targets)
        {
            Target.Damaged(Special1.damagePercent, true);
        }

        yield return new WaitForSeconds(0.2f);

        GameObject warning2 = Instantiate(Special1.warning2, center);
        patternObjects.Add(warning2);

        yield return new WaitForSeconds(2f);

        Destroy(warning2);

        yield return new WaitForSeconds(0.2f);

        targets = Physics.OverlapSphere(center.transform.position, 40f, LayerMask.GetMask("Player"));
        Collider[] safe = Physics.OverlapSphere(center.transform.position, 20f, LayerMask.GetMask("Player"));

        var finalHits = targets.Except(safe);

        foreach (Collider target in finalHits)
        {
            Target.Damaged(Special1.damagePercent, true);
        }

        yield return new WaitForSeconds(1f);

        isDoingSpecial = false;
    }

    IEnumerator Special_Aracna()
    {
        SetImmunity(true);

        yield return StartCoroutine(ReadyForSpecial());

        Vector3 spawnPos = center.position + new Vector3(0, -1.43f, 0);
        GameObject special = Instantiate(Special2.prefab, spawnPos, Quaternion.identity);
        patternObjects.Add(special);
        special.GetComponentInChildren<SurvivalPattern1>().Init(this);
        AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_S2);
    }

    IEnumerator Special_Chip()
    {
        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(EndSpecial());

        isDoingSpecial = false;
    }

    IEnumerator Special_Mix()
    {
        SetImmunity(true);

        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(1f);

        GameObject yabawi = Instantiate(Special4.yabawiPrefab, center.transform.position - new Vector3(0, 0, 4),Quaternion.identity);
        patternObjects.Add(yabawi);

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_S4);

        yabawi.GetComponent<D1_Yabawe>().StartYabawi(this);

        // 광대 생성
        StartCoroutine(SpawnCrown());
        StartCoroutine(SpawnSwing());
        StartCoroutine(SpawnBall());

    }

    private IEnumerator SpawnCrown()
    {
        GameObject crownSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(crownSpawnPos);

        int childCount = crownSpawnPos.transform.childCount;
        Transform[] spawnPoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            spawnPoints[i] = crownSpawnPos.transform.GetChild(i);
        }

        // 4. 배열 무작위 섞기 (Fisher-Yates Shuffle)
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // i부터 배열 끝 사이에서 무작위 인덱스 뽑기
            int randomIndex = Random.Range(i, spawnPoints.Length);

            // 현재 자리(i)와 무작위로 뽑힌 자리(randomIndex)의 값을 서로 교환(Swap)
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temp;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject crown = Instantiate(Special4.crownPrefab, spawnPoints[i].position, Quaternion.identity);
            crown.GetComponent<D1_Crown>().Init(Special4.bulletPrefab, Special4.bulletSpeed, Special4.panicBuffSO, Special4.stunBuffSO, Target);
            yield return new WaitForSeconds(1f);
        }

        Destroy(crownSpawnPos);
    }

    private IEnumerator SpawnSwing()
    {
        GameObject swingSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(swingSpawnPos);

        // 2. 자식들을 배열 대신 List에 담기 (넣고 빼기 쉽게 하기 위함)
        int childCount = swingSpawnPos.transform.childCount;
        List<Transform> availablePoints = new List<Transform>();

        for (int i = 0; i < childCount; i++)
        {
            availablePoints.Add(swingSpawnPos.transform.GetChild(i));
        }

        // 3. 중복 없이 랜덤으로 3개 뽑기
        int pickCount = Mathf.Min(3, availablePoints.Count); // 혹시 자식이 3개 미만일 때의 에러 방지용 안전장치
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < pickCount; i++)
        {
            // 남은 위치들 중에서 무작위로 하나 선택
            int randomIndex = Random.Range(0, availablePoints.Count);

            // 선택된 위치를 최종 타겟 리스트에 추가
            selectedPoints.Add(availablePoints[randomIndex]);

            // ⭐️ [핵심] 방금 뽑은 위치를 후보 리스트에서 아예 지워버림 (중복 절대 불가)
            availablePoints.RemoveAt(randomIndex);
        }

        // 4. 뽑힌 3개의 위치에 각각 그네 생성 및 발사
        foreach (Transform spawnPoint in selectedPoints)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = 0;

            // [방향 설정 1] Center를 바라보는 방향 계산
            Vector3 dirToCenter = center.transform.position - spawnPos;
            dirToCenter.y = 0f; // 수평 비행을 위해 높이 차이 무시

            Quaternion lookRotation = Quaternion.LookRotation(dirToCenter);

            // [방향 설정 2] -22.5 ~ +22.5도 사이의 랜덤 오프셋
            float randomAngle = Random.Range(-15f, 15f);
            Quaternion randomOffset = Quaternion.Euler(0f, randomAngle, 0f);

            // 최종 조준 각도 = 기본 방향 * 랜덤 오프셋
            Quaternion finalRotation = lookRotation * randomOffset;

            // 5. 그네 생성
            GameObject swingObj = Instantiate(Special4.swingPrefab, spawnPos, finalRotation);
            patternObjects.Add(swingObj);

            swingObj.GetComponent<D1_Swing>().Init();

            yield return new WaitForSeconds(2.5f);
        }

        Destroy(swingSpawnPos);
    }

    private IEnumerator SpawnBall()
    {
        GameObject ballSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(ballSpawnPos);

        // 2. 자식들을 배열 대신 List에 담기 (넣고 빼기 쉽게 하기 위함)
        int childCount = ballSpawnPos.transform.childCount;
        List<Transform> availablePoints = new List<Transform>();

        for (int i = 0; i < childCount; i++)
        {
            availablePoints.Add(ballSpawnPos.transform.GetChild(i));
        }

        // 3. 중복 없이 랜덤으로 3개 뽑기
        int pickCount = Mathf.Min(4, availablePoints.Count); // 혹시 자식이 3개 미만일 때의 에러 방지용 안전장치
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < pickCount; i++)
        {
            // 남은 위치들 중에서 무작위로 하나 선택
            int randomIndex = Random.Range(0, availablePoints.Count);

            // 선택된 위치를 최종 타겟 리스트에 추가
            selectedPoints.Add(availablePoints[randomIndex]);

            // ⭐️ [핵심] 방금 뽑은 위치를 후보 리스트에서 아예 지워버림 (중복 절대 불가)
            availablePoints.RemoveAt(randomIndex);
        }

        // 4. 뽑힌 3개의 위치에 각각 그네 생성 및 발사
        foreach (Transform spawnPoint in selectedPoints)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = 0;

            // [방향 설정 1] Center를 바라보는 방향 계산
            Vector3 dirToCenter = center.transform.position - spawnPos;
            dirToCenter.y = 0f; // 수평 비행을 위해 높이 차이 무시

            Quaternion lookRotation = Quaternion.LookRotation(dirToCenter);

            // [방향 설정 2] -22.5 ~ +22.5도 사이의 랜덤 오프셋
            float randomAngle = Random.Range(-22.5f, 22.5f);
            Quaternion randomOffset = Quaternion.Euler(0f, randomAngle, 0f);

            // 최종 조준 각도 = 기본 방향 * 랜덤 오프셋
            Quaternion finalRotation = lookRotation * randomOffset;

            // 5. 그네 생성
            GameObject ball = Instantiate(Special4.ballPrefab, spawnPos, finalRotation);
            patternObjects.Add(ball);

            ball.GetComponent<D1_Ball>().Init();

            yield return new WaitForSeconds(3.5f);
        }

        Destroy(ballSpawnPos);
    }

    IEnumerator Special_Chess()
    {
        SetImmunity(true);

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(0.5f);

        Target.canMove = false;

        yield return new WaitForSeconds(0.5f);

        GameEvent.OnUIInvisable?.Invoke();

        Special5.cutSceneObj.SetActive(true);
        PlayableDirector director = Special5.cutSceneObj.GetComponent<PlayableDirector>();
        director.Play();

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlayBGM(C_Enums.BGM_List.D1_Final_BGM3);

        if (director != null)
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        else
            yield return new WaitForSeconds(1f);

        Target.Navmesh.enabled = false;

        Target.transform.position = Special5.prefab.startPos.position;

        Target.Navmesh.enabled = true;

        yield return new WaitForSeconds(0.5f);


        Target.SetCanMove();
        GameEvent.OnMainUIviable?.Invoke();


        yield return new WaitForSeconds(0.5f);

        Special5.prefab.StartCheckmate(this);
    }

    #endregion

    #region 일반 패턴

    

    #endregion
}
