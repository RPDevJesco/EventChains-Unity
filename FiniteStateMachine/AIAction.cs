using UnityEngine;

public class AIAction
{
    public string Type { get; set; }
    public Vector3 TargetPosition { get; set; }
    public AIAgent TargetAgent { get; set; }
    public float Priority { get; set; }
    
    public bool IsValidFor(AIAgent agent, GameState state)
    {
        // Validate action is possible
        return Type switch
        {
            "Move" => agent.CanMove && Vector3.Distance(agent.Position, TargetPosition) > 0.1f,
            "Attack" => agent.CanAttack && TargetAgent != null && !TargetAgent.IsDead,
            "Heal" => agent.CanHeal && agent.Health < agent.MaxHealth,
            "TakeCover" => agent.CanMove && state.HasCoverNear(TargetPosition),
            _ => true
        };
    }
}