using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<string, Pool<PoolableMono>> _pools = new Dictionary<string, Pool<PoolableMono>>();
    private readonly HashSet<string> localPoolNames = new HashSet<string>();

    [Header("글로벌 풀")]
    public PoolingListSO globalPoolList;

    [Header("로컬 풀")]
    public PoolingListSO currentStageList;
    
    private Transform _globalParent;
    private Transform _localParent;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _globalParent = new GameObject("GlobalPool").transform;
            _globalParent.SetParent(transform);

            _localParent = new GameObject("LocalPool").transform;
            _localParent.SetParent(transform);

            InitGlobalPool();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    private void InitGlobalPool()
    {
        if (globalPoolList != null)
        {
            foreach (var item in globalPoolList.PoolList)
            {
                CreatePool(item.Prefab, item.Count,true);
            }
        }
    }

    public void LoadStagePools(PoolingListSO newStagePool)
    {
        ClearStagePools();

        currentStageList = newStagePool;

        if (currentStageList != null)
        {
            foreach (var item in currentStageList.PoolList)
            {
                CreatePool(item.Prefab, item.Count, false);
            }
        }
    }

    public void CreatePool(PoolableMono prefab, int count, bool isGlobal)
    {
        if (prefab == null || _pools.ContainsKey(prefab.name)) return;
        Transform parentTrm = isGlobal ? _globalParent : _localParent ;

        // 기존 작성하신 Pool 클래스 생성자 구조에 맞춰 parentTrm 전달
        Pool<PoolableMono> pool = new Pool<PoolableMono>(prefab, parentTrm, count);
        _pools.Add(prefab.gameObject.name, pool);
        if (!isGlobal) localPoolNames.Add(prefab.name);
    }

    public PoolableMono Pop(string prefabName)
    {
        if (!_pools.TryGetValue(prefabName, out Pool<PoolableMono> pool))
        {
            Debug.LogError($"Prefab does no exist on pool : {prefabName}");
            return null;
        }

        PoolableMono item = pool.Pop();
        if (item != null) item.Reset();
        return item;
    }

    public void Push(PoolableMono obj) => TryPush(obj);
    internal bool TryPush(PoolableMono obj)
    {
        if (obj == null) return false;
        foreach (var pool in _pools.Values)
            if (pool.Owns(obj)) return pool.TryPush(obj);
        return false;
    }
    public void ClearStagePools()
    {
        // Remove ownership before disabling objects: callbacks may return them.
        var oldPools = new List<Pool<PoolableMono>>();
        foreach (var name in localPoolNames)
            if (_pools.TryGetValue(name, out var pool)) { oldPools.Add(pool); _pools.Remove(name); }
        localPoolNames.Clear(); currentStageList = null;
        foreach (var pool in oldPools) pool.Clear();
    }
    private void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        var oldPools = new List<Pool<PoolableMono>>(_pools.Values);
        _pools.Clear(); localPoolNames.Clear();
        foreach (var pool in oldPools) pool.Clear();
    }
    // 씬 내에 활성화된 모든 풀링 객체를 집어넣는 기존 기능
    public void PushAllActiveObjects()
    {
        var activeObjects = FindObjectsOfType<PoolableMono>();
        foreach(var obj in activeObjects)
        {
            Push(obj);
        }
    }
}
