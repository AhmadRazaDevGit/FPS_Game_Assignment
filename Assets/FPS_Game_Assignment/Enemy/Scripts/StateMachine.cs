public class StateMachine
{
    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }

    // Change to a new state (no-op if same instance)
    public void ChangeState(IState newState)
    {
        if (newState == CurrentState) return;

        CurrentState?.Exit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState?.Enter();
    }

    // Call from MonoBehaviour.Update()
    public void Tick()
    {
        CurrentState?.Tick();
    }

    /// Convenience: go back to previous state if available
    public void RevertToPrevious()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }
}
