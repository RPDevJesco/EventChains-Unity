public class AIDecisionRecord
{
    public float Timestamp { get; set; }
    public AIStrategy Strategy { get; set; }
    public AIState State { get; set; }
    public float ThreatLevel { get; set; }
    public float HealthPercent { get; set; }
    public float Confidence { get; set; }
}