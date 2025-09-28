using UnityEngine;

public class AttackState : IState
{
    private readonly IEnemyContext _context;
    private Transform _target;
    private IState _nextState;
    private float _cooldownTimer;
    private IHealth iHealth;

    public AttackState(IEnemyContext context)
    {
        _context = context;
    }

    public void SetNextState(IState next) => _nextState = next;

    // Called by BaseEnemy when target detected / assigned
    public void SetTarget(Transform t) { _target = t; SetTargetHealth(_target.GetComponent<IHealth>()); }

    public void SetTargetHealth(IHealth health) => iHealth = health;

    public void ClearTarget() => _target = null;

    public void Enter()
    {
        var d = _context.EnemyData;
        _cooldownTimer = 0f;

        if (_context.Animator != null && !string.IsNullOrEmpty(d.attackAnimationName))
            _context.Animator.CrossFade(d.attackAnimationName, 0.1f);

        if (_context.Agent != null)
            _context.Agent.isStopped = true;
    }

    public void Tick()
    {
        if (_target == null)
        {
            _context.SwitchState(_nextState); // fallback
            return;
        }

        var d = _context.EnemyData;
        var dist = Vector3.Distance(_context.Transform.position, _target.position);

        // Lost target -> fallback (Idle)
        if (dist > d.chaseLoseDistance)
        {
            ClearTarget();
            _context.SwitchState(_nextState);
            return;
        }

        // If target moved out of attack range, go back to chase
        if (dist > (d.chaseStoppingDistance + 0.3f))
        {
            _context.SwitchState(_nextState); // _nextState should be ChaseState
            return;
        }

        // Face target
        var dir = (_target.position - _context.Transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            _context.Transform.rotation = Quaternion.Slerp(_context.Transform.rotation,
                Quaternion.LookRotation(dir.normalized),
                Time.deltaTime * 10f);
        }

        // attack on cooldown
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            _context.Animator.Play(d.attackAnimationName, -1, 0f);
            _cooldownTimer = d.attackCooldown;
        }
    }

    public void DoAttack()
    {
        var dmg = _context.EnemyData.attackDamage;
        if (iHealth != null)
        {
            iHealth.TakeDamage(dmg);
            return;
        }

    }

    public void Exit()
    {
    }
}
