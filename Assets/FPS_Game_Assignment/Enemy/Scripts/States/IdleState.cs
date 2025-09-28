public class IdleState : IState
{
    private readonly IEnemyContext _context;
    private float _elapsed;
    private IState _nextState; // usually PatrolState

    public string Name => "Idle";

    public IdleState(IEnemyContext context)
    {
        _context = context;
    }

    /// Set the state to transition into after idle delay.
    public void SetNextState(IState next)
    {
        _nextState = next;
    }

    public void Enter()
    {
        _elapsed = 0f;
        var d = _context.EnemyData;

        if (_context.Animator != null)
        {
            if (!string.IsNullOrEmpty(d.idleAnimationName))
                _context.Animator.CrossFade(d.idleAnimationName, 0.5f);
        }

        // Stop agent so it doesn't drift
        if (_context.Agent != null)
        {
            _context.Agent.isStopped = true;
            _context.Agent.ResetPath();
        }
    }

    public void Tick()
    {
        _elapsed += UnityEngine.Time.deltaTime;

        if (_elapsed >= _context.EnemyData.idleDelayTime)
        {
            if (_nextState != null)
            {
                _context.SwitchState(_nextState);
            }

        }
    }

    public void Exit()
    {

    }
}
