using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class D1_FianlPatterns : MonoBehaviour
{
   
}

public class D1_Final_Normal1 : BossPatternBase
{
    public GameObject box;
    public LayerMask groundMask;
    public int count;
    public float radius;
    public Transform center;
    public BuffSO stunDebuffSO;
    public float stunDuration;

    public D1_Final_Normal1(D1_Final_Normal1Data data, Transform center)
    {
        patternName = "Normal1";
        cooldown = 30f;
        weight = 30f;
        range = 20f;

        this.box = data.prefab;
        this.groundMask = data.groundLayer;
        this.count = data.boxCount;
        this.radius = data.spawnRadius;
        this.center = center;
        this.stunDebuffSO = data.stunDebuffSO;
        this.stunDuration = data.stunDuration;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(SpawnBox(boss, 2f));
    }

    private IEnumerator SpawnBox(BossModel boss, float delay)
    {
        boss.Anim.SetTrigger(patternName);

        yield return new WaitForSeconds(delay);

        Vector3 mapCenter = center.position;

        int index = 0;
        while (index < 3)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;

            Vector3 rayStart = new Vector3(
                    mapCenter.x + randomCircle.x,
                    mapCenter.y + 10f,
                    mapCenter.z + randomCircle.y
                );

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, groundMask))
            {
                index++;
                // 추후 풀로 바꿔야함
                GameObject gbBox = GameObject.Instantiate(box, hit.point, Quaternion.identity);
                boss.patternObjects.Add(gbBox);
                gbBox.GetComponent<D1_Box>().Init(boss.Target, stunDebuffSO, stunDuration);
            }
            else
            {
                continue;
            }
        }

        boss.OnPatternEnd();
    }
}

public class D1_Final_Normal2 : BossPatternBase
{
    private GameObject knife;
    private BuffSO slowDebuffSO;
    private int count;
    private float damagePercent;
    private float slowPercent;
    private float slowDuration;
    private Transform center;

    public D1_Final_Normal2(D1_Final_Normal2Data data, Transform center)
    {
        patternName = "Normal2";
        cooldown = 20f;
        weight = 20f;
        range = 15f;

        this.knife = data.prefab;
        this.slowDebuffSO = data.slowDebuffSO;
        this.count = data.knifeCount;
        this.damagePercent = data.damagePercent;
        this.slowPercent = data.slowPercent;
        this.slowDuration = data.slowDuration;
        this.center = center;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(SpawnKnife(boss, 3f));
    }

    private IEnumerator SpawnKnife(BossModel boss, float delay)
    {
        boss.Anim.SetTrigger(patternName);

        yield return new WaitForSeconds(delay);

        Vector3 center = boss.transform.position;
        center.y = 1f;

        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            Quaternion randomRot = Quaternion.Euler(0f,randomAngle, 0f);

            GameObject gbKnife =  GameObject.Instantiate(knife, center, randomRot);
            boss.patternObjects.Add(gbKnife);
            gbKnife.GetComponent<D1_Knife>().Init(slowDebuffSO,damagePercent,slowPercent,slowDuration,boss.Target, boss.transform, this.center);

            yield return new WaitForSeconds(0.2f);
        }

        boss.OnPatternEnd();
    }
}

public class D1_Final_Normal3 : BossPatternBase
{
    public GameObject normal3Swing;
    public float damagePercent;
    public AnimationCurve jumpCurve;
    public GameObject normal3Warning1;
    public GameObject normal3Warning2;

    public D1_Final_Normal3(D1_Final_Normal3Data data)
    {
        patternName = "Normal3";
        cooldown = 60f;
        weight = 30f;
        range = 10f;

        this.jumpCurve = data.jumpCurve;
        this.normal3Swing = data.swingPrefab;
        this.damagePercent = data.damagePercent;
        this.normal3Warning1 = data.warning1;
        this.normal3Warning2 = data.warning2;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(JumpUp(boss));
    }

