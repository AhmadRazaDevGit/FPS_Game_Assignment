public class DieState : IState
{
    private readonly IEnemyContext _context;
    private bool _entered = false;

    public DieState(IEnemyContext context)
    {
        _context = context;
    }

    public void Enter()
    {
        if (_entered) return; // protect against multiple enters
        _entered = true;

        // stop navmesh movement
        if (_context.Agent != null && _context.Agent.isActiveAndEnabled)
        {
            _context.Agent.isStopped = true;
            _context.Agent.ResetPath();
        }

        if (_context.Animator != null && !string.IsNullOrEmpty(_context.EnemyData.dieAnimationName))
        {
            _context.Animator.CrossFade(_context.EnemyData.dieAnimationName, 0.1f);
        }

        // Request the context to hide the GameObject after configured delay
        float delay = _context.EnemyData != null ? _context.EnemyData.hideAfterDeath : 2f;
        _context.ScheduleHide(delay);
    }

    public void Tick() { }

    public void Exit()
    {

    }
}
