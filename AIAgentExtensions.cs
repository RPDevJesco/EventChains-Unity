using UnityEngine;

public static class AIAgentExtensions
{
    public static bool HasPatrolWaypoint(this AIAgent agent) 
        => agent.CurrentWaypoint != Vector3.zero;
    
    public static void AdvanceToNextWaypoint(this AIAgent agent)
    {
        // Implementation specific to your patrol system
    }
}