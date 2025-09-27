// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight generic pool for prefabs. Instantiates a few at start and reuses them.
/// Mobile-friendly: avoids allocations at runtime.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Tooltip("Prefab to pool.")]
    public GameObject prefab;
    [Tooltip("Initial instances to create.")]
    public int initialSize = 10;

    private Stack<GameObject> _stack = new Stack<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var go = CreateNew();
            Return(go);
        }
    }

    private GameObject CreateNew()
    {
        var go = Instantiate(prefab, transform);
        go.SetActive(false);
        // Optionally parent to pool object to keep hierarchy tidy
        return go;
    }

    public GameObject Get()
    {
        if (_stack.Count == 0) return CreateNew();
        var go = _stack.Pop();
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform, false);
        _stack.Push(go);
    }
}
