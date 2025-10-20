using System.Collections.Generic;

public class AIResult
{
    public AIStrategy Strategy { get; set; }
    public AIState CurrentState { get; set; }
    public List<AIAction> Actions { get; set; }
    
    public float StrategyQuality { get; set; }
    public float StateQuality { get; set; }
    public float BehaviorQuality { get; set; }
    public float OverallQuality { get; set; }
}