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

    public void ResetCondition()
    {
        StopAllCoroutines();
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                // 죽어서 들어가는 게 아니므로 이벤트 비우고 Push
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
            // 위치 설정 및 리셋
            enemy.transform.position = data.spawnPoint.position;
            enemy.transform.rotation = data.spawnPoint.rotation;
            enemy.Reset();

            if (enemy is EnemyModel model)
            {
                model.SetSpawnPoint(data.spawnPoint);
                // 던전은 OnReturnToPool에 리스폰 로직을 넣지 않음!
                // 단순히 리스트 관리만 수행
                model.OnReturnToPool = (e) => {
                    // 필요 시 여기서 사망 알림 등만 처리
                    DungeonManager.instance.UpdateDungeonUI();
                };
            }
            _spawnedEnemies.Add(enemy);
            DungeonManager.instance.UpdateDungeonUI();
        }
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal} {DeadEnemyCount} / {TotalEnemyCount}";
    }
}
