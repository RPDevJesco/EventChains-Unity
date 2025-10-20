using System;
using System.Threading.Tasks;

namespace EventChain
{
    public interface IMiddleware<TContext> where TContext : EventContext
    {
        Task<EventResult<TContext>> ExecuteAsync(
            TContext context, 
            Func<Task<EventResult<TContext>>> next
        );
    }
}