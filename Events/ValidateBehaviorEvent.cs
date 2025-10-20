using System.Linq;
using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class ValidateBehaviorEvent : IEvent<AIContext>
{
    public string Name => "Validate Behavior";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        // Ensure actions are physically possible
        var validActions = context.Data.Actions
            .Where(a => a.IsValidFor(context.Agent, context.State))
            .ToList();
        
        if (validActions.Count < context.Data.Actions.Count)
        {
            Debug.LogWarning($"[Behavior] Filtered {context.Data.Actions.Count - validActions.Count} invalid actions");
            context.Data.Actions = validActions;
            context.Data.BehaviorConfidence *= 0.85f;
        }
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}