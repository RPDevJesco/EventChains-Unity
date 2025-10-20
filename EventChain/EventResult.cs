using System;

namespace EventChain
{
    public class EventResult<TContext> where TContext : EventContext
    {
        public bool Success { get; set; }
        public TContext Context { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public float Precision { get; set; } = 1.0f;
    
        // Fix the method names to not conflict
        public static EventResult<TContext> SuccessResult(TContext context, string message = "Success")
        {
            return new EventResult<TContext>
            {
                Success = true,
                Context = context,
                Message = message,
                Precision = 1.0f
            };
        }
    
        public static EventResult<TContext> FailureResult(TContext context, string message, Exception ex = null)
        {
            context.Errors.Add(message);
            return new EventResult<TContext>
            {
                Success = false,
                Context = context,
                Message = message,
                Exception = ex,
                Precision = 0.0f
            };
        }
    }
}