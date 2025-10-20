using System.Threading.Tasks;
using EventChain;
using UnityEngine;

public class ExecuteBehaviorTreeEvent : IEvent<AIContext>
{
    private readonly BehaviorTree _behaviorTree;
    
    public string Name => "Execute Behavior Tree";
    
    public ExecuteBehaviorTreeEvent(BehaviorTree tree)
    {
        _behaviorTree = tree;
    }
    
    public async Task<EventResult<AIContext>> ExecuteAsync(AIContext context)
    {
        var startTime = Time.realtimeSinceStartup;
        
        // Behavior tree generates concrete actions
        var actions = await _behaviorTree.Tick(context.Agent, context.State, context.Data.CurrentState);
        
        context.Data.Actions = actions;
        context.Data.BehaviorConfidence = _behaviorTree.LastExecutionConfidence;
        context.TacticalThinkTime = Time.realtimeSinceStartup - startTime;
        
        Debug.Log($"[Behavior] Generated {actions.Count} actions for state {context.Data.CurrentState}");
        
        return EventResult<AIContext>.SuccessResult(context);
    }
}