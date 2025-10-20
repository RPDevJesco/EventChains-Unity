using System.Collections.Generic;

public class AIDecisionData
{
    // Strategic Layer Output
    public AIStrategy Strategy { get; set; }
    public float StrategyConfidence { get; set; }
    
    // Operational Layer Output
    public AIState CurrentState { get; set; }
    public AIState PreviousState { get; set; }
    public float StateTransitionConfidence { get; set; }
    
    // Tactical Layer Output
    public List<AIAction> Actions { get; set; } = new();
    public float BehaviorConfidence { get; set; }
    
    // Learning Data
    public Dictionary<string, float> PerformanceMetrics { get; set; } = new();
}