using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    [Header("설정")]
    public GameObject damageTextPrefab; // 생성할 텍스트 프리팹
    public int poolSize = 30;           // 미리 만들어둘 개수

    private Queue<GameObject> textPool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    // 1. 게임 시작 시 미리 텍스트를 만들어 창고(Queue)에 넣어둡니다.
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(damageTextPrefab, transform);
            obj.SetActive(false);
            textPool.Enqueue(obj);
        }
    }

    // 2. 몬스터가 맞았을 때 호출할 함수
    public void SpawnDamageText(Vector3 hitPosition, float damage, bool isCritical, bool isPlayer = false)
    {
        GameObject textObj = null;

        // 창고에 남은 게 있으면 꺼내 쓰고, 부족하면 새로 만듭니다.
        if (textPool.Count > 0)
        {
            textObj = textPool.Dequeue();
        }
        else
        {
            textObj = Instantiate(damageTextPrefab, transform);
        }

        // 약간의 랜덤 오프셋을 주어 숫자끼리 완벽히 겹치는 것을 방지합니다.
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

        textObj.transform.position = hitPosition + randomOffset;
        textObj.SetActive(true);

        int finalDamage = Mathf.RoundToInt(damage);

        // 텍스트 세팅 (아래에서 만들 스크립트)
        textObj.GetComponent<DamageText>().Setup(finalDamage, isCritical,isPlayer);
    }

    // 3. 다 쓴 텍스트를 다시 창고로 반납하는 함수
    public void ReturnToPool(GameObject textObj)
    {
        textObj.SetActive(false);
        textPool.Enqueue(textObj);
    }
}
