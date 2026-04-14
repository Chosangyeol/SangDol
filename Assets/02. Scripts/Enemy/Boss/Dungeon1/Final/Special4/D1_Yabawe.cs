using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Yabawe : MonoBehaviour
{
    [Header("야바위 설정")]
    public List<D1_Hat> hats;
    public int shuffleCount = 10;
    public float swapDuration = 1.5f;
    public GameObject redChip;
    public GameObject purpleChip;

    private int realIndex = 0;

    public D1_FinalBoss parent;

    private bool isShuffling = false;

    public void StartYabawi(D1_FinalBoss parent)
    {
        this.parent = parent;
        StartCoroutine(YabawiStart());
    }

    private IEnumerator YabawiStart()
    {
        isShuffling = true;

        realIndex = Random.Range(0, hats.Count);
        // D1_Hat에 함수 만들어서 진짜 보여주기
    
        for (int i = 0; i < hats.Count; i++)
        {
            if (i == realIndex)
                hats[i].Init(true, this);
            else
                hats[i].Init(false, this);

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);
        // D1_Hat에 함수 만들어서 정답 가리기

        for (int i = 0; i < hats.Count;i++)
        {
            hats[i].DropHatStart();
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(2.8f);

        for(int i =0; i< shuffleCount; i++)
        {
            int indexA = Random.Range(0, hats.Count);
            int indexB = Random.Range(0, hats.Count);
            while (indexA == indexB)
            {
                indexB = Random.Range(0, hats.Count);
            }

            yield return StartCoroutine(Swap(hats[indexA].gameObject, hats[indexB].gameObject, swapDuration));
            swapDuration -= 0.1f;
        }

        for (int i = 0; i < hats.Count; i++)
            hats[i].SetImmunity(false);

    }

    private IEnumerator Swap(GameObject objA, GameObject objB, float duration)
    {
        Vector3 startPosA = objA.transform.position;
        Vector3 startPosB = objB.transform.position;

        float percent = 0f;

        while (percent < 1f)
        {
            percent += Time.deltaTime / duration;

            // Lerp를 사용해 A는 B의 위치로, B는 A의 위치로 이동시킵니다.
            objA.transform.position = Vector3.Lerp(startPosA, startPosB, percent);
            objB.transform.position = Vector3.Lerp(startPosB, startPosA, percent);

            yield return null;
        }

        objA.transform.position = startPosB;
        objB.transform.position = startPosA;
    }

    public void Success()
    {
        StartCoroutine(SuccessRoutine());

    }

    private IEnumerator SuccessRoutine()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < hats.Count; i++)
        {
            hats[i].spawnChip.transform.position = transform.position;
        }

        while (transform.position.y > -10f)
        {
            transform.position += Vector3.down * 30 * Time.deltaTime;
            yield return null;
        }
        parent.EndSpecialPattern();
    }

    public void Fail()
    {
        StartCoroutine(FailRoutine());
    }

    private IEnumerator FailRoutine()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < hats.Count; i++)
        {
            hats[i].spawnChip.transform.position = transform.position;
        }

        while (transform.position.y > -10f)
        {
            transform.position += Vector3.down * 30 * Time.deltaTime;
            yield return null;
        }
        parent.Target.Damaged(2.0f, true);

        parent.EndSpecialPattern();
    }
}
