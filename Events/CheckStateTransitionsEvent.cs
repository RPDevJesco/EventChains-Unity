using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class CheckStateTransitionsEvent : IEvent<AIContext>
{
    public string Name => "Check State Transitions";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var startTime = Time.realtimeSinceStartup;
        
        // Map strategy to appropriate state
        var newState = MapStrategyToState(context.Data.Strategy, context);
        
        if (newState != context.Agent.CurrentState)
        {
            context.Data.PreviousState = context.Agent.CurrentState;
            context.Data.CurrentState = newState;
            Debug.Log($"[FSM] Transition: {context.Data.PreviousState} -> {newState}");
        }
        else
        {
            context.Data.CurrentState = context.Agent.CurrentState;
        }
        
        context.OperationalThinkTime = Time.realtimeSinceStartup - startTime;
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
    
    private AIState MapStrategyToState(AIStrategy strategy, AIContext context)
    {
        return strategy switch
        {
            AIStrategy.Aggressive => AIState.Combat,
            AIStrategy.Defensive => AIState.Investigate,
            AIStrategy.Stealth => AIState.Patrol,
            AIStrategy.Support => AIState.Collaborate,
            AIStrategy.Retreat => AIState.Flee,
            _ => AIState.Idle
        };
    }
}