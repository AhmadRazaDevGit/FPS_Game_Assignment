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
    protected IdleState _idleState;
    protected PatrolState _patrolState;
    protected ChaseState _chaseState;
    protected AttackState _attackState;
    protected HitState _hitState;
    protected virtual void Awake()
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

    protected virtual void OnEnable()
    {
        if (sensor != null)
        {
            sensor.OnDetected += HandleTargetDetected;
        }
    }

    protected virtual void OnDisable()
    {
        if (sensor != null)
        {
            sensor.OnDetected -= HandleTargetDetected;
        }
    }

    protected virtual void Start()
    {
        sensor.ConfigureRadius(enemyData.detectionRadius);
        // start in Idle state
        StateMachine.ChangeState(_idleState);
    }

    protected virtual void Update()
    {
        StateMachine.Tick();
    }

    // IEnemyContext implementation for states to call
    public virtual void SwitchState(IState newState)
    {
        StateMachine.ChangeState(newState);
    }

    protected virtual void InitializeStates()
    {
        _idleState = new IdleState(this);
        _patrolState = new PatrolState(this);
        _chaseState = new ChaseState(this);
        _attackState = new AttackState(this);
        _hitState = new HitState(this);

        // wire transitions (set next states)
        _idleState.SetNextState(_patrolState);
        _patrolState.SetNextState(_idleState);
        _chaseState.SetNextState(_attackState);
        _attackState.SetNextState(_chaseState);
    }

    protected virtual void HandleTargetDetected(Transform target)
    {
        if (target == null) return;

        // Set target on chase state and switch to it
        _chaseState.SetTarget(target);
        _attackState.SetTarget(target);
        SwitchState(_chaseState);
    }

    public virtual void NotifyTargetLost(Transform target)
    {
        HandleTargetLost(target);
    }

    protected virtual void HandleTargetLost(Transform target)
    {
        if (target == null) return;
        _chaseState.ClearTarget();
        _attackState.ClearTarget();
        SwitchState(_idleState);
    }

    public virtual void OnHit()
    {
        SwitchState(_hitState);
    }
    public virtual void Attack()
    {
        _attackState?.DoAttack();
    }

    public void RevertToPreviousState()
    {
        if (StateMachine.PreviousState != null)
        {
            StateMachine.RevertToPrevious();
        }
        else
        {
            StateMachine.ChangeState(_idleState);
        }
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
