using System;
using UnityEngine;

public class PoolSpawner : MonoBehaviour
{
    public PoolingListSO skillEffects;

    private void Awake()
    {
        skillEffects.PoolList.ForEach(p =>
        {
            PoolManager.Instance.CreatePool(p.Prefab,p.Count);
        });
        Debug.Log("풀 생성");
    }
}
