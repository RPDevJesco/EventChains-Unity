using UnityEngine;

public abstract class BaseState : IState
{
    protected float _stateTime;
    
    public virtual void OnEnter()
    {
        _stateTime = 0f;
    }
    
    public virtual void OnUpdate(AIAgent agent, GameState state)
    {
        _stateTime += Time.deltaTime;
    }
    
    public virtual void OnExit()
    {
        // Cleanup if needed
    }
}