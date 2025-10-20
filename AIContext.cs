using EventChain;

public class AIContext : EventContext
{
    public AIAgent Agent { get; set; }
    public GameState State { get; set; }
    public AIDecisionData Data { get; set; } = new();
    
    // Timing for each layer
    public float StrategyThinkTime { get; set; }
    public float OperationalThinkTime { get; set; }
    public float TacticalThinkTime { get; set; }
}