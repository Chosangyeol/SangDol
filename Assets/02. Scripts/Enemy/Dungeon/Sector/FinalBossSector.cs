using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalBossSector : MonoBehaviour, ISectorCondition
{
    [Header("스폰 데이터")]
    public List<SpawnData> spawnDataList;
    [SerializeField] private string _goal;

    private List<EnemyBase> _spawnedEnemies = new List<EnemyBase>();

    private bool _isStarted = false;
    public string SectorGoal => _goal;

    public int TotalEnemyCount => spawnDataList.Count;
    public int DeadEnemyCount => _spawnedEnemies.Count(e => e != null && e.IsDead);

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

        EnemyBase enemy = PoolManager.Instance.Pop(data.enemyPrefab.name) as EnemyBase;
        if (enemy != null)
        {
            // 위치 설정 및 리셋
            enemy.transform.position = data.spawnPoint.position;
            enemy.transform.rotation = data.spawnPoint.rotation;
            enemy.Reset();

            if (enemy is BossModel model)
            {
                model.OnReturnToPool = (e) => {
                    // 필요 시 여기서 사망 알림 등만 처리
                    DungeonManager.instance.UpdateDungeonUI();
                };
            }
            _spawnedEnemies.Add(enemy);
            DungeonManager.instance.UpdateDungeonUI();
        }
    }

    public void ResetCondition()
    {
        StopAllCoroutines();

        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                if (enemy is BossModel model)
                {
                    model.ResetBossState();
                    model.OnReturnToPool = null;
                }
            }
        }

        _spawnedEnemies.Clear();
        _isStarted = false;
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal} {DeadEnemyCount} / {TotalEnemyCount}";
    }
}
