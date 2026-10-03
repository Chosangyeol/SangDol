using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : PoolableMono
{
    private readonly Stack<T> _pool = new Stack<T>();
    private readonly HashSet<T> owned = new HashSet<T>();
    private readonly HashSet<T> available = new HashSet<T>();
    private readonly T _prefab;
    private readonly Transform _parent;
    private bool closed;

    public Pool(T prefab, Transform parent, int count = 10)
    {
        _prefab = prefab; _parent = parent;
        for (int i = 0; i < count; i++) Push(Create());
    }
    private T Create()
    {
        var obj = Object.Instantiate(_prefab, _parent);
        obj.name = _prefab.name;
        owned.Add(obj);
        return obj;
    }
    public T Pop()
    {
        if (closed) return null;
        T obj = null;
        while (_pool.Count > 0 && obj == null)
        {
            obj = _pool.Pop(); available.Remove(obj);
            if (obj == null) owned.Remove(obj);
        }
        if (obj == null) obj = Create();
        obj.gameObject.SetActive(true);
        return obj;
    }
    internal bool Owns(T obj) => !closed && obj != null && owned.Contains(obj);
    internal bool TryPush(T obj)
    {
        if (!Owns(obj) || !available.Add(obj)) return false;
        // Register first: OnDisable may return the same object again.
        _pool.Push(obj);
        obj.transform.SetParent(_parent, true);
        obj.gameObject.SetActive(false);
        return true;
    }
    public void Push(T obj) => TryPush(obj);
    public void Clear()
    {
        if (closed) return;
        closed = true;
        var snapshot = new List<T>(owned);
        _pool.Clear(); available.Clear(); owned.Clear();
        foreach (var obj in snapshot)
        {
            if (obj == null) continue;
            obj.gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(obj.gameObject);
            else Object.DestroyImmediate(obj.gameObject);
        }
    }
}