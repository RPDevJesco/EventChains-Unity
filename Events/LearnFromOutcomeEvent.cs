using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class LearnFromOutcomeEvent : IEvent<AIContext>
{
    public string Name => "Learn From Outcome";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        // Store decision for future learning
        var decision = new AIDecisionRecord
        {
            Timestamp = Time.time,
            Strategy = context.Data.Strategy,
            State = context.Data.CurrentState,
            ThreatLevel = context.Data.PerformanceMetrics["ThreatLevel"],
            HealthPercent = context.Data.PerformanceMetrics["HealthPercent"],
            Confidence = context.Data.PerformanceMetrics["AverageConfidence"]
        };
        
        context.Agent.DecisionHistory.Add(decision);
        
        // Keep only last 100 decisions
        if (context.Agent.DecisionHistory.Count > 100)
        {
            context.Agent.DecisionHistory.RemoveAt(0);
        }
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}