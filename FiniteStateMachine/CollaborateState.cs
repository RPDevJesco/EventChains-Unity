using System.Linq;
using UnityEngine;

public class CollaborateState : BaseState
{
    private AIAgent _allyToSupport;
    private Vector3 _supportPosition;
    
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("[State] Entered COLLABORATE - Supporting allies");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        // Find ally that needs support
        var allies = state.GetNearbyAllies(agent.Position, 20f);
        _allyToSupport = allies
            .OrderBy(a => a.Health / a.MaxHealth) // Prioritize wounded
            .FirstOrDefault();
        
        if (_allyToSupport == null)
        {
            Debug.Log("[Collaborate] No allies nearby");
            return;
        }
        
        // Position near ally
        var distance = Vector3.Distance(agent.Position, _allyToSupport.Position);
        
        if (distance > 8f)
        {
            // Move closer to ally
            agent.MoveTo(_allyToSupport.Position);
        }
        else if (distance < 3f)
        {
            // Too close - spread out a bit
            var spreadOffset = (agent.Position - _allyToSupport.Position).normalized * 5f;
            _supportPosition = _allyToSupport.Position + spreadOffset;
            agent.MoveTo(_supportPosition);
        }
        else
        {
            // Good position - provide covering fire
            var allyTarget = _allyToSupport.CurrentTarget;
            if (allyTarget != null)
            {
                agent.AimAt(allyTarget.Position);
                agent.Shoot(allyTarget);
            }
            
            // Call out enemy positions
            var enemies = state.GetNearbyEnemies(agent.Position, 25f);
            foreach (var enemy in enemies)
            {
                agent.CallOut(enemy.Position);
            }
        }
    }
}