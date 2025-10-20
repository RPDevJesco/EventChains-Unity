using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class ValidateStrategyEvent : IEvent<AIContext>
{
    public string Name => "Validate Strategy";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        // Ensure strategy makes sense given current state
        var strategy = context.Data.Strategy;
        var health = context.Data.PerformanceMetrics["HealthPercent"];
        
        // Override if critically wounded
        if (health < 0.15f && strategy != AIStrategy.Retreat)
        {
            Debug.LogWarning($"[Strategy Override] Health critical, forcing Retreat");
            context.Data.Strategy = AIStrategy.Retreat;
            context.Data.StrategyConfidence *= 0.9f;
        }
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}