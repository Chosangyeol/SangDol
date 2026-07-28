using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRay : MonoBehaviour
{
    public LayerMask enemyLayer;
    public float maxRayDistance = 50f;

    private EnemyBase _lastHoveredEnemy = null;

    private void Update()
    {
        HandleMouseHover();
    }

    private void HandleMouseHover()
    {
        // 1. 마우스 위치로부터 3D 공간으로 레이저(Ray)를 생성
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 2. 레이저에 몬스터가 맞았는지 검사
        if (Physics.Raycast(ray, out hit, maxRayDistance, enemyLayer))
        {
            // 맞은 오브젝트에서 EnemyModel(또는 최상위 부모)을 가져옵니다.
            EnemyModel currentEnemy = hit.collider.GetComponentInParent<EnemyModel>();

            if (currentEnemy != null && !currentEnemy.IsDead)
            {
                // 마우스가 가리키는 몬스터가 새로 바뀐 경우
                if (_lastHoveredEnemy != currentEnemy)
                {
                    // 이전 몬스터의 외곽선은 끄고
                    if (_lastHoveredEnemy != null) _lastHoveredEnemy.ToggleOutline(false);

                    // 새로운 몬스터의 외곽선은 켭니다
                    _lastHoveredEnemy = currentEnemy;
                    _lastHoveredEnemy.ToggleOutline(true);
                }
                return; // 함수 종료
            }
        }

        // 3. 아무것도 맞지 않았거나 플레이어가 죽은 시체 위에 있다면 기존 외곽선 제거
        if (_lastHoveredEnemy != null)
        {
            _lastHoveredEnemy.ToggleOutline(false);
            _lastHoveredEnemy = null;
        }
    }
}
