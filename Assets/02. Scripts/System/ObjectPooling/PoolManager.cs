using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, Pool<PoolableMono>> _pools = new Dictionary<string, Pool<PoolableMono>>();

    private Transform _trmParent;

    private void Awake()
    {
        Instance = this;

        GameObject parentObj = new GameObject("PoolRoot");
        _trmParent = parentObj.transform;
        _trmParent.SetParent(transform);

    }

    public void CreatePool(PoolableMono prefab, int count = 10)
    {
        Pool<PoolableMono> pool = new Pool<PoolableMono>(prefab, _trmParent, count);
        _pools.Add(prefab.gameObject.name, pool);
    }

    public PoolableMono Pop(string prefabName)
    {
        if (!_pools.ContainsKey(prefabName))
        {
            Debug.LogError($"Prefab does no exist on pool : {prefabName}");
            return null;
        }

        PoolableMono item = _pools[prefabName].Pop();
        Debug.Log(item.name + " 꺼냄");
        item.Reset();
        return item;
    }

    public void Push(PoolableMono obj)
    {
        _pools[obj.name].Push(obj);
    }

    public void ClearPoolingObject()
    {
        var activeObjects = FindObjectsOfType<PoolableMono>();
        foreach(var obj in activeObjects)
        {
            Push(obj);
        }
    }
}