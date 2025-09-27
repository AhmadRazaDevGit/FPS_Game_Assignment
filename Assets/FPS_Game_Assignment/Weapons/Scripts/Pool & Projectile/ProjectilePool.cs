// ProjectilePool.cs
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialSize = 10;

    private Queue<Projectile> _pool = new Queue<Projectile>();

    private void Awake()
    {
        if (projectilePrefab == null) return;
        for (int i = 0; i < initialSize; i++) CreateOne();
    }

    private Projectile CreateOne()
    {
        var go = Instantiate(projectilePrefab, transform);
        var proj = go.GetComponent<Projectile>();
        if (proj == null) proj = go.AddComponent<Projectile>();
        go.SetActive(false);
        _pool.Enqueue(proj);
        return proj;
    }

    public Projectile Get()
    {
        if (_pool.Count == 0) CreateOne();
        var p = _pool.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(Projectile p)
    {
        p.gameObject.SetActive(false);
        p.transform.SetParent(transform, false);
        _pool.Enqueue(p);
    }
}
