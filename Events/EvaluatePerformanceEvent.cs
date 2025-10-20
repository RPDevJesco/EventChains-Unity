using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class EvaluatePerformanceEvent : IEvent<AIContext>
{
    public string Name => "Evaluate Performance";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var totalThinkTime = context.StrategyThinkTime + 
                             context.OperationalThinkTime + 
                             context.TacticalThinkTime;
        
        var avgConfidence = (context.Data.StrategyConfidence +
                             context.Data.StateTransitionConfidence +
                             context.Data.BehaviorConfidence) / 3f;
        
        context.Data.PerformanceMetrics["TotalThinkTime"] = totalThinkTime;
        context.Data.PerformanceMetrics["AverageConfidence"] = avgConfidence;
        context.Data.PerformanceMetrics["ActionCount"] = context.Data.Actions.Count;
        
        Debug.Log($"[Performance] Think: {totalThinkTime * 1000:F2}ms | Confidence: {avgConfidence:P0} | Actions: {context.Data.Actions.Count}");
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}