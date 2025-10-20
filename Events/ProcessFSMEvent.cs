using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class ProcessFSMEvent : IEvent<AIContext>
{
    private readonly FiniteStateMachine _fsm;
    
    public string Name => "Process FSM";
    
    public ProcessFSMEvent(FiniteStateMachine fsm)
    {
        _fsm = fsm;
    }
    
    public Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var startTime = Time.realtimeSinceStartup;
        
        // Execute FSM state logic
        _fsm.TransitionTo(context.Data.CurrentState);
        _fsm.Update(context.Agent, context.State);
        
        context.Data.StateTransitionConfidence = _fsm.LastTransitionConfidence;
        context.OperationalThinkTime += Time.realtimeSinceStartup - startTime;
        
        return Task.FromResult(EventResult<AIContext>.SuccessResult(context));
    }
}