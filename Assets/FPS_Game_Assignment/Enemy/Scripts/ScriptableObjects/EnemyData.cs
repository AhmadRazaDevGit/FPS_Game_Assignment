using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]

    [Tooltip("Delay to stop in Idle state at start and after reached waypoint")]
    public float idleDelayTime = 1.5f;

    [Tooltip("Speed of agent while patrolling (NavMeshAgent.speed)")]
    public float patrolSpeed = 3.5f;

    [Tooltip("Stopping distance used to detect arrival at waypoint")]
    public float patrolStoppingDistance = 0.5f;

    [Header("Chase")]
    [Tooltip("Speed of agent while chasing the target")]
    public float chaseSpeed = 5f;

    [Tooltip("If target distance grows beyond this value, target is considered lost")]
    public float chaseLoseDistance = 12f;

    [Header("Sensor")]

    [Tooltip("Radius detection of collider")]
    public float detectionRadius = 10f;

    [Header("Animation")]
    [Tooltip("Animator trigger name for Idle animation")]
    public string idleAnimationName= "Idle";

    [Tooltip("Animator trigger name for Patrol animation")]
    public string patrolAnimationName = "Patrol";

    [Tooltip("Animator trigger name for Chase animation")]
    public string chaseAnimationName = "Chase";
}
