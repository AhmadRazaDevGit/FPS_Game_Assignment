using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Delay in seconds the enemy stays idle before starting patrol.")]
    public float idleDelayTime = 1.5f;
    [Tooltip("Movement speed while patrolling (units per second).")]
    public float patrolSpeed = 3.5f;
    [Tooltip("Distance to a waypoint at which the enemy will consider itself 'arrived' while patrolling.")]
    public float patrolStoppingDistance = 0.5f;

    [Header("Chase")]
    [Tooltip("Movement speed while chasing the target (units per second).")]
    public float chaseSpeed = 5f;
    [Tooltip("Distance to the target at which the enemy will stop moving and perform close-range actions (e.g. attack).")]
    public float chaseStoppingDistance = 1.2f;
    [Tooltip("If the target moves farther than this distance, the enemy will give up chasing and return to patrol.")]
    public float chaseLoseDistance = 12f;

    [Header("Sensor")]
    [Tooltip("Radius (in world units) used to detect potential targets (e.g. the player).")]
    public float detectionRadius = 10f;

    [Header("Attack")]
    [Tooltip("Damage applied to the target (optional, requires target to implement IDamageable/IHealth or TakeDamage).")]
    public int attackDamage = 10;
    [Tooltip("Seconds between consecutive attacks.")]
    public float attackCooldown = 1.0f;

    [Header("Hit / Stagger")]
  
    [Tooltip("Seconds to stay in HitState (fallback if animation length unknown).")]
    public float hitRecoveryTime = 0.6f;

    [Header("Animation")]
    [Tooltip("Name of the idle animation/state used by the animator.")]
    public string idleAnimationName = "Idle";
    [Tooltip("Name of the patrol animation/state used by the animator.")]
    public string patrolAnimationName = "Patrol";
    [Tooltip("Name of the chase animation/state used by the animator.")]
    public string chaseAnimationName = "Chase";
    [Tooltip("Name of the attack animation/state used by the animator.")]
    public string attackAnimationName = "Attack";
    [Tooltip("Name of the hit/recover animation state.")]
    public string hitAnimationName = "Hit";
    [Tooltip("Name of the die animation state.")]
    public string dieAnimationName = "Die";
}

