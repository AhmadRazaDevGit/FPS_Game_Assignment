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
    public float stoppingDistance = 0.5f;

    [Header("Animation")]
    [Tooltip("Animator trigger name for Idle animation")]
    public string idleAnimationName= "Idle";

    [Tooltip("Animator trigger name for Patrol animation")]
    public string patrolAnimationName = "Patrol";
}
