using UnityEngine;

public class PlayerHealth : HealthBase
{
    private PlayerMovement playerMovement;

    private WeaponManager weaponManager;
    private RotationController rotationController;

    [Tooltip("Raise when player died to active the game (SO to decouple")]
    [SerializeField] GameEventWithBool OnPlayerDied;
    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
        weaponManager = GetComponent<WeaponManager>();
        rotationController = GetComponent<RotationController>();
    }
    protected override void OnDeath(GameObject source)
    {
        OnPlayerDied?.Raise(false);
        playerMovement.enabled = false;
        weaponManager.enabled = false;
        rotationController.enabled = false;
    }
}
