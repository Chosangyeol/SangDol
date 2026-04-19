using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractSector : MonoBehaviour, ISectorCondition
{
    [Header("상호작용 설정")]
    public List<InteractObjectBase> targetObjects;
    [SerializeField] private string _goal;

    [Header("진행 방식")]
    public bool isSequential = false; // true면 순서대로, false면 동시 활성화

    private int _completedCount = 0;
    private int _currentIndex = 0;
    private bool _isCleared = false;

    public bool IsSatisfied => _isCleared;
    public string SectorGoal => _goal;

    public void OnConditionStart()
    {
        if (targetObjects.Count == 0) return;
        _completedCount = 0;
        _currentIndex = 0;

        // 모든 오브젝트 이벤트 구독 및 초기화
        for (int i = 0; i < targetObjects.Count; i++)
        {
            if (targetObjects[i] is InteractObjectBase interactBase)
            {
                int index = i;
                interactBase.OnActivated += () => OnObjectActivated(index);
            }

            // [핵심] 순차 진행이 아니면 시작하자마자 모두 잠금 해제
            if (!isSequential)
            {
                targetObjects[i].SetLock(false);
            }
            else
            {
                targetObjects[i].SetLock(true); // 순차 진행이면 일단 다 잠금
            }
        }

        // 순차 진행일 때만 첫 번째 오브젝트 활성화
        if (isSequential) ActivateNextStep();
    }

    private void OnObjectActivated(int index)
    {
        if (isSequential)
        {
            // 순서대로 진행 중일 때
            if (index == _currentIndex)
            {
                _currentIndex++;
                if (_currentIndex >= targetObjects.Count) _isCleared = true;
                else ActivateNextStep();
            }
        }
        else
        {
            // [동시 진행] 순서 상관없이 완료된 개수만 체크
            _completedCount++;
            if (_completedCount >= targetObjects.Count)
            {
                _isCleared = true;
            }
        }
    }

    private void ActivateNextStep()
    {
        if (_currentIndex < targetObjects.Count)
        {
            targetObjects[_currentIndex].SetLock(false);
        }
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal}";
    }
}