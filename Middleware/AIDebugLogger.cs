using System;
using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class AIDebugLogger : IMiddleware<AIContext>
{
    public static AIDebugLogger Create() => new();
    
    public async Task<EventResult<AIContext>> ExecuteAsync(
        AIContext context,
        Func<Task<EventResult<AIContext>>> next)
    {
        Debug.Log($"[AI System] Starting think cycle for {context.Agent.Name}");
        
        var result = await next();
        
        if (result.Success)
        {
            Debug.Log($"[AI System] Completed with {context.Data.Actions.Count} actions | Strategy: {context.Data.Strategy}");
        }
        else
        {
            Debug.LogError($"[AI System] Failed: {result.Message}");
        }
        
        return result;
    }
}