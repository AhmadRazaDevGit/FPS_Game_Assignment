using UnityEngine;
using UnityEngine.AI;

public class PlayerHealth : HealthBase
{
    private PlayerMovement playerMovement;

    private WeaponManager weaponManager;
    private RotationController rotationController;
    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
        weaponManager = GetComponent<WeaponManager>();
        rotationController = GetComponent<RotationController>();
    }
    protected override void OnDeath(GameObject source)
    {
        playerMovement.enabled = false;
        weaponManager.enabled = false;
        rotationController.enabled = false;
    }
}
