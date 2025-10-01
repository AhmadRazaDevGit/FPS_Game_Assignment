// WeaponManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple manager that holds weapon prefabs (one per slot) and equips them under WeaponHolder.
/// Designed so new weapon type prefabs are easily added.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Tooltip("Weapon holder transform (child of camera/player).")]
    public Transform weaponHolder;

    [SerializeField] Weapons weapon;

    private GameObject[] weaponPrefabs;

    private List<WeaponBase> _instantiated = new List<WeaponBase>();
    private int _currentIndex = -1;

    public event Action<WeaponBase> OnWeaponEquipped;
    private void Start()
    {
        weaponPrefabs = weapon.weapons;
        // instantiate all weapons but disable them. This lets us switch quickly.
        foreach (var prefab in weaponPrefabs)
        {
            var go = Instantiate(prefab, transform); // initially parent to manager for cleanliness
            var weapon = go.GetComponent<WeaponBase>();
            if (weapon == null)
            {
                Debug.LogWarning("Weapon prefab missing WeaponBase: " + prefab.name);
                Destroy(go);
                continue;
            }
            go.SetActive(false);
            _instantiated.Add(weapon);
        }

        if (_instantiated.Count > 0)
            EquipWeapon(0);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= _instantiated.Count) return;
        if (_currentIndex == index) return;

        if (_currentIndex >= 0)
            _instantiated[_currentIndex].Unequip();

        _currentIndex = index;
        var w = _instantiated[_currentIndex];

        // place at holder; muzzleLocalPosition from data sets local placement if wanted
        Vector3 localPos = w.data != null ? w.data.muzzleLocalPosition : Vector3.zero;
        w.Equip(weaponHolder, localPos, Quaternion.identity);

        OnWeaponEquipped?.Invoke(w);
    }

    public WeaponBase GetCurrentWeapon()
    {
        if (_currentIndex < 0) return null;
        return _instantiated[_currentIndex];
    }
    // call from UI
    public void StartReloadCurrent()
    {
        GetCurrentWeapon()?.StartReload();
    }
    public void SwitchNext()
    {
        if (_instantiated.Count <= 1) return;
        int next = (_currentIndex + 1) % _instantiated.Count;
        EquipWeapon(next);
    }
}
