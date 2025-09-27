// Projectile.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Tooltip("Speed in units/second.")]
    public float speed = 60f;
    [Tooltip("Seconds before auto-returning to pool.")]
    public float lifetime = 3f;
    [Tooltip("Damage dealt on hit.")]
    public float damage = 20f;
    [Tooltip("Layers we can hit.")]
    public LayerMask hitMask = ~0;

    private float _expiration;
    private Vector3 _direction;
    private ObjectPool _ownerPool;

    private void OnEnable()
    {
        _expiration = Time.time + lifetime;
    }

    private void Update()
    {
        // Simple manual movement (cheap, avoids Rigidbody overhead).
        float step = speed * Time.deltaTime;
        Vector3 next = transform.position + _direction * step;

        // Perform a physics check along movement to prevent tunneling
        if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, step + 0.01f, hitMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
        }
        else
        {
            transform.position = next;
        }

        if (Time.time >= _expiration)
            ReturnToPool();
    }

    private void OnHit(RaycastHit hit)
    {
        // Try to apply damage
        var damageable = hit.collider.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage, hit.point, hit.normal);

        // Add impact effects here (decals, particles) - keep simple for mobile

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_ownerPool != null) _ownerPool.Return(gameObject);
        else Destroy(gameObject); // fallback
    }

    /// <summary>
    /// Initializes the projectile and gives it direction and owning pool.
    /// </summary>
    public void Init(Vector3 position, Vector3 direction, ObjectPool pool, float speedOverride, float lifetimeOverride, float damageOverride, LayerMask mask)
    {
        transform.position = position;
        transform.forward = direction;
        _direction = direction.normalized;
        _ownerPool = pool;
        speed = speedOverride;
        lifetime = lifetimeOverride;
        damage = damageOverride;
        hitMask = mask;
        _expiration = Time.time + lifetime;
        gameObject.SetActive(true);
    }
}
