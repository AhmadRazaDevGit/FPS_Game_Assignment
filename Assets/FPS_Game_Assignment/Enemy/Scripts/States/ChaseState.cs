using UnityEngine;

public class ChaseState : IState
{
    private readonly IEnemyContext _context;
    private Transform _target;
    private IState _nextState;

    public string Name => "Chase";

    public ChaseState(IEnemyContext context)
    {
        _context = context;
    }

    public void SetNextState(IState next) => _nextState = next;

    /// Called by BaseEnemy when a target is detected
    public void SetTarget(Transform t) => _target = t;

    /// Clear the target (called when lost or on exit)
    public void ClearTarget() => _target = null;

    public void Enter()
    {
        var d = _context.EnemyData;
        if (_context.Animator != null)
        {
            if (!string.IsNullOrEmpty(d.chaseAnimationName))
                _context.Animator.CrossFade(d.chaseAnimationName, 0.2f);
        }

        if (_context.Agent != null && _context.Agent.isActiveAndEnabled)
        {
            _context.Agent.isStopped = false;
            _context.Agent.speed = d.chaseSpeed;
            _context.Agent.stoppingDistance = d.chaseStoppingDistance;
        }
    }


    public void Tick()
    {
        if (_target == null)
        {
            // no target -> fallback
            _context.SwitchState(_nextState);
            return;
        }

        // Move towards target every frame
        if (_context.Agent != null)
        {
            _context.Agent.SetDestination(_target.position);

            // distance checks
            var dist = Vector3.Distance(_context.Transform.position, _target.position);

            // If target gets too far, treat as lost
            if (dist > _context.EnemyData.chaseLoseDistance)
            {
                _context.NotifyTargetLost(_target);
                ClearTarget();
                return;
            }

            // If near enough to stoppingDistance, we can stop/chose next state (e.g., attack or idle)
            if (!_context.Agent.pathPending && dist <= (_context.Agent.stoppingDistance + 0.28f))
            {
                _context.SwitchState(_nextState);
            }
        }
    }

    public void Exit()
    {

    }
}
