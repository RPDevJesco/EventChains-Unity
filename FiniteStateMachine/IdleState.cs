using UnityEngine;

public class IdleState : BaseState
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("[State] Entered IDLE - Waiting for stimulus");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        // Look around slowly
        agent.RotationSpeed = 30f;
        agent.LookAtNearest(state.GetPointsOfInterest());
        
        // Idle animations
        if (_stateTime > 5f)
        {
            agent.PlayAnimation("Idle_Bored");
        }
    }
}