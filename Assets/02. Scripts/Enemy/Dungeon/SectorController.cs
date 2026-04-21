using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Playables;

public class SectorController : MonoBehaviour
{
    [Header("섹터 설정")]
    [Tooltip("퀘스트 위 던전 목표 탭에 표시될 중간 목표")]
    public string sectorName;
    public List<GameObject> sectorObjects;
    public bool isSequential = true;

    [Header("섹터 보상 ( 다음 섹터 )")]
    public GameObject gateObject;
    public GameObject nextSectorTrigger;

    [Header("컷씬 연출")]
    public PlayableDirector cutsceneDirector; // 재생할 타임라인
    public bool hasCutscene;
    public NavMeshSurface navMeshSurface;

    private List<ISectorCondition> _conditions = new List<ISectorCondition>();
    public List<ISectorCondition> conditions => _conditions;
    private int _currentConditionIndex = 0;
    public int currentConditionIndex => _currentConditionIndex;
    private bool _isActivated = false;
    private bool _isCleared = false;

    private void Awake()
    {
        navMeshSurface = FindAnyObjectByType<NavMeshSurface>();

        foreach (var obj in sectorObjects)
        {
            if (obj.TryGetComponent<ISectorCondition>(out var condition))
                _conditions.Add(condition);
        }
    }

    public void ActivateSector()
    {
        if (_isActivated || _isCleared) return;
        _isActivated = true;

        if (isSequential)
        {
            // 순차 모드: 첫 번째 조건(전투)만 먼저 시작
            Debug.Log("순차 모드 : 시작");
            _currentConditionIndex = 0;
            _conditions[_currentConditionIndex].OnConditionStart();
            StartCoroutine(SequentialCheckRoutine());
        }
        else
        {
            // 동시 모드: 기존 로직
            Debug.Log("동시 모드 : 시작");
            foreach (var c in _conditions) c.OnConditionStart();
            StartCoroutine(AllCheckRoutine());
        }

        DungeonManager.instance.UpdateDungeonUI();
    }

    private IEnumerator SequentialCheckRoutine()
    {
        while (!_isCleared)
        {
            // 현재 단계의 조건이 만족되었는지 체크
            if (_conditions[_currentConditionIndex].IsSatisfied)
            {
                _currentConditionIndex++;

                // 모든 단계를 다 끝냈다면 클리어
                if (_currentConditionIndex >= _conditions.Count)
                {
                    OnSectorCleared();
                    yield break;
                }
                else
                {
                    // 다음 단계(상호작용) 활성화
                    Debug.Log($"[Sector] 다음 단계 진입: {_currentConditionIndex}");
                    _conditions[_currentConditionIndex].OnConditionStart();
                    DungeonManager.instance.UpdateDungeonUI();
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AllCheckRoutine()
    {
        while (!_isCleared)
        {
            bool allSatisfied = true;

            // 리스트에 있는 모든 조건(ISectorCondition)을 순회하며 체크
            foreach (var condition in _conditions)
            {
                if (!condition.IsSatisfied)
                {
                    allSatisfied = false;
                    break; // 하나라도 만족 못 했다면 더 검사할 필요 없음
                }
            }

            // 모든 조건이 True라면 클리어 처리
            if (allSatisfied && _conditions.Count > 0)
            {
                OnSectorCleared();
                yield break;
            }

            // 최적화를 위해 0.5초마다 체크
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnSectorCleared()
    {
        _isCleared = true;

        if (hasCutscene && cutsceneDirector != null)
        {
            StartCoroutine(PlayCutsceneSequence());
        }
        else
        {
            // 컷씬이 없으면 기존처럼 바로 게이트 제거
            if (gateObject != null)
                gateObject.SetActive(false);

            FinishSector();
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        // 1. 플레이어 입력 제한 (이미 만들어둔 model.canMove 등 활용)
        // 2. 타임라인 재생
        cutsceneDirector.Play();

        // 3. 타임라인이 끝날 때까지 대기
        yield return new WaitUntil(() => cutsceneDirector.state != PlayState.Playing);

        // 4. 지형 변경 확정 (예: 애니메이션이 끝난 위치에서 콜라이더만 끔)
        if (navMeshSurface != null)
        {
            // 업데이트된 지형을 기반으로 파란색 영역을 다시 계산합니다.
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            if (gateObject != null) gateObject.SetActive(false);
            Debug.Log("NavMesh 실시간 업데이트 완료 - 다리 연결됨");
        }

        FinishSector();
    }

    private void FinishSector()
    {
        if (nextSectorTrigger != null) nextSectorTrigger.SetActive(true);
        DungeonManager.instance.OnSectorCleared(this);
    }

    #region Navmesh 재계산 -> CutScene 대응


    #endregion
}
