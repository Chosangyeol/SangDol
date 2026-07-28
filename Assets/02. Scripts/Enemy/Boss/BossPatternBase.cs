using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossPatternBase
{
    public string patternName;
    public float cooldown;
    public float weight;
    public float range;

    protected float lastUsedTime = -999f;

    public virtual bool IsReady(BossModel boss, Transform player)
    {
        if (Time.time - lastUsedTime < cooldown) return false;
        if (Vector3.Distance(boss.transform.position, boss.Target.transform.position) > range) return false;

        return true;
    }

    public virtual void Execute(BossModel boss)
    {
        lastUsedTime = Time.time;
        Debug.Log($"[보스 패턴 발동] {patternName} (사거리: {range})");

        boss.StartCoroutine(SmoothLookAtRoutine(boss, boss.Target.transform.position, 0.2f));
    }

    protected IEnumerator SmoothLookAtRoutine(BossModel boss, Vector3 targetPos, float duration)
    {
        float time = 0f;
        Quaternion startRot = boss.transform.rotation;

        // 타겟 방향 계산 (Y축 높낮이는 무시하여 땅을 쳐다보거나 하늘을 보지 않게 함)
        Vector3 dir = (targetPos - boss.transform.position).normalized;
        dir.y = 0;

        // 목표로 하는 최종 회전값
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // duration(초) 동안 부드럽게 회전
        while (time < duration)
        {
            time += Time.deltaTime;
            // Slerp: 시작점과 끝점 사이를 구형 보간하여 아주 부드럽게 이어줌
            boss.transform.rotation = Quaternion.Slerp(startRot, targetRot, time / duration);
            yield return null; // 다음 프레임까지 대기
        }

        // 혹시나 오차가 생길 수 있으므로 마지막에 정확한 방향으로 꽂아줌
        boss.transform.rotation = targetRot;
    }
}
