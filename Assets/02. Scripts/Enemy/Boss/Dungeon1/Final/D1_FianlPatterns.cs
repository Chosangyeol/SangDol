using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    public D1_Final_Normal1(GameObject box, LayerMask layer, int count, float radius, Transform center)
    {
        patternName = "Normal1";
        cooldown = 30f;
        weight = 30f;
        range = 20f;

        this.box = box;
        this.groundMask = layer;
        this.count = count;
        this.radius = radius;
        this.center = center;
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
                GameObject.Instantiate(box, hit.point, Quaternion.identity);
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
    public GameObject knife;
    public int count;

    public D1_Final_Normal2(GameObject knife, int count)
    {
        patternName = "Normal2";
        cooldown = 20f;
        weight = 20f;
        range = 15f;

        this.knife = knife;
        this.count = count;
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

            GameObject.Instantiate(knife, center, randomRot);

            yield return new WaitForSeconds(0.2f);
        }

        boss.OnPatternEnd();
    }
}

public class D1_Final_Normal3 : BossPatternBase
{
    public D1_Final_Normal3()
    {
        patternName = "Normal3";
        cooldown = 60f;
        weight = 30f;
        range = 10f;
    }

    public override void Execute(BossModel boss)
    {
        base.Execute(boss);

        boss.StartCoroutine(JumpUp(boss));
    }

    private IEnumerator JumpUp(BossModel boss)
    {
        //boss.Anim.SetTrigger(patternName);

        yield return new WaitForSeconds(2f);

        if (boss.Agent != null) boss.Agent.enabled = false;

        while (boss.transform.position.y < 20)
        {
            boss.transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            yield return null;
        }

        boss.transform.position += boss.transform.forward * 3f;

        yield return new WaitForSeconds(1f);

        while (boss.transform.position.y > 0)
        {
            boss.transform.Translate(Vector3.down * 100 * Time.deltaTime, Space.World);
            yield return null;

        }

        Vector3 landPos = boss.transform.position;
        landPos.y = 0.5f;
        boss.transform.position = landPos;

        if (boss.Agent != null) boss.Agent.enabled = true;

        boss.OnPatternEnd();
    }

}

public class D1_Final_Normal4 : BossPatternBase
{
    public GameObject warning1;
    public GameObject warning2;


    public D1_Final_Normal4(GameObject warning1, GameObject warning2)
    {
        patternName = "Normal4";
        cooldown = 15f;
        weight = 30f;
        range = 8f;

        this.warning1 = warning1;
        this.warning2 = warning2;
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

        yield return new WaitForSeconds(1.5f);

        GameObject.Destroy(gwarning1);

        GameObject gwarning2 = GameObject.Instantiate(warning2, boss.transform.position, boss.transform.rotation); ;

        yield return new WaitForSeconds(1.5f);

        GameObject.Destroy(gwarning2);

        boss.OnPatternEnd();
    }
}

public class D1_Final_Normal5 : BossPatternBase
{
    public D1_Final_Normal5()
    {
        patternName = "Normal5";
        cooldown = 60f;
        weight = 30f;
        range = 50f;
    }
}
