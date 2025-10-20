using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class GatherIntelligenceEvent : IEvent<AIContext>
{
    public string Name => "Gather Intelligence";
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var startTime = Time.realtimeSinceStartup;
        
        // Gather environmental data
        var nearbyEnemies = context.State.GetNearbyEnemies(context.Agent.Position, 20f);
        var nearbyAllies = context.State.GetNearbyAllies(context.Agent.Position, 15f);
        var healthPercent = context.Agent.Health / context.Agent.MaxHealth;
        var ammoPercent = context.Agent.Ammo / context.Agent.MaxAmmo;
        
        // Store intelligence in context
        context.Data.PerformanceMetrics["EnemyCount"] = nearbyEnemies.Count;
        context.Data.PerformanceMetrics["AllyCount"] = nearbyAllies.Count;
        context.Data.PerformanceMetrics["HealthPercent"] = healthPercent;
        context.Data.PerformanceMetrics["AmmoPercent"] = ammoPercent;
        context.Data.PerformanceMetrics["ThreatLevel"] = CalculateThreatLevel(nearbyEnemies, healthPercent);
        
        context.StrategyThinkTime = Time.realtimeSinceStartup - startTime;
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
    
    private float CalculateThreatLevel(List<AIAgent> enemies, float health)
    {
        if (enemies.Count == 0) return 0f;
        
        float totalThreat = enemies.Sum(e => e.CombatRating);
        float healthModifier = 1f / Mathf.Max(health, 0.1f);
        
        return Mathf.Clamp01(totalThreat * healthModifier / 10f);
    }
}