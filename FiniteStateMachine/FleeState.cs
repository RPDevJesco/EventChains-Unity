using System.Linq;
using UnityEngine;

public class FleeState : BaseState
{
    private Vector3 _fleeDirection;
    private float _fleeDuration = 5f;
    
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("[State] Entered FLEE - Retreating to safety");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        // Find enemies to flee from
        var enemies = state.GetNearbyEnemies(agent.Position, 30f);
        
        if (enemies.Count > 0)
        {
            // Calculate flee direction (away from enemies)
            var avgEnemyPosition = enemies
                .Aggregate(Vector3.zero, (sum, e) => sum + e.Position) / enemies.Count;
            
            _fleeDirection = (agent.Position - avgEnemyPosition).normalized;
        }
        
        // Run away!
        var fleeTarget = agent.Position + _fleeDirection * 20f;
        agent.MoveTo(fleeTarget);
        agent.Sprint(true);
        
        // Look for safe positions
        var safePosition = state.FindSafePosition(agent.Position, 30f);
        if (safePosition.HasValue)
        {
            agent.MoveTo(safePosition.Value);
        }
        
        // Stop fleeing if safe or after duration
        if (_stateTime > _fleeDuration || enemies.Count == 0)
        {
            Debug.Log("[State] Reached safety");
            agent.Sprint(false);
        }
    }
}