    private IEnumerator JumpUp(BossModel boss)
    {
        //boss.Anim.SetTrigger(patternName);

        GameObject swing = GameObject.Instantiate(
            normal3Swing,
            boss.transform.position + new Vector3(0,20f,3f), // 위치 수정
            boss.transform.rotation
            );

        boss.patternObjects.Add(swing);

        boss.EnableCounter();
        yield return new WaitForSeconds(2f);
        boss.DisableCounter();

        while (swing.transform.position.y > 3)
        {
            swing.transform.Translate(Vector3.down * 30 * Time.deltaTime, Space.World);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (boss.Agent != null) boss.Agent.enabled = false;

        Vector3 startPos = boss.transform.position;
        Vector3 targetPos = swing.transform.position + Vector3.up * 2.5f;

        float time = 0f;
        while (time < 1.5f)
        {
            time += Time.deltaTime;

            // 1. 진행도 (0.0 ~ 1.0)
            float t = time / 1.5f;

            // 2. 앞으로 나아가는 '속도' 조절 (이징)
            float curveT = jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 2.5f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            boss.transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        boss.transform.position = targetPos;

        yield return new WaitForSeconds(0.5f);

        while (boss.transform.position.y < 20)
        {
            boss.transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            swing.transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            yield return null;
        }

        GameObject.Destroy(swing);

        Vector3 dropPos = boss.Target.transform.position;

        dropPos.y = boss.transform.position.y;

        boss.transform.position = dropPos;

        yield return new WaitForSeconds(0.5f);

        Vector3 warning1pos = boss.transform.position;
        warning1pos.y = 0f;

        GameObject warning1 = GameObject.Instantiate(normal3Warning1, warning1pos,  Quaternion.identity);
        boss.patternObjects.Add(warning1);

        yield return new WaitForSeconds(2f);

        GameObject.Destroy(warning1);

        while (boss.transform.position.y > 0)
        {
            boss.transform.Translate(Vector3.down * 100 * Time.deltaTime, Space.World);
            yield return null;

        }

        Vector3 landPos = boss.transform.position;
        landPos.y = 0.5f;
        boss.transform.position = landPos;

        int playerLayer = LayerMask.GetMask("Player");

        Collider[] hit = Physics.OverlapSphere(boss.transform.position, 6f, playerLayer);
        foreach(Collider c in hit)
        {
            CharacterModel model = c.GetComponent<CharacterModel>();
            model.Damaged(damagePercent, true);
        }

        landPos.y = 0f;

        GameObject warning2 = GameObject.Instantiate(normal3Warning2, landPos, Quaternion.identity);
        boss.patternObjects.Add(warning2);

        yield return new WaitForSeconds(1f);

        GameObject.Destroy(warning2);

        hit = Physics.OverlapSphere(boss.transform.position, 12f, playerLayer);
        Collider[] safe = Physics.OverlapSphere(boss.transform.position, 6f, playerLayer);

        var finalHits = hit.Except(safe);

        foreach (Collider c in finalHits)
        {
            CharacterModel model = c.GetComponent<CharacterModel>();
            model.Damaged(damagePercent, true);
        }

        yield return new WaitForSeconds(1f);

        if (boss.Agent != null) boss.Agent.enabled = true;

        boss.OnPatternEnd();
    }

}

public class D1_Final_Normal4 : BossPatternBase
{
    public float damagePercent;
    public GameObject warning1;
    public GameObject warning2;

    public D1_Final_Normal4(D1_Final_Normal4Data data)
    {
        patternName = "Normal4";
        cooldown = 15f;
        weight = 30f;
        range = 8f;

        this.damagePercent = data.damagePercent;
        this.warning1 = data.warning1;
        this.warning2 = data.warning2;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(CrossFire(boss));
    }

    private IEnumerator CrossFire(BossModel boss)
    {
        boss.Anim.SetTrigger(patternName);

        yield return new WaitForSeconds(0.5f);

        GameObject gwarning1 = GameObject.Instantiate(warning1, boss.transform.position, boss.transform.rotation); ;
        boss.patternObjects.Add(gwarning1);

        yield return new WaitForSeconds(1.5f);

        Vector3 center = boss.transform.position;
        Quaternion rot = boss.transform.rotation;

        Vector3 forwardBox = new Vector3(3.25f, 2.5f, 15f);
        Vector3 sideBox = new Vector3(15f, 2.5f, 3.25f);

        int playerLayer = LayerMask.GetMask("Player");

        Collider[] forward = Physics.OverlapBox(center,forwardBox,rot,playerLayer);
        Collider[] side = Physics.OverlapBox(center,sideBox,rot,playerLayer);

        HashSet<Collider> uniqueTargets = new HashSet<Collider>();

        foreach (Collider hit in forward) uniqueTargets.Add(hit);
        foreach (Collider hit in side) uniqueTargets.Add(hit);

        foreach(Collider hit in uniqueTargets)
        {
            CharacterModel model = hit.GetComponent<CharacterModel>();
            if (model != null)
                model.Damaged(damagePercent, true);
        }

        GameObject.Destroy(gwarning1);

        yield return new WaitForSeconds(0.5f);

        GameObject gwarning2 = GameObject.Instantiate(warning2, boss.transform.position, boss.transform.rotation); ;
        boss.patternObjects.Add(gwarning2);

        yield return new WaitForSeconds(1.5f);

        rot = boss.transform.rotation * Quaternion.Euler(0f, 45f, 0f);

        forward = Physics.OverlapBox(center, forwardBox, rot, playerLayer);
        side = Physics.OverlapBox(center, sideBox, rot, playerLayer);

        uniqueTargets = new HashSet<Collider>();

        foreach (Collider hit in forward) uniqueTargets.Add(hit);
        foreach (Collider hit in side) uniqueTargets.Add(hit);

        foreach (Collider hit in uniqueTargets)
        {
            CharacterModel model = hit.GetComponent<CharacterModel>();
            if (model != null)
                model.Damaged(damagePercent, true);
        }

        GameObject.Destroy(gwarning2);

        boss.OnPatternEnd();
    }
}

public class D1_Final_Normal5 : BossPatternBase
{
    public GameObject hat;
    public GameObject normal5Bullet;
    public GameObject normal5Warning;
    public float damagePercent;


    public D1_Final_Normal5(D1_Final_Normal5Data data)
    {
        patternName = "Normal5";
        cooldown = 60f;
        weight = 30f;
        range = 50f;

        this.hat = data.hat;
        damagePercent = data.damagePercent;
        normal5Bullet = data.bulletPrefab;
        normal5Warning = data.warningPrefab;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(HatShot(boss));
    }

    private IEnumerator HatShot(BossModel boss)
    {
        int shotCount = 0;

        Vector3 dir = (boss.Target.transform.position - hat.transform.position).normalized;

        while (shotCount < 3)
        {
            GameObject warning = GameObject.Instantiate(normal5Warning, hat.transform.position, Quaternion.identity);
            boss.patternObjects.Add(warning);

            float timer = 0;

            while (timer < 2f)
            {
                timer += Time.deltaTime;
                Vector3 lookTarget = boss.Target.transform.position;

                // ⭐️ 2. 타겟의 Y(높이) 값을 경고장판의 Y(높이) 값과 똑같이 덮어씌웁니다.
                lookTarget.y = warning.transform.position.y;

                // ⭐️ 3. 수평이 맞춰진 가짜 목표점을 바라보게 합니다!
                warning.transform.LookAt(lookTarget);

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            GameObject bullet = GameObject.Instantiate(normal5Bullet, hat.transform.position, warning.transform.rotation);
            boss.patternObjects.Add(bullet);
            
            bullet.GetComponent<D1_Bullet>().Init(damagePercent,50f, boss.Target, true);
            GameObject.Destroy(warning);

            yield return new WaitForSeconds(0.5f);

            shotCount++;
        }

        GameObject bosswarning = GameObject.Instantiate(normal5Warning, hat.transform.position, Quaternion.identity);
        boss.patternObjects.Add(bosswarning);

        float bosstimer = 0;

        while (bosstimer < 2f)
        {
            bosstimer += Time.deltaTime;
            Vector3 lookTarget = boss.transform.position;

            // ⭐️ 2. 타겟의 Y(높이) 값을 경고장판의 Y(높이) 값과 똑같이 덮어씌웁니다.
            lookTarget.y = bosswarning.transform.position.y;

            // ⭐️ 3. 수평이 맞춰진 가짜 목표점을 바라보게 합니다!
            bosswarning.transform.LookAt(lookTarget);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        GameObject bossBullet = GameObject.Instantiate(normal5Bullet, hat.transform.position, bosswarning.transform.rotation);
        boss.patternObjects.Add(bossBullet);
        bossBullet.GetComponent<D1_Bullet>().Init(damagePercent,80f, boss.Target, false);

        GameObject.Destroy(bosswarning);

        yield return new WaitForSeconds(2f);

        boss.OnPatternEnd();
    }
}
