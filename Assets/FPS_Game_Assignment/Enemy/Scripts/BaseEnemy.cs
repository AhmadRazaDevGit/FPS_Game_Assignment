using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IEnemyContext
{
    [Header("Enemy Data")]
    [Tooltip("SO for this enemy (properties speed, animations, idle delay, etc.)")]
    [SerializeField] protected EnemyData enemyData;

    [Header("Waypoints")]
    [Tooltip("Waypoints list used for patrolling (pick random index each patrol).")]
    [SerializeField] protected List<Transform> waypoints;

    [Header("Sensor")]
    [Tooltip("Optional: assign an EnemySensor (child or same object). If left empty, the script will look for one in children.")]
    [SerializeField] protected EnemySensor sensor;

    // Exposed to states via IEnemyContext
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public EnemyData EnemyData => enemyData;
    public Transform Transform { get; private set; }
    public List<Transform> Waypoints => waypoints;

    // State machine & states
    public StateMachine StateMachine { get; private set; }
    private IdleState _idleState;
    private PatrolState _patrolState;
    private ChaseState _chaseState;
    private AttackState _attackState;
    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Transform = transform;

        StateMachine = new StateMachine();

        // If sensor not assigned in inspector, try to find one in children (non-allocating GetComponent in Awake is fine)
        if (sensor == null)
            sensor = GetComponentInChildren<EnemySensor>();
        
        InitializeStates();
    }

    private void OnEnable()
    {
        if (sensor != null)
        {
            sensor.OnDetected += HandleTargetDetected;
        }
    }

    private void OnDisable()
    {
        if (sensor != null)
        {
            sensor.OnDetected -= HandleTargetDetected;
        }
    }

    private void Start()
    {
        sensor.ConfigureRadius(enemyData.detectionRadius);
        // start in Idle state
        StateMachine.ChangeState(_idleState);
    }

    private void Update()
    {
        StateMachine.Tick();
    }

    // IEnemyContext implementation for states to call
    public void SwitchState(IState newState)
    {
        StateMachine.ChangeState(newState);
    }

    private void InitializeStates()
    {
        _idleState = new IdleState(this);
        _patrolState = new PatrolState(this);
        _chaseState = new ChaseState(this);
        _attackState = new AttackState(this);
        // wire transitions (set next states)
        _idleState.SetNextState(_patrolState);
        _patrolState.SetNextState(_idleState);
        _chaseState.SetNextState(_attackState);
        _attackState.SetNextState(_chaseState);
    }

    private void HandleTargetDetected(Transform target)
    {
        if (target == null) return;

        // Set target on chase state and switch to it
        _chaseState.SetTarget(target);
        _attackState.SetTarget(target);
        SwitchState(_chaseState);
    }

    public void NotifyTargetLost(Transform target)
    {
        HandleTargetLost(target);
    }

    private void HandleTargetLost(Transform target)
    {
        if (target == null) return;
        _chaseState.ClearTarget();
        _attackState.ClearTarget();
        SwitchState(_idleState);
    }

#if UNITY_EDITOR
    // editor visualization to easily add waypoints in scene view
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var t in waypoints)
        {
            if (t == null) continue;
            Gizmos.DrawSphere(t.position, 0.15f);
            Gizmos.DrawLine(transform.position, t.position);
        }
    }
#endif
}
