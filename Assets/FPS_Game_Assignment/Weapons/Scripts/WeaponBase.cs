using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Data-only ScriptableObject; contains numbers and references.")]
    public WeaponData data;

    [Tooltip("Transform used as muzzle spawn point (local to WeaponHolder).")]
    public Transform muzzleTransform;

    [Tooltip("Pool that contains the projectile prefab.")]
    public ObjectPool projectilePool;

    [Header("Assign only for editor/debug")]
    [Tooltip("Layer mask applying to projectile hits.")]
    public LayerMask hitMask;

    // runtime ammo tracking
    protected int currentAmmo;
    protected int spareAmmo;

    protected float _timeBetweenShots;
    protected bool _isFiring;
    protected Coroutine _firingRoutine;
    protected AudioSource _audio;

    // Reloading state & events
    public bool IsReloading { get; private set; } = false;
    /// <summary>Normalized [0..1] reload progress event.</summary>
    public event Action<float> ReloadProgressChanged;
    /// <summary>Fired when reload starts.</summary>
    public event Action ReloadStarted;
    /// <summary>Fired when reload completes or is aborted.</summary>
    public event Action ReloadCompleted;

    protected virtual void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (data != null)
        {
            currentAmmo = data.magazineSize;
            spareAmmo = data.spareAmmo;
            _timeBetweenShots = 60f / data.roundsPerMinute;
            hitMask = data.hitMask;
        }
    }

    // Public wrapper to start reload from UI or code
    public void StartReload()
    {
        // basic guard clauses
        if (IsReloading || data == null) return;
        if (currentAmmo >= data.magazineSize) return; // nothing to reload
        if (spareAmmo <= 0) return; // no spare ammo

        StartCoroutine(ReloadProcess());
    }

    // internal reload coroutine that updates progress and fires events
    private IEnumerator ReloadProcess()
    {
        IsReloading = true;
        ReloadStarted?.Invoke();

        float duration = Mathf.Max(0.001f, data.reloadTime);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            ReloadProgressChanged?.Invoke(Mathf.Clamp01(timer / duration));
            yield return null;
        }

        // Perform the actual ammo refill
        int needed = data.magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, spareAmmo);
        currentAmmo += toLoad;
        spareAmmo -= toLoad;

        ReloadProgressChanged?.Invoke(1f);
        IsReloading = false;
        ReloadCompleted?.Invoke();
    }

    public virtual void Equip(Transform parent, Vector3 localPos, Quaternion localRot)
    {
        transform.SetParent(parent, false);
        transform.localPosition = localPos;
        transform.localRotation = localRot;
        gameObject.SetActive(true);
    }

    public virtual void Unequip()
    {
        gameObject.SetActive(false);
        _isFiring = false;
        if (_firingRoutine != null) StopCoroutine(_firingRoutine);
    }

    public void TryStartFire()
    {
        if (IsReloading) return; // block firing while reloading
        if (_isFiring) return;
        _isFiring = true;
        _firingRoutine = StartCoroutine(FireLoop());
    }

    public void StopFire()
    {
        _isFiring = false;
        if (_firingRoutine != null) { StopCoroutine(_firingRoutine); _firingRoutine = null; }
    }

    protected IEnumerator FireLoop()
    {
        if (data == null) yield break;

        switch (data.fireMode)
        {
            case WeaponData.FireMode.SemiAuto:
                FireOne();
                _isFiring = false;
                break;

            case WeaponData.FireMode.FullAuto:
                while (_isFiring)
                {
                    FireOne();
                    yield return new WaitForSeconds(_timeBetweenShots);
                }
                break;

            case WeaponData.FireMode.Burst:
                while (_isFiring)
                {
                    for (int i = 0; i < data.burstCount; i++)
                    {
                        if (!_isFiring) break;
                        FireOne();
                        yield return new WaitForSeconds(_timeBetweenShots);
                    }
                    _isFiring = false;
                }
                break;
        }
    }

    protected virtual void FireOne()
    {
        if (IsReloading) return; // extra safeguard
        if (currentAmmo <= 0)
        {
            OnEmpty();
            return;
        }

        if (projectilePool == null || data.projectilePrefab == null)
        {
            Debug.LogWarning("Projectile pool or prefab missing on weapon: " + name);
            currentAmmo--;
            return;
        }

        Vector3 dir = CalculateSpreadDirection(muzzleTransform.forward, data.spreadAngle);

        GameObject projGo = projectilePool.Get();
        var proj = projGo.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Init(muzzleTransform.position, dir, projectilePool, data.projectileSpeed, data.projectileLifetime, data.damage, data.hitMask);
        }

        currentAmmo--;
        OnFireEffects();
    }

    protected Vector3 CalculateSpreadDirection(Vector3 forward, float spreadAngleDeg)
    {
        if (spreadAngleDeg <= 0f) return forward;
        float halfAngle = spreadAngleDeg * 0.5f;
        float yaw = UnityEngine.Random.Range(-halfAngle, halfAngle);
        float pitch = UnityEngine.Random.Range(-halfAngle, halfAngle);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        return rot * forward;
    }

    protected virtual void OnFireEffects()
    {
        if (_audio && _audio.clip) _audio.PlayOneShot(_audio.clip);
    }

    protected virtual void OnEmpty()
    {
        // click sound or UI hint
    }

    // quick accessors
    public int CurrentAmmo => currentAmmo;
    public int SpareAmmo => spareAmmo;
}
