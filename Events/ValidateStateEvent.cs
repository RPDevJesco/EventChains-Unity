using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class ValidateStateEvent : IEvent<AIContext>
{
    public string Name => "Validate State";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        // Ensure state is safe and valid
        var state = context.Data.CurrentState;
        
        // Check for state validity
        if (context.Agent.IsStunned && state != AIState.Idle)
        {
            Debug.LogWarning($"[FSM Override] Agent stunned, forcing Idle");
            context.Data.CurrentState = AIState.Idle;
        }
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}