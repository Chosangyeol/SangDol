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
    public GameObject portalObject;
    // 🌟 추가: 이 섹터가 끝나면 즉시 바톤을 이어받아 실행될 다음 섹터 컨트롤러
    public SectorController nextSector;

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

        if (portalObject != null) portalObject.SetActive(false);
    }

    public void ActivateSector()
    {
        if (_isActivated || _isCleared) return;
        _isActivated = true;

        if (DungeonManager.instance != null)
        {
            DungeonManager.instance.RegisterActiveSector(this);
        }

        if (isSequential)
        {
            Debug.Log($"[{sectorName}] 순차 모드 : 시작");
            _currentConditionIndex = 0;
            _conditions[_currentConditionIndex].OnConditionStart();
            StartCoroutine(SequentialCheckRoutine());
        }
        else
        {
            Debug.Log($"[{sectorName}] 동시 모드 : 시작");
            foreach (var c in _conditions) c.OnConditionStart();
            StartCoroutine(AllCheckRoutine());
        }

        DungeonManager.instance.UpdateDungeonUI();
    }

    public void ResetSector()
    {
        StopAllCoroutines();

        _isActivated = false;
        _isCleared = false;
        _currentConditionIndex = 0;

        if (portalObject != null) portalObject.SetActive(false);

        foreach (var condition in _conditions)
        {
            if (condition is EnemySector es) es.ResetCondition();
            else if (condition is MiddleBossSector mbs) mbs.ResetCondition();
            else if (condition is FinalBossSector fbs) fbs.ResetCondition();
            // 🌟 추가: 던전 실패/부활 리셋 시 이동 섹터 상태도 안전하게 리셋
            else if (condition is MoveSector ms) ms.ResetCondition();
        }

        Debug.Log($"[{sectorName}] 섹터가 완벽하게 초기화되었습니다.");
    }

    private IEnumerator SequentialCheckRoutine()
    {
        while (!_isCleared)
        {
            if (_conditions[_currentConditionIndex].IsSatisfied)
            {
                _currentConditionIndex++;

                if (_currentConditionIndex >= _conditions.Count)
                {
                    OnSectorCleared();
                    yield break;
                }
                else
                {
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

            foreach (var condition in _conditions)
            {
                if (!condition.IsSatisfied)
                {
                    allSatisfied = false;
                    break;
                }
            }

            if (allSatisfied && _conditions.Count > 0)
            {
                OnSectorCleared();
                yield break;
            }

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
            if (gateObject != null)
                gateObject.SetActive(false);

            FinishSector();
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        cutsceneDirector.Play();

        yield return new WaitUntil(() => cutsceneDirector.state != PlayState.Playing);

        if (navMeshSurface != null)
        {
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            if (gateObject != null) gateObject.SetActive(false);
            Debug.Log("NavMesh 실시간 업데이트 완료 - 다리 연결됨");
        }

        FinishSector();
    }

    private void FinishSector()
    {
        if (nextSectorTrigger != null) nextSectorTrigger.SetActive(true);

        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log($"[{sectorName}] 섹터 클리어 보상: 포탈 활성화!");
        }

        // 🌟 핵심 수정: 다음 섹터가 인스펙터에 연결되어 있다면 즉시 자동 가동시킵니다.
        if (nextSector != null)
        {
            nextSector.ActivateSector();
            Debug.Log($"[{sectorName}] 클리어 -> 다음 섹터 [{nextSector.sectorName}] 자동 활성화 완료!");
        }

        DungeonManager.instance.OnSectorCleared(this);
    }
}