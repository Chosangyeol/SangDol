using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct EnemySpawnPointData
{
    public Transform spanwPos;
    public GameObject enemyPrefab;
    public float respawnTime;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("스포너 세팅")]
    [SerializeField] private List<EnemySpawnPointData> spawnData = new List<EnemySpawnPointData>();
    [SerializeField] private bool isActive = false;

    private List<EnemyModel> spawnedEnemy = new List<EnemyModel>();

    public void ActiveSpawner()
    {
        if (isActive) return;
        isActive = true;

        Debug.Log($"{gameObject.name} 스포너 활성화: 몬스터 생성");

        foreach (var data in spawnData)
        {
            SpawnEnemy(data);
        }
    }

    private void SpawnEnemy(EnemySpawnPointData data)
    {
        if (!isActive) return;

        EnemyModel enemy = PoolManager.Instance.Pop(data.enemyPrefab.name) as EnemyModel;
    
        if (enemy != null)
        {
            enemy.transform.position = data.spanwPos.position;
            enemy.transform.rotation = data.spanwPos.rotation;
            enemy.SetSpawnPoint(data.spanwPos);
            enemy.Reset();

            enemy.SetSpawnPoint(data.spanwPos);

            spawnedEnemy.Add(enemy);

            enemy.OnReturnToPool = (returnedEnemy) => {
                spawnedEnemy.Remove(returnedEnemy);
                if (isActive && returnedEnemy.IsDead)
                {
                    StartCoroutine(RespawnRoutine(data));
                }
            };
        }
    
    }

    private IEnumerator RespawnRoutine(EnemySpawnPointData data)
    {
        yield return new WaitForSeconds(data.respawnTime);
        if (isActive) SpawnEnemy(data);
    }

    public void DeactiveSpawner()
    {
        if (!isActive) return;
        isActive = false;

        Debug.Log($"{gameObject.name} 스포너 비활성화: 몬스터 회수");

        StopAllCoroutines();

        foreach (var enemy in spawnedEnemy)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.OnReturnToPool = null;
                PoolManager.Instance.Push(enemy);
            }
        }

        spawnedEnemy.Clear();
    }
}
