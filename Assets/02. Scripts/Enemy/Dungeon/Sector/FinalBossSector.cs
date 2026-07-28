using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
        Debug.Log("보스 섹터 시작");
        _spawnedEnemies.Clear();

        foreach (var data in spawnDataList)
        {
            StartCoroutine(SpawnRoutine(data));
        }
        DungeonManager.instance.UpdateDungeonUI();

    }

    IEnumerator SpawnRoutine(SpawnData data)
    {
        if (data.delay > 0)
            yield return new WaitForSeconds(data.delay);

        PoolableMono enemy = PoolManager.Instance.Pop(data.enemyPrefab.name);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            enemy.transform.position = data.spawnPoint.position;
            enemy.transform.rotation = data.spawnPoint.rotation;
            agent.enabled = true;
        }
        else
        {
            enemy.transform.position = data.spawnPoint.position;
            enemy.transform.rotation = data.spawnPoint.rotation;
        }

        // 3. 위치가 완벽히 세팅된 후 초기화 로직 실행
        enemy.Reset();

        if (enemy is BossModel model)
        {
            model.OnReturnToPool = (e) => {
                // 필요 시 여기서 사망 알림 등만 처리
                DungeonManager.instance.UpdateDungeonUI();
            };
        }
        _spawnedEnemies.Add(enemy.GetComponent<EnemyBase>());
        DungeonManager.instance.UpdateDungeonUI();

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
                    Debug.Log("보스 상태 초기화 완료");
                }

                PoolableMono poolObj = enemy.GetComponent<PoolableMono>();
                if (poolObj != null)
                {
                    PoolManager.Instance.Push(poolObj);
                }
                else
                {
                    Destroy(enemy.gameObject);
                }
            }
        }

        _spawnedEnemies.Clear();
        _isStarted = false;

        // (참고: ResetSector에서 UpdateDungeonUI를 하므로 여기서 뺄 수 있으면 빼도 됩니다)
        DungeonManager.instance.UpdateDungeonUI();
    }

    public string GetProgressString()
    {
        // 로아 스타일: "적 처치 3 / 10"
        return $"{SectorGoal} {DeadEnemyCount} / {TotalEnemyCount}";
    }
}
