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

    private void Awake()
    {
        
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();

        StateMachine = new StateMachine();

        InitializeStates();
   
    }

    private void Start()
    {
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

        // wire transitions (set next states)
        _idleState.SetNextState(_patrolState);
        _patrolState.SetNextState(_idleState);
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
