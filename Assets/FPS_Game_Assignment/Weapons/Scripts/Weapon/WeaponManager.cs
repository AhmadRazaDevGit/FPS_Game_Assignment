// WeaponManager.cs
using UnityEngine;

/// <summary>
/// Manages equipping, firing and reloading of a set of weapons.
/// Hook UI (WeaponUI) to automatically bind to the active weapon.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Setup")]
    [Tooltip("List of available Weapon components. Assign weapons in the order you want them indexed (0 = first).")]
    [SerializeField] private Weapon[] weapons;    

    [Tooltip("Index of the weapon to equip when the scene starts (0-based).")]
    [SerializeField] private int startWeaponIndex = 0;

    [Header("Optional References")]
    [Tooltip("Optional PlayerMovement reference (useful if the manager needs camera or movement state). Can be left empty.")]
    [SerializeField] private PlayerMovement playerMovement; 

    [Tooltip("Optional UI component that will be bound to the currently equipped weapon (shows ammo, etc.).")]
    [SerializeField] private WeaponUI weaponUI;      

    private int _currentIndex = 0;
    private Weapon _currentWeapon;
    private bool _firingHeld;

    private void Start()
    {
        if (weapons == null || weapons.Length == 0) return;
        // clamp start index to a valid value to avoid misconfiguration errors
        int index = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);
        EquipWeapon(index);
    }

    private void Update()
    {
        // If automatic and the fire button is held, attempt to fire repeatedly.
        if (_firingHeld && _currentWeapon != null && _currentWeapon.TryFire())
        {
            // TryFire handles rate limiting. We call repeatedly from Update to attempt next shot.
            // For performance, TryFire returns quickly if not ready.
        }
    }

    /// <summary>
    /// Equip a weapon by index (0-based). Binds WeaponUI if assigned.
    /// </summary>
    /// <param name="index">Index of the weapon in the 'weapons' array.</param>
    public void EquipWeapon(int index)
    {
        if (weapons == null || index < 0 || index >= weapons.Length) return;

        _currentIndex = index;
        _currentWeapon = weapons[index];

        // Hook UI
        if (weaponUI != null)
        {
            weaponUI.BindWeapon(_currentWeapon);
        }
    }

    /// <summary>
    /// Called by UI Button (tap) or input provider for a single shot.
    /// </summary>
    public void FirePressed()
    {
        if (_currentWeapon == null) return;

        // For tap-button behavior, call TryFire once
        _currentWeapon.TryFire();
    }

    /// <summary>
    /// Call this when fire is held (e.g. PointerDown on mobile). Releasing should call FireHeldStop().
    /// </summary>
    public void FireHeldStart()
    {
        _firingHeld = true;
        // If weapon is semi-auto, still attempt TryFire once right away
        _currentWeapon?.TryFire();
    }

    /// <summary>
    /// Call this when fire hold ends (e.g. PointerUp).
    /// </summary>
    public void FireHeldStop()
    {
        _firingHeld = false;
    }

    /// <summary>
    /// Start reload on the currently equipped weapon.
    /// </summary>
    public void Reload()
    {
        if (_currentWeapon == null) return;
        _currentWeapon.StartReload(this); // pass MonoBehaviour to start coroutine
    }
}
