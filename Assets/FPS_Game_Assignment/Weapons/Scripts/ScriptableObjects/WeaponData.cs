using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData_", menuName = "Weapon/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject
{
    public enum FireMode { SemiAuto, FullAuto}


    [Header("Identity")]
    [Tooltip("Human readable name.")]
    public string weaponName = "Pistol";

    [Header("Ammo")]
    [Tooltip("Rounds in a full magazine.")]
    public int magazineSize = 12;
    [Tooltip("Total spare ammo that the player can carry (not including magazine).")]
    public int spareAmmo = 36;
    [Tooltip("Reload time in seconds.")]
    public float reloadTime = 1.2f;

    [Header("Fire")]
    [Tooltip("Semi-auto, full-auto, or burst.")]
    public FireMode fireMode = FireMode.SemiAuto;
    [Tooltip("Rounds per minute / fire rate.")]
    public float roundsPerMinute = 400f;
    [Tooltip("Spread in degrees (cone half-angle).")]
    public float spreadAngle = 1.2f;

    [Header("Projectile")]
    [Tooltip("Prefab of projectile to spawn (should be pooled).")]
    public GameObject projectilePrefab;
    [Tooltip("Muzzle spawn local position relative to WeaponHolder or muzzleTransform.")]
    public Vector3 muzzleLocalPosition = Vector3.zero;
    [Tooltip("Projectile speed in units/sec.")]
    public float projectileSpeed = 60f;
    [Tooltip("Projectile lifetime in seconds.")]
    public float projectileLifetime = 3f;
    [Tooltip("Damage per projectile.")]
    public float damage = 20f;

    [Header("Misc")]
    [Tooltip("Layer mask for what projectiles can hit.")]
    public LayerMask hitMask = ~0;

    [Header("SFX")]
    [Tooltip("Fire sound clip")]
    public AudioClip fireClip;
    [Tooltip("Reload sound clip")]
    public AudioClip realodClip;
    [Tooltip("Empty magzine sound clip")]
    public AudioClip emptyClip;
}
