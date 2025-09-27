// WeaponConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/WeaponConfig", fileName = "WeaponConfig")]
public class WeaponConfig : ScriptableObject
{
    public string weaponName = "Weapon";

    [Header("General")]
    [Tooltip("true => spawn projectile, false => raycast hits")]
    public bool isProjectile = false;

    [Tooltip("hold to keep firing")]
    public bool isAutomatic = false;

    [Tooltip("shots per second")]
    public float fireRate = 5f;


    public int magazineSize = 12;

    [Tooltip("ammo in reserve (for reload)")]
    public int reserveAmmo = 48;

    [Tooltip("seconds to reload")]
    public float reloadTime = 1.6f;

    [Header("Damage & Range")]
    public float damage = 20f;
    public float range = 100f;

    [Header("Spread & Pellets")]
    [Tooltip("Degrees of cone half-angle")]
    public float spreadAngle = 1.5f;
    [Tooltip("For shotgun: number of pellets per shot (1 = single bullet)")]
    public int pelletCount = 1;

    [Header("Projectile (if projectile)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 40f;
    public float projectileLifeTime = 8f;

    [Tooltip("used for collision spherecast by projectile")]
    public float projectileRadius = 0.2f;  

    [Header("Effects (data-only references)")]
    public GameObject muzzleFlashPrefab;
    public GameObject impactEffectPrefab;
    public AudioClip fireSound;
    public AudioClip reloadSound;

    [Header("Misc")]

    [Tooltip("what layers you can hit")]
    public LayerMask hitMask = ~0;           
}
