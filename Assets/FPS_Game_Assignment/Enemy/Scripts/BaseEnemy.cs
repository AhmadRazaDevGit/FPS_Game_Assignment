using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IEnemyContext, IWayPointAssignable
{
    [Header("Enemy Data")]
    [Tooltip("SO for this enemy (properties speed, animations, idle delay, etc.)")]
    [SerializeField] protected EnemyData enemyData;

    [Header("Waypoints")]
    [Tooltip("Waypoints list used for patrolling (pick random index each patrol).")]
    [SerializeField] protected Transform[] waypoints;

    [Header("Sensor")]
    [Tooltip("Optional: assign an EnemySensor (child or same object). If left empty, the script will look for one in children.")]
    [SerializeField] protected EnemySensor sensor;

    [Header("Health")]
    [Tooltip("Optional: assign an health (child or same object). If left empty, the script will look for one in component.")]
    [SerializeField] protected EnemyHealth enemyHealth;
    // Exposed to states via IEnemyContext
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public EnemyData EnemyData => enemyData;
    public Transform Transform { get; private set; }
    public Transform[] Waypoints { get => waypoints; set => waypoints = value; }


    // State machine & states
    public StateMachine StateMachine { get; private set; }
    protected IdleState _idleState;
    protected PatrolState _patrolState;
    protected ChaseState _chaseState;
    protected AttackState _attackState;
    protected HitState _hitState;
    protected DieState _dieState;
    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Transform = transform;
        StateMachine = new StateMachine();


        if (sensor == null)
            sensor = GetComponentInChildren<EnemySensor>();
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        InitializeStates();
    }
    public void AssignWayPoints(Transform[] wayPoints)
    {
        if (wayPoints != null) { waypoints = wayPoints; }
        else { Debug.LogWarning("No waypoints found Ai will not patrol"); }
    }
    protected virtual void OnEnable()
    {
        if (sensor != null)
        {
            sensor.OnDetected += HandleTargetDetected;
            enemyHealth.OnEnemyDeath += GoToDieState;
        }
    }

    protected virtual void OnDisable()
    {
        if (sensor != null)
        {
            sensor.OnDetected -= HandleTargetDetected;
            enemyHealth.OnEnemyDeath -= GoToDieState;
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
        _dieState = new DieState(this);

        SetNextState();
    }

    protected virtual void SetNextState()
    {
        // wire transitions (set next states)
        _idleState.SetNextState(_patrolState);
        _patrolState.SetNextState(_idleState);
        _chaseState.SetNextState(_attackState);
        _attackState.SetNextState(_chaseState);
    }

    private void GoToDieState()
    {
        SwitchState(_dieState);
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
    public void ScheduleHide(float seconds) { Agent.enabled = false; Invoke(nameof(Hide), seconds); }

    protected void Hide()
    {
        gameObject.SetActive(false);
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
