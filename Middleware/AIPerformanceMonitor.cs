using System;
using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class AIPerformanceMonitor : IMiddleware<AIContext>
{
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
    
    public static AIPerformanceMonitor Create() => new();
    
    public async Task<EventResult<AIContext>> ExecuteAsync(
        AIContext context, 
        Func<Task<EventResult<AIContext>>> next)
    {
        _stopwatch.Restart();
        var result = await next();
        _stopwatch.Stop();
        
        if (_stopwatch.ElapsedMilliseconds > 16) // Longer than one frame at 60fps
        {
            Debug.LogWarning($"[AI Performance] Chain took {_stopwatch.ElapsedMilliseconds}ms (exceeded frame budget)");
        }
        
        return result;
    }
}