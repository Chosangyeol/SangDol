using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpObject : InteractableObject
{
    [Header("목적지 및 궤적")]
    public Transform targetPos;
    public Transform apexPos; // ⭐️ 사용자가 직접 지정할 포물선의 최고점(경유지)
    public AnimationCurve jumpCurve;
    public float jumpDuration = 1f; // 점프에 걸리는 시간

    protected override void Start()
    {
        base.Start();
        Init($"G - 이동");
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;

        UpdateUIState();

        CharacterModel model = target.GetComponent<CharacterModel>();

        model.Navmesh.enabled = false;

        model.canAttack = false;
        model.canMove = false;
        model.canSkill = false;
        model.canUse = false;

        model.transform.LookAt(targetPos);

        StartCoroutine(JumpSequence(model));

        return true;
    }

    IEnumerator JumpSequence(CharacterModel model)
    {
        Vector3 p0 = model.transform.position; // 시작점 (P0)
        Vector3 p1 = apexPos.position;         // 제어점/최고점 (P1)
        Vector3 p2 = targetPos.position;       // 도착점 (P2)

        float time = 0f;
        while (time < jumpDuration)
        {
            time += Time.deltaTime;
            float t = time / jumpDuration;

            // 커브를 통해 점프의 가감속(Easing)을 제어합니다.
            float curveT = jumpCurve.Evaluate(t);

            // ⭐️ 2차 베지어 곡선 (Quadratic Bezier Curve) 공식
            Vector3 position = Mathf.Pow(1 - curveT, 2) * p0 +
                               2 * (1 - curveT) * curveT * p1 +
                               Mathf.Pow(curveT, 2) * p2;

            model.transform.position = position;

            yield return null;
        }

        // 루프가 끝난 후 정확한 도착 지점에 맞춥니다.
        model.transform.position = p2;
        model.EndJump();
    }

#if UNITY_EDITOR
    // 💡 에디터 씬(Scene) 창에서 점프 궤적을 미리 볼 수 있게 선을 그어줍니다.
    private void OnDrawGizmos()
    {
        if (targetPos == null || apexPos == null) return;

        Gizmos.color = Color.green;
        Vector3 p0 = transform.position; // 대략적인 시작 위치 (오브젝트 위치 기준)
        Vector3 p1 = apexPos.position;
        Vector3 p2 = targetPos.position;

        Vector3 prevPos = p0;
        int segments = 20; // 선을 나눌 쪼개기 횟수 (부드러움 정도)

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 nextPos = Mathf.Pow(1 - t, 2) * p0 +
                              2 * (1 - t) * t * p1 +
                              Mathf.Pow(t, 2) * p2;

            Gizmos.DrawLine(prevPos, nextPos);
            prevPos = nextPos;
        }

        // 최고점의 위치를 붉은 구체로 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p1, 0.5f);
    }
#endif
}
