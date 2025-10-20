using System.Threading.Tasks;

namespace EventChain
{
    public interface IEvent<TContext> where TContext : EventContext
    {
        string Name { get; }
        Task<EventResult<TContext>> ExecuteAsync(TContext context);
    }
}