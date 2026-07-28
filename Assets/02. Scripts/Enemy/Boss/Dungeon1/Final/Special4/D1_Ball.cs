using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Ball : MonoBehaviour
{
    public float speed;
    public AnimationCurve jumpCurve;

    public void Init()
    {
        StartCoroutine(Roll());
    }


    private IEnumerator Roll()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + Vector3.forward * 8f;

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
            float arc = Mathf.Sin(t * Mathf.PI) * 8f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        float Timer = 0f;

        while (Timer < 10f)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterModel model = other.GetComponent<CharacterModel>();

            if (model != null)
            {
                StartCoroutine(KnockBack(model));
            }
        }
    }

    private IEnumerator KnockBack(CharacterModel model)
    {
        model.PlayerController.StopMove();
        model.canMove = false;

        float timer = 0f;

        Vector3 dir = model.transform.position - transform.position;
        dir.y = 0f;

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;

            Vector3 pushV = dir.normalized * 40f;
            pushV.y = 0f;

            model.Navmesh.velocity = pushV;

            yield return null;
        }

        model.Navmesh.velocity = Vector3.zero;
        model.canMove = true;

    }

}
