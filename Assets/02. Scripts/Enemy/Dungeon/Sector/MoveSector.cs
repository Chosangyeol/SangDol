using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSector : MonoBehaviour, ISectorCondition
{
    [Header("목표 지점 설정")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string _goal;
    [SerializeField] private bool _isCleared = false;

    // 시각적 피드백을 위한 파티클이나 아웃라인 (선택 사항)
    public GameObject goalVisualEffect;

    public bool IsSatisfied => _isCleared;
    public string SectorGoal => _goal;

    private void Start()
    {
        // 처음에는 목표 지점 연출을 꺼둡니다.
        if (goalVisualEffect != null) goalVisualEffect.SetActive(false);
    }

    public void OnConditionStart()
    {
        _isCleared = false;

        // 섹터가 시작되면 어디로 가야 할지 시각적으로 보여줍니다.
        if (goalVisualEffect != null) goalVisualEffect.SetActive(true);

        Debug.Log("[GoalSector] 목표 지점으로 이동하세요.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 섹터가 활성화된 상태에서 플레이어가 들어왔는지 체크
        // (LayerMask나 Tag를 활용)
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _isCleared = true;
            Debug.Log("[GoalSector] 목표 지점 도달 완료!");

            // 도달 후 연출을 끄거나 변경
            if (goalVisualEffect != null) goalVisualEffect.SetActive(false);
        }
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal}";
    }
}
