// WeaponBase.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Data-only ScriptableObject; contains numbers and references.")]
    public WeaponData data;

    [Tooltip("Transform used as muzzle spawn point (local to WeaponHolder).")]
    public Transform muzzleTransform; // can be assigned to empty at WeaponHolder

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
                // Single shot per input press - handled externally by StartFire call
                FireOne();
                _isFiring = false; // stop after one
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
                    // after burst wait for full trigger reset (optional). Here we break loop so player must release/re-press.
                    _isFiring = false;
                }
                break;
        }
    }

    protected virtual void FireOne()
    {
        if (currentAmmo <= 0)
        {
            OnEmpty();
            return;
        }

        // spawn projectile from pool
        if (projectilePool == null || data.projectilePrefab == null)
        {
            Debug.LogWarning("Projectile pool or prefab missing on weapon: " + name);
            currentAmmo--;
            return;
        }

        // calculate direction with spread
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
        // random direction inside cone
        float halfAngle = spreadAngleDeg * 0.5f;
        float yaw = Random.Range(-halfAngle, halfAngle);
        float pitch = Random.Range(-halfAngle, halfAngle);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        return rot * forward;
    }

    protected virtual void OnFireEffects()
    {
        // play sound, muzzle flash, recoil stubs (implement in derived classes or add simple audio clip)
        if (_audio && _audio.clip) _audio.PlayOneShot(_audio.clip);
    }

    protected virtual void OnEmpty()
    {
        // click sound or UI hint
        // could play empty click sound by _audio
    }

    public virtual IEnumerator Reload()
    {
        if (currentAmmo >= data.magazineSize || spareAmmo <= 0) yield break;
        // simple reload
        yield return new WaitForSeconds(data.reloadTime);

        int needed = data.magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, spareAmmo);
        currentAmmo += toLoad;
        spareAmmo -= toLoad;
    }

    // Helper accessors
    public int CurrentAmmo => currentAmmo;
    public int SpareAmmo => spareAmmo;
}
