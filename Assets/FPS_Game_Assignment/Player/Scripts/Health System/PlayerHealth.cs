using UnityEngine;
using UnityEngine.AI;

public class PlayerHealth : HealthBase
{
    private PlayerMovement playerMovement;

    private WeaponManager weaponManager;
    private NavMeshAgent navMeshAgent;
    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
        weaponManager = GetComponent<WeaponManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    protected override void OnDeath(GameObject source)
    {
        navMeshAgent.enabled = false;
        playerMovement.enabled = false;
        weaponManager.enabled = false;
    }
}
