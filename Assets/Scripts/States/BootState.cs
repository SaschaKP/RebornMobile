public class BootState : IState
{
    public void Enter()
    {
        StateManager.GoToState<DownloadState>();
    }

    public void Exit()
    {
        
    }
}