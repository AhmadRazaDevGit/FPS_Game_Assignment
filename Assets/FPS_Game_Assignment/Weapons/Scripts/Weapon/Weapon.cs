using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
{
    [Tooltip("Reference to the WeaponConfig ScriptableObject (damage, fire rate, magazine size, effects, etc.).")]
    public WeaponConfig config;

    [Tooltip("Transform where muzzle flash / projectiles will spawn (usually the weapon's muzzle).")]
    [SerializeField] private Transform muzzleTransform; 

    [Tooltip("Camera used for aiming (usually the player's main camera). Used as ray origin/direction base.")]
    [SerializeField] private Camera playerCamera;       

    [Tooltip("Optional projectile pool. Required if this weapon uses projectiles (isProjectile = true).")]
    [SerializeField] private ProjectilePool projectilePool; 

    [Tooltip("Fallback LayerMask to use for raycasts if WeaponConfig.hitMask is not set.")]
    [SerializeField] private LayerMask worldHitMask;    

    // Events (simple C# events,replace with UnityEvent if needed)
   
    public event Action<int, int> OnAmmoChanged; 
 
    public event Action OnStartReload;
  
    public event Action OnEndReload;

    public event Action<RaycastHit> OnHit; 
  
    public event Action OnFire;

    private int _currentAmmo;
    private int _reserveAmmo;
    private float _lastFireTime = -999f;
    private bool _isReloading;
    private AudioSource _audio;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (config == null) Debug.LogError("Weapon has no WeaponConfig assigned.", this);

        _currentAmmo = config != null ? config.magazineSize : 0;
        _reserveAmmo = config != null ? config.reserveAmmo : 0;
    }

    
    public void StartFire()
    {
        if (config == null) return;
        if (config.isAutomatic) TryFire();
        else TryFire(); // for non-auto, calling StartFire does a single shot; manager or UI should call accordingly
    }

    public void StopFire()
    {
        // for auto-fire  calling TryFire repeatedly from an input system - WeaponManager handles repeated attempts
    }

    
    public bool TryFire()
    {
        if (config == null) return false;
        if (_isReloading) return false;

        float timeBetween = 1f / config.fireRate;
        if (Time.time - _lastFireTime < timeBetween) return false;
        if (_currentAmmo <= 0)
        {
            //trigger empty click sound here
            return false;
        }

        _lastFireTime = Time.time;
        _currentAmmo--;

        // Play audio
        if (_audio != null && config.fireSound != null) _audio.PlayOneShot(config.fireSound);

        // Muzzle flash spawn (simple Instantiate scaled with auto-destroy)
        if (config.muzzleFlashPrefab != null && muzzleTransform != null)
        {
            var fx = Instantiate(config.muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);
            Destroy(fx, 1.0f);
        }

        // Fire logic
        if (config.isProjectile)
            FireProjectile();
        else
            FireRaycast();

        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
        OnFire?.Invoke();

        return true;
    }

    private void FireRaycast()
    {
        // For pellet weapons, perform multiple Raycast in a cone
        for (int i = 0; i < Mathf.Max(1, config.pelletCount); i++)
        {
            Vector3 dir = GetSpreadDirection();

            // choose mask: config.hitMask if set, otherwise worldHitMask
            LayerMask mask = config.hitMask != 0 ? config.hitMask : worldHitMask;

            if (Physics.Raycast(playerCamera.transform.position, dir, out RaycastHit hit, config.range, mask))
            {
                // Apply damage
                var dmg = hit.collider.GetComponent<IDamageable>();
                if (dmg != null) dmg.ApplyDamage(config.damage, hit);

                OnHit?.Invoke(hit);

                // Spawn impact effect
                if (config.impactEffectPrefab != null)
                {
                    var fx = Instantiate(config.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, 3f);
                }
            }
            else
            {
                // Optionally spawn tracer to max distance
                // Ray didn't hit - could spawn a tracer or bullet hole at distance
            }
        }
    }

    private void FireProjectile()
    {
        if (projectilePool == null)
        {
            Debug.LogWarning($"Weapon {name} requires a ProjectilePool assigned for projectile firing.", this);
            return;
        }

        // spawn projectile from pool and initialize
        var proj = projectilePool.Get();
        proj.transform.position = muzzleTransform != null ? muzzleTransform.position : playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        // direction uses spread
        Vector3 dir = GetSpreadDirection();
        proj.transform.rotation = Quaternion.LookRotation(dir);

        // choose mask: config.hitMask if set, otherwise worldHitMask
        LayerMask mask = config.hitMask != 0 ? config.hitMask : worldHitMask;

        proj.Initialize(config.projectileSpeed, config.projectileLifeTime, config.damage, mask, config.impactEffectPrefab, transform, projectilePool.Return);
    }

    private Vector3 GetSpreadDirection()
    {
        // take camera forward and randomize within cone defined by spreadAngle
        var cam = playerCamera.transform;
        if (config.spreadAngle <= 0f)
            return cam.forward;

        // Sample random direction in cone
        float halfAngle = config.spreadAngle;
        // generate a random point on unit sphere cone
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        float angle = UnityEngine.Random.Range(0f, halfAngle);
        // Convert random polar to a rotation offset
        Quaternion rot = Quaternion.AngleAxis(UnityEngine.Random.Range(-halfAngle, halfAngle), cam.up) *
                         Quaternion.AngleAxis(UnityEngine.Random.Range(-halfAngle, halfAngle), cam.right);
        Vector3 dir = rot * cam.forward;
        return dir.normalized;
    }

    public bool CanReload => !_isReloading && _currentAmmo < config.magazineSize && _reserveAmmo > 0;

    public void StartReload(MonoBehaviour coroutineRunner)
    {
        if (!CanReload) return;
        coroutineRunner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        OnStartReload?.Invoke();
        if (_audio != null && config.reloadSound != null) _audio.PlayOneShot(config.reloadSound);

        float start = Time.time;
        while (Time.time - start < config.reloadTime)
        {
            // could provide reload progress events here
            yield return null;
        }
        // compute transferred ammo
        int needed = config.magazineSize - _currentAmmo;
        int toLoad = Mathf.Min(needed, _reserveAmmo);
        _currentAmmo += toLoad;
        _reserveAmmo -= toLoad;

        _isReloading = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(_currentAmmo, _reserveAmmo);
    }

    // Utility getters
    public int CurrentAmmo => _currentAmmo;
    public int ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _isReloading;
}
