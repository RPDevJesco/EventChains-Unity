using System.Collections.Generic;
using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class DetermineStrategyEvent : IEvent<AIContext>
{
    public string Name => "Determine Strategy";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var metrics = context.Data.PerformanceMetrics;
        
        float threatLevel = metrics.GetValueOrDefault("ThreatLevel", 0f);
        float healthPercent = metrics.GetValueOrDefault("HealthPercent", 1f);
        float enemyCount = metrics.GetValueOrDefault("EnemyCount", 0f);
        float allyCount = metrics.GetValueOrDefault("AllyCount", 0f);
        
        // Decide strategy based on situation
        AIStrategy strategy;
        float confidence;
        
        if (healthPercent < 0.25f)
        {
            // Critical health - retreat!
            strategy = AIStrategy.Retreat;
            confidence = 0.95f;
        }
        else if (enemyCount == 0)
        {
            // No enemies - patrol
            strategy = AIStrategy.Stealth;
            confidence = 0.8f;
        }
        else if (allyCount > 0 && healthPercent < 0.6f)
        {
            // Wounded with allies nearby - seek support
            strategy = AIStrategy.Support;
            confidence = 0.85f;
        }
        else if (enemyCount > allyCount + 2)
        {
            // Heavily outnumbered - defensive
            strategy = AIStrategy.Defensive;
            confidence = 0.9f;
        }
        else if (threatLevel > 0.7f)
        {
            // High threat - defensive
            strategy = AIStrategy.Defensive;
            confidence = 0.75f;
        }
        else
        {
            // Normal situation - aggressive
            strategy = AIStrategy.Aggressive;
            confidence = 0.7f;
        }
        
        context.Data.Strategy = strategy;
        context.Data.StrategyConfidence = confidence;
        
        Debug.Log($"[Strategy] Determined: {strategy} (Confidence: {confidence:P0})");
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}