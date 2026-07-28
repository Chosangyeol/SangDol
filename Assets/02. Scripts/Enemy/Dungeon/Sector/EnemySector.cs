using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EEnemySpawnMotion
{
    None,
    Swing
}

[Serializable]
public struct SpawnData
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public EEnemySpawnMotion motion;
    public float delay;
}

public class EnemySector : MonoBehaviour, ISectorCondition
{
    [Header("스폰 데이터")]
    public List<SpawnData> spawnDataList;
    [SerializeField] private string _goal;

    [Header("그네(스윙) 모션 세팅")]
    public GameObject swingPrefab;      // 연출용 그네/줄 프리팹
    public float swingHeight = 15f;      // 줄의 길이 (높이)
    public float swingStartAngle = -60f; // 시작할 하늘 위 각도 (음수: 뒤쪽 하늘)
    public float swingDuration = 1.0f;   // 내려오는 데 걸리는 시간

    private List<EnemyBase> _spawnedEnemies = new List<EnemyBase>();

    public int TotalEnemyCount => spawnDataList.Count;
    public int DeadEnemyCount => _spawnedEnemies.Count(e => e != null && e.IsDead);

    private bool _isStarted = false;
    public string SectorGoal => _goal;

    public bool IsSatisfied => _isStarted && DeadEnemyCount >= TotalEnemyCount;

    public void OnConditionStart()
    {
        if (_isStarted) return;
        _isStarted = true;

        _spawnedEnemies.Clear();

        foreach (var data in spawnDataList)
        {
            StartCoroutine(SpawnRoutine(data));
        }
    }

    public void ResetCondition()
    {
        StopAllCoroutines();
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                if (enemy is EnemyModel model)
                {
                    model.OnReturnToPool = null;
                    model.StateMachine.ChangeState(new IdleState(model));
                }
                PoolManager.Instance.Push(enemy);
            }
        }
        _spawnedEnemies.Clear();
        _isStarted = false;
        DungeonManager.instance.UpdateDungeonUI();
    }

    IEnumerator SpawnRoutine(SpawnData data)
    {
        if (data.delay > 0)
            yield return new WaitForSeconds(data.delay);

        EnemyBase enemy = PoolManager.Instance.Pop(data.enemyPrefab.name) as EnemyBase;
        if (enemy != null)
        {
            _spawnedEnemies.Add(enemy);
            DungeonManager.instance.UpdateDungeonUI();

            if (data.motion == EEnemySpawnMotion.Swing)
            {
                yield return StartCoroutine(ExecuteSwingMotion(enemy, data));
            }
            else
            {
                enemy.transform.position = data.spawnPoint.position;
                enemy.transform.rotation = data.spawnPoint.rotation;
                enemy.Reset();
            }

            InitializeEnemyModel(enemy, data.spawnPoint);
        }
    }

    // 1단계: 뒤쪽 하늘(-60도)에서 정착 바닥(0도)으로 내려오며 몬스터 배달
    private IEnumerator ExecuteSwingMotion(EnemyBase enemy, SpawnData data)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Vector3 targetLandPos = data.spawnPoint.position;
        Quaternion targetLandRot = data.spawnPoint.rotation;

        Vector3 pivotPos = targetLandPos + Vector3.up * swingHeight;

        // 시작 위치 계산
        Quaternion startRot = Quaternion.AngleAxis(swingStartAngle, data.spawnPoint.right);
        Vector3 startPos = pivotPos + startRot * (-Vector3.up * swingHeight);

        GameObject ropeInstance = null;
        if (swingPrefab != null)
        {
            ropeInstance = GameObject.Instantiate(swingPrefab, startPos, targetLandRot);
        }

        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / swingDuration);
            float acceleratedProgress = progress * progress;

            float currentAngle = Mathf.Lerp(swingStartAngle, 0f, acceleratedProgress);
            Quaternion swingRotation = Quaternion.AngleAxis(currentAngle, data.spawnPoint.right);
            Vector3 currentPos = pivotPos + swingRotation * (-Vector3.up * swingHeight);

            enemy.transform.position = currentPos;
            enemy.transform.rotation = targetLandRot;

            if (ropeInstance != null)
            {
                ropeInstance.transform.position = currentPos;
                ropeInstance.transform.rotation = swingRotation * targetLandRot;
            }

            yield return null;
        }

        // 몬스터 바닥에 강제 안착 및 복구
        enemy.transform.position = targetLandPos;
        enemy.transform.rotation = targetLandRot;
        enemy.Reset();
        if (enemy is EnemyModel model)
            model.isAggressive = true;

        if (agent != null) agent.enabled = true;

        // 🌟 핵심 수정: 되돌아가는 게 아니라, 가던 방향 그대로 전진(+각도)하도록 반대 부호를 넘겨줍니다.
        // 예: -60도에서 시작했으면 복귀 목표점은 +60도입니다.
        if (ropeInstance != null)
        {
            float swingEndAngle = -swingStartAngle;
            StartCoroutine(RetractRopeRoutine(ropeInstance, pivotPos, data.spawnPoint.right, targetLandRot, swingEndAngle));
        }
    }

    // 🌟 2단계 수정: 가던 방향 그대로 앞쪽 하늘(+각도)로 치솟으며 소멸하는 코루틴
    private IEnumerator RetractRopeRoutine(GameObject rope, Vector3 pivotPos, Vector3 rotationAxis, Quaternion targetLandRot, float targetAngle)
    {
        float t = 0f;
        float returnDuration = swingDuration * 0.8f; // 올라갈 때는 날아가듯 기분 좋게 빠르게

        while (t < returnDuration)
        {
            if (rope == null) yield break;

            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / returnDuration);

            // 위로 올라갈수록 중력에 의해 부드럽게 느려지며 정지하는 Ease-Out 효과
            float deceleratedProgress = 1f - (1f - progress) * (1f - progress);

            // 🌟 바닥(0도)에서 앞쪽 하늘(targetAngle, 예: +60도)을 향해 보간 연산
            float currentAngle = Mathf.Lerp(0f, targetAngle, deceleratedProgress);
            Quaternion swingRotation = Quaternion.AngleAxis(currentAngle, rotationAxis);
            Vector3 currentPos = pivotPos + swingRotation * (-Vector3.up * swingHeight);

            if (rope != null)
            {
                rope.transform.position = currentPos;
                // 진행 방향에 맞춰 줄의 기울임 각도도 정방향 유지
                rope.transform.rotation = swingRotation * targetLandRot;
            }

            yield return null;
        }

        // 앞쪽 공중 정점에 도달하면 깔끔하게 파괴
        if (rope != null)
        {
            GameObject.Destroy(rope);
        }
    }

    private void InitializeEnemyModel(EnemyBase enemy, Transform spawnPoint)
    {
        if (enemy is EnemyModel model)
        {
            model.SetSpawnPoint(spawnPoint);
            model.OnReturnToPool = (e) => {
                DungeonManager.instance.UpdateDungeonUI();
            };
        }
        DungeonManager.instance.UpdateDungeonUI();
    }

    public string GetProgressString()
    {
        return $"{SectorGoal} {DeadEnemyCount} / {TotalEnemyCount}";
    }
}