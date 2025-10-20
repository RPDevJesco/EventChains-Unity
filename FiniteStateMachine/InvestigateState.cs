using UnityEngine;

public class InvestigateState : BaseState
{
    private Vector3 _investigatePosition;
    private float _investigateRadius = 5f;
    
    public override void OnEnter()
    {
        base.OnEnter();
        _investigatePosition = Vector3.zero; // Will be set by first suspicious event
        Debug.Log("[State] Entered INVESTIGATE - Checking suspicious activity");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        // Move to last known suspicious position
        if (_investigatePosition != Vector3.zero)
        {
            agent.MoveTo(_investigatePosition);
            
            if (Vector3.Distance(agent.Position, _investigatePosition) < 2f)
            {
                // Search the area
                agent.SearchArea(_investigatePosition, _investigateRadius);
            }
        }
        
        // Look for clues or enemies
        var enemies = state.GetNearbyEnemies(agent.Position, 20f);
        if (enemies.Count > 0)
        {
            _investigatePosition = enemies[0].LastKnownPosition;
        }
        
        // Give up after 15 seconds
        if (_stateTime > 15f)
        {
            Debug.Log("[State] Investigation timeout - nothing found");
        }
    }
}