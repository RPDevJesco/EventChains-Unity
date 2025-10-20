using System.Linq;
using UnityEngine;

public class CombatState : BaseState
{
    private AIAgent _currentTarget;
    private float _lastShotTime;
    private Vector3 _lastCoverPosition;
    
    public override void OnEnter()
    {
        base.OnEnter();
        _currentTarget = null;
        Debug.Log("[State] Entered COMBAT - Engaging hostiles");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        // Acquire target
        if (_currentTarget == null || _currentTarget.IsDead)
        {
            var enemies = state.GetNearbyEnemies(agent.Position, 30f);
            _currentTarget = enemies
                .OrderBy(e => Vector3.Distance(agent.Position, e.Position))
                .FirstOrDefault();
        }
        
        if (_currentTarget == null)
        {
            Debug.Log("[Combat] No targets available");
            return;
        }
        
        // Combat tactics
        var distanceToTarget = Vector3.Distance(agent.Position, _currentTarget.Position);
        
        if (distanceToTarget > 20f)
        {
            // Too far - move closer
            agent.MoveTo(_currentTarget.Position);
        }
        else if (distanceToTarget < 5f)
        {
            // Too close - back up to cover
            var coverPos = state.FindNearestCover(agent.Position, _currentTarget.Position);
            if (coverPos.HasValue)
            {
                agent.MoveTo(coverPos.Value);
                _lastCoverPosition = coverPos.Value;
            }
        }
        else
        {
            // Good range - take cover and shoot
            if (agent.IsInCover || Vector3.Distance(agent.Position, _lastCoverPosition) < 1f)
            {
                agent.AimAt(_currentTarget.Position);
                
                if (Time.time - _lastShotTime > agent.FireRate)
                {
                    agent.Shoot(_currentTarget);
                    _lastShotTime = Time.time;
                }
            }
            else
            {
                // Find cover
                var coverPos = state.FindNearestCover(agent.Position, _currentTarget.Position);
                if (coverPos.HasValue)
                {
                    agent.MoveTo(coverPos.Value);
                    _lastCoverPosition = coverPos.Value;
                }
            }
        }
    }
}