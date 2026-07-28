using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniClown : EnemyModel
{
    [Header("오버랩 박스 가로/세로 두께 세팅")]
    public float boxWidthHalf = 1.2f;  // 박스 가로 반폭 (실제 가로 크기는 x2인 2.4m)
    public float boxHeightHalf = 1.0f; // 박스 세로 반고 (실제 높이 크기는 x2인 2.0m)
    public float centerOffsetY = 1.0f; // 몬스터 발밑 기준 높이 보정

    public override void Attack()
    {
        base.Attack();

        // 플레이어가 없거나 이미 죽었다면 판정 스킵
        if (Target == null || Target.isDie) return;

        float currentAttackRange = statSO.attackRange;
        float currentDamage = statSO.attackDamage;

        // 🌟 1. 오버랩 박스의 중심점(Center) 계산
        // 몬스터 위치에서 정면(forward)으로 사거리의 절반만큼 밀어주고, Y축 높이를 보정합니다.
        Vector3 center = transform.position + (transform.forward * (currentAttackRange * 0.5f));
        center.y += centerOffsetY;

        // 🌟 2. 오버랩 박스의 실제 크기의 절반(HalfExtents) 계산
        // Z축 절반 크기를 사거리의 절반으로 설정하여 정확히 사거리 길이만큼 박스가 형성됩니다.
        Vector3 halfExtents = new Vector3(boxWidthHalf, boxHeightHalf, currentAttackRange * 0.5f);

        // 🌟 3. 회전값을 포함하여 영역 내 모든 콜라이더 검출
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation);

        bool isHitSuccess = false;

        foreach (var hit in hitColliders)
        {
            // 충돌한 오브젝트나 그 부모에게 플레이어(CharacterModel) 컴포넌트가 있고, 그게 현재 타겟인지 검사
            CharacterModel player = hit.transform.GetComponentInParent<CharacterModel>();

            if (player != null && player == Target)
            {
                // 플레이어 피격 처리
                Target.Damaged(currentDamage, false);
                isHitSuccess = true;

                Debug.Log($"<color=red>[OverlapBox 적중] {statSO.enemyName} -> 플레이어 대미지: {currentDamage}</color>");
                break; // 플레이어를 맞췄으므로 뒤쪽 루프 검사는 생략하고 탈출
            }
        }

        if (!isHitSuccess)
        {
            Debug.Log("<color=gray>[OverlapBox 불발] 사각형 범위 내에 플레이어가 없습니다.</color>");
        }
    }
}