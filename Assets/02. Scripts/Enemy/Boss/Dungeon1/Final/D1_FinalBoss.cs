using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject prefab;
    public int shuffleCount = 5;
    public float swapDuration = 1.5f;
}

[System.Serializable]
public class D1_Final_Special5Data
{
    public D1_Chess prefab;
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


    private MeshRenderer[] bossMeshs;

    protected override void Start()
    {
        base.Start();

        bossMeshs = GetComponentsInChildren<MeshRenderer>();

        normalPatterns.Add(new D1_Final_Normal1(pattern1, center));
        normalPatterns.Add(new D1_Final_Normal2(pattern2, center));
        normalPatterns.Add(new D1_Final_Normal3(pattern3));
        normalPatterns.Add(new D1_Final_Normal4(pattern4));
        normalPatterns.Add(new D1_Final_Normal5(pattern5));
    }

    protected override void StartSpecialPattern(BossSpecialPattern pattern)
    {
        Debug.Log($"🚨 [기믹 발동] {pattern.patternName} 시작!");

        if (pattern.patternName == "쇼타임")
            StartCoroutine(Special_Mix());
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

        Destroy(swing);

    }

    public void EndSpecialPattern()
    {
        StartCoroutine(EndSpecial());
    }

    IEnumerator EndSpecial()
    {
        GameObject swing = GameObject.Instantiate(
            pattern3.swingPrefab,
            center.transform.position + new Vector3(0f, 20f, 3f), // 위치 수정
            Quaternion.identity
            );

        patternObjects.Add(swing);

        transform.position = swing.transform.position + Vector3.up * 2.5f;
        transform.rotation = Quaternion.identity;

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
        yield return StartCoroutine(ReadyForSpecial());

        Vector3 spawnPos = center.position + new Vector3(0, -1.43f, 0);
        GameObject special = Instantiate(Special2.prefab, spawnPos, Quaternion.identity);
        patternObjects.Add(special);
        special.GetComponentInChildren<SurvivalPattern1>().Init(this);
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
        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(1f);

        GameObject yabawi = Instantiate(Special4.prefab, center.transform.position - new Vector3(0, 0, 4),Quaternion.identity);
        patternObjects.Add(yabawi);

        yabawi.GetComponent<D1_Yabawe>().StartYabawi(this);
    }
    IEnumerator Special_Chess()
    {
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(1f);

        Target.Navmesh.enabled = false;

        Target.transform.position = Special5.prefab.startPos.position;

        Target.Navmesh.enabled = true;

        yield return new WaitForSeconds(1f);

        Special5.prefab.StartCheckmate(this);
    }

    #endregion

    #region 일반 패턴

    

    #endregion
}
