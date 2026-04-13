using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class D1_Rook : MonoBehaviour, ICounterable
{
    private bool canCounter = false;
    public bool CanCounter => canCounter;
    public float speed;
    public bool isCounterable = false;

    public bool isLast = false;

    public bool isRushing = false;

    public bool hasAttack = false;

    public void Init(bool iscounterable, bool isLast = false)
    {
        isCounterable = iscounterable;
        this.isLast = isLast;

        if (isCounterable)
            EnableCounter();

        RushStart();
    }

    public void RushStart()
    {
        StartCoroutine(Rush());
    }

    IEnumerator Rush()
    {
        while (transform.position.y > 0)
        {
            transform.position += Vector3.down * 40f * Time.deltaTime;
            yield return null;
        }

        Vector3 pos = transform.position;
        pos.y = 0;

        transform.position = pos;

        if (!isLast)
            yield return new WaitForSeconds(2f);
        else
            yield return new WaitForSeconds(2.5f);

        DisableCounter();

        isRushing = true;

        while (transform.localPosition.z > -7)
        {
            transform.position += new Vector3(0, 0, -1f) * speed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    public void EnableCounter()
    {
        canCounter = true;
    }

    public void DisableCounter()
    {
        canCounter = false;
        this.GetComponentInChildren<Renderer>().material.color = Color.red;
    }

    public void OnCounterSuccess(SDamageInfo info)
    {
        // 1. 몬스터가 카운터 가능한 상태가 아니거나, 공격이 카운터 속성이 아니면 즉시 취소!
        if (!canCounter || !info.isCounterable)
        {
            return;
        }

        // 2. 공격 방향이 헤드(1)가 아니면 즉시 취소!
        if (CheckAttackDir(info) != 1)
        {
            return;
        }

        // --- 여기까지 무사히 넘어왔다면 진짜 카운터 성공 ---
        Debug.Log("카운터 성공");
        canCounter = false;
        StopAllCoroutines();
        StartCoroutine(Counter(1));
    }

    public IEnumerator Counter(float duration)
    {

        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
    }

    public int CheckAttackDir(SDamageInfo info)
    {
        Vector3 dir = (info.source.transform.position - transform.position).normalized;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !hasAttack)
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
        hasAttack = true;
        model.PlayerController.StopMove();
        model.canMove = false;

        float timer = 0f;

        Vector3 dir = model.transform.position - transform.position;
        dir.y = 0f;

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;

            Vector3 pushV = dir.normalized * 25f;
            pushV.y = 0f;

            model.Navmesh.velocity = pushV;

            yield return null;
        }

        model.Navmesh.velocity = Vector3.zero;
        model.canMove = true;

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
