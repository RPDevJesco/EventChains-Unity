using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EventChain
{
    public class EventChain<TContext> where TContext : EventContext
        {
            private readonly List<IEvent<TContext>> _events = new();
            private readonly List<IMiddleware<TContext>> _middlewares = new();
            private readonly bool _isLenient;
            
            private EventChain(bool isLenient)
            {
                _isLenient = isLenient;
            }
            
            public static EventChain<TContext> Lenient()
            {
                return new EventChain<TContext>(true);
            }
            
            public static EventChain<TContext> Strict()
            {
                return new EventChain<TContext>(false);
            }
            
            public EventChain<TContext> AddEvent(IEvent<TContext> @event)
            {
                _events.Add(@event);
                return this;
            }
            
            public EventChain<TContext> UseMiddleware(IMiddleware<TContext> middleware)
            {
                _middlewares.Add(middleware);
                return this;
            }
            
            public async Task<ChainResult<TContext>> ExecuteAsync(TContext context)
            {
                var chainResult = new ChainResult<TContext>
                {
                    Context = context,
                    Success = true
                };
                
                var startTime = DateTime.UtcNow;
                var precisionSum = 0f;
                var precisionCount = 0;
                
                try
                {
                    // Build middleware pipeline
                    Func<Task<EventResult<TContext>>> pipeline = async () =>
                    {
                        // Execute all events in sequence
                        foreach (var @event in _events)
                        {
                            var eventStart = DateTime.UtcNow;
                            
                            try
                            {
                                var result = await @event.ExecuteAsync(context);
                                
                                var record = new EventExecutionRecord
                                {
                                    EventName = @event.Name,
                                    Success = result.Success,
                                    Precision = result.Precision,
                                    Duration = DateTime.UtcNow - eventStart,
                                    Message = result.Message
                                };
                                
                                chainResult.Events.Add(record);
                                precisionSum += result.Precision;
                                precisionCount++;
                                
                                if (!result.Success)
                                {
                                    if (_isLenient)
                                    {
                                        Debug.LogWarning($"Event '{@event.Name}' failed (lenient mode): {result.Message}");
                                    }
                                    else
                                    {
                                        chainResult.Success = false;
                                        chainResult.FailureReason = $"Event '{@event.Name}' failed: {result.Message}";
                                        return result;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var record = new EventExecutionRecord
                                {
                                    EventName = @event.Name,
                                    Success = false,
                                    Precision = 0f,
                                    Duration = DateTime.UtcNow - eventStart,
                                    Message = ex.Message
                                };
                                
                                chainResult.Events.Add(record);
                                
                                if (_isLenient)
                                {
                                    Debug.LogError($"Event '{@event.Name}' threw exception (lenient mode): {ex.Message}");
                                }
                                else
                                {
                                    chainResult.Success = false;
                                    chainResult.FailureReason = $"Event '{@event.Name}' threw exception: {ex.Message}";
                                    throw;
                                }
                            }
                        }
                        
                        return EventResult<TContext>.SuccessResult(context);
                    };
                    
                    // Wrap pipeline with middlewares (in reverse order)
                    for (int i = _middlewares.Count - 1; i >= 0; i--)
                    {
                        var middleware = _middlewares[i];
                        var next = pipeline;
                        pipeline = () => middleware.ExecuteAsync(context, next);
                    }
                    
                    // Execute the complete pipeline
                    await pipeline();
                }
                catch (Exception ex)
                {
                    chainResult.Success = false;
                    chainResult.FailureReason = $"Chain execution failed: {ex.Message}";
                    Debug.LogError($"EventChain failed: {ex}");
                }
                
                chainResult.TotalDuration = DateTime.UtcNow - startTime;
                chainResult.Precision = precisionCount > 0 ? precisionSum / precisionCount : 0f;
                
                return chainResult;
            }
        }
}