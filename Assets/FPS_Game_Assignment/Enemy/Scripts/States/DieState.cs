public class DieState : IState
{
    private readonly IEnemyContext _context;


    public DieState(IEnemyContext context)
    {
        _context = context;
    }

    public void Enter()
    {

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

    }

    public void Tick()
    {

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
