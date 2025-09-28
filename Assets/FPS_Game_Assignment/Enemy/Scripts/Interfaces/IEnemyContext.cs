using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Minimal surface area for States to interact with the enemy
/// (keeps state dependencies small / testable).
/// </summary>
public interface IEnemyContext
{
    Animator Animator { get; }
    EnemyData EnemyData { get; }
    NavMeshAgent Agent { get; }
    Transform Transform { get; }
    List<Transform> Waypoints { get; }
    void SwitchState(IState newState);

    void NotifyTargetLost(Transform target);

    void RevertToPreviousState();

}
