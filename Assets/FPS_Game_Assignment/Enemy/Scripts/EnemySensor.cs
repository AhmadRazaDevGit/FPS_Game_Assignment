using System;
using UnityEngine;

/// <summary>
/// Small trigger-based sensor. Raises events when an object on configured layers enters/exits.
/// Keeps sensing responsibility out of states and enemy logic (SRP).
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemySensor : MonoBehaviour
{
    [Tooltip("Layers considered as valid targets (e.g. Player)")]
    [SerializeField] private LayerMask detectionMask = 0;

    public event Action<Transform> OnDetected;

    private Transform _currentTarget;

    private SphereCollider col;
    private void Awake()
    {
        // ensure collider is a trigger for OnTrigger events
        col = GetComponent<SphereCollider>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }
    public void ConfigureRadius(float radius)
    {
        col.radius = radius;
    }
    private void OnTriggerEnter(Collider other)
    {
        // quick mask check
        if (((1 << other.gameObject.layer) & detectionMask) == 0) return;

        _currentTarget = other.transform;
        OnDetected?.Invoke(_currentTarget);
    }
}
