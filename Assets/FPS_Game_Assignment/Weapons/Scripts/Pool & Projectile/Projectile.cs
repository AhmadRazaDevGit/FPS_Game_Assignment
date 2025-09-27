// Projectile.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    private float _speed;
    private float _lifeTime;
    private float _damage;
    private LayerMask _hitMask;
    private float _spawnTime;
    private GameObject _impactPrefab;
    private System.Action<Projectile> _returnToPool;
    private Transform _owner; // optional to ignore collisions with owner

    private void Update()
    {
        float dt = Time.deltaTime;
        Vector3 move = transform.forward * (_speed * dt);

        // SphereCast ahead to detect fast collisions
        if (Physics.SphereCast(transform.position, 0.05f, transform.forward, out RaycastHit hit, move.magnitude + 0.01f, _hitMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
            return;
        }

        transform.position += move;

        if (Time.time - _spawnTime >= _lifeTime)
        {
            Recycle();
        }
    }

    private void OnHit(RaycastHit hit)
    {
        // Apply damage if available
        var hitDamageable = hit.collider.GetComponent<IDamageable>();
        if (hitDamageable != null) hitDamageable.ApplyDamage(_damage, hit);

        // Spawn impact effect (if provided)
        if (_impactPrefab != null)
        {
            var go = Instantiate(_impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Object.Destroy(go, 3f);
        }

        Recycle();
    }

    public void Initialize(float speed, float lifeTime, float damage, LayerMask hitMask, GameObject impactPrefab, Transform owner, System.Action<Projectile> returnToPool)
    {
        _speed = speed;
        _lifeTime = lifeTime;
        _damage = damage;
        _hitMask = hitMask;
        _impactPrefab = impactPrefab;
        _spawnTime = Time.time;
        _owner = owner;
        _returnToPool = returnToPool;
    }

    private void Recycle()
    {
        _returnToPool?.Invoke(this);
    }
}
