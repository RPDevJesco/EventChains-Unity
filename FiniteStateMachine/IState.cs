public interface IState
{
    void OnEnter();
    void OnUpdate(AIAgent agent, GameState state);
    void OnExit();
}