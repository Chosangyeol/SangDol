using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct SpawnData
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float delay;
}

public class EnemySector : MonoBehaviour, ISectorCondition
{
    [Header("스폰 데이터")]
    public List<SpawnData> spawnDataList;
    [SerializeField] private string _goal;

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

    IEnumerator SpawnRoutine(SpawnData data)
    {
        if (data.delay > 0)
            yield return new WaitForSeconds(data.delay);

        if (data.enemyPrefab != null && data.spawnPoint != null)
        {
            // 2. 실제 몬스터 생성
            GameObject instance = Instantiate(data.enemyPrefab, data.spawnPoint.position, data.spawnPoint.rotation);

            // 3. EnemyBase 컴포넌트 가져와서 리스트에 등록 (사망 체크용)
            if (instance.TryGetComponent<EnemyBase>(out var enemy))
            {
                if (enemy.TryGetComponent<EnemyModel>(out var model))
                    model.SetSpawnPoint(data.spawnPoint);

                _spawnedEnemies.Add(enemy);
            }
        }
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal} {DeadEnemyCount} / {TotalEnemyCount}";
    }
}
