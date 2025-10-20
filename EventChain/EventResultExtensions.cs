using System;

namespace EventChain
{
    public static class EventResultExtensions
    {
        public static EventResult<TContext> Success<TContext>(this TContext context, string message = "Success") 
            where TContext : EventContext
        {
            return EventResult<TContext>.SuccessResult(context, message);
        }
        
        public static EventResult<TContext> Failure<TContext>(this TContext context, string message, Exception ex = null) 
            where TContext : EventContext
        {
            return EventResult<TContext>.FailureResult(context, message, ex);
        }
    }
}