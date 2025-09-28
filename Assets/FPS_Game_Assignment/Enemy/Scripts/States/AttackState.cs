using UnityEngine;

public class AttackState : IState
{
    private readonly IEnemyContext _context;
    private Transform _target;
    private IState _nextState; // expected to be ChaseState (or Idle depending on wiring)
    private float _cooldownTimer;

    public string Name => "Attack";

    public AttackState(IEnemyContext context)
    {
        _context = context;
    }

    public void SetNextState(IState next) => _nextState = next;

    // Called by BaseEnemy when target detected / assigned
    public void SetTarget(Transform t) => _target = t;

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

        // Lost target -> fallback (Idle or configured next)
        if (dist > d.chaseLoseDistance)
        {
            ClearTarget();
            _context.SwitchState(_nextState); // BaseEnemy will normally redirect to Idle in HandleTargetLost
            return;
        }

        // If target moved out of attack range, go back to chase
        if (dist > (d.chaseStoppingDistance + 0.3f))
        {
            _context.SwitchState(_nextState); // _nextState should be ChaseState
            return;
        }

        // Face target (optional; smooth rotation could be added)
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
            DoAttack();
            _cooldownTimer = d.attackCooldown;
        }
    }

    private void DoAttack()
    {
        var dmg = _context.EnemyData.attackDamage;

        var health = _target.GetComponentInParent<IHealth>();
        if (health != null)
        {
            health.TakeDamage(dmg);
            return;
        }

        //// Fallback: SendMessage (will not throw if method missing)
        //_target.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);

        // replay attack anim quickly (optional)
        if (_context.Animator != null && !string.IsNullOrEmpty(_context.EnemyData.attackAnimationName))
            _context.Animator.CrossFade(_context.EnemyData.attackAnimationName, 0.05f);
    }

    public void Exit()
    {
    }
}
