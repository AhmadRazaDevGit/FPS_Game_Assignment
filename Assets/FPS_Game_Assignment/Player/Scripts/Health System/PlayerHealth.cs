using System;
using UnityEngine;

public class PlayerHealth : HealthBase
{
    private PlayerMovement playerMovement;

    private WeaponManager weaponManager;
    private RotationController rotationController;

    public event Action OnPlayerDied;
    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
        weaponManager = GetComponent<WeaponManager>();
        rotationController = GetComponent<RotationController>();
    }
    protected override void OnDeath(GameObject source)
    {
        OnPlayerDied?.Invoke();
        playerMovement.enabled = false;
        weaponManager.enabled = false;
        rotationController.enabled = false;
    }
}
