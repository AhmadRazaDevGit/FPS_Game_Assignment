using UnityEngine;

public class HitState : IState
{
    private readonly IEnemyContext _context;
    private float _remaining;
    private float _defaultRecovery;
    private bool _isActive;

    public HitState(IEnemyContext context)
    {
        _context = context;
        _defaultRecovery = _context.EnemyData.hitRecoveryTime;
    }

    /// Called when the state is entered. Optionally pass a custom duration.
    public void SetRecovery(float recoverySeconds)
    {
        _remaining = recoverySeconds > 0f ? recoverySeconds : _defaultRecovery;
    }

    public void Enter()
    {
        _isActive = true;

        // stop navmesh movement
        if (_context.Agent != null && _context.Agent.isActiveAndEnabled)
        {
            _context.Agent.isStopped = true;
            _context.Agent.ResetPath();
        }

        // play hit animation (use CrossFade to be safe)
        if (_context.Animator != null && !string.IsNullOrEmpty(_context.EnemyData.hitAnimationName))
        {
            _context.Animator.CrossFade(_context.EnemyData.hitAnimationName, 0.1f);
        }

        // if SetRecovery() wasn't called, use default
        if (_remaining <= 0f) _remaining = _defaultRecovery;
    }

    public void Tick()
    {
        if (!_isActive) return;

        _remaining -= Time.deltaTime;
        if (_remaining <= 0f)
        {
            // recovery finished -> revert to previous state
            _isActive = false;
            _context.RevertToPreviousState();
        }
    }

    public void Exit()
    {
        // resume navmesh movement
        if (_context.Agent != null && _context.Agent.isActiveAndEnabled)
        {
            _context.Agent.isStopped = false;
        }
    }

}
