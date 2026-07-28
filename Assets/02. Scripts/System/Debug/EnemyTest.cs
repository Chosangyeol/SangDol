using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{

    public PoolableMono testEnemyPrefab;


    void Start()
    {
        PoolManager.Instance.LoadStagePools(PoolManager.Instance.currentStageList);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            var enemy = PoolManager.Instance.Pop(testEnemyPrefab.name);
            enemy.transform.position = Vector3.zero;
        }

    }

}
