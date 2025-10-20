using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    private List<Vector3> _patrolPoints;
    private int _currentPointIndex;
    
    public override void OnEnter()
    {
        base.OnEnter();
        _patrolPoints = GetPatrolRoute();
        _currentPointIndex = 0;
        Debug.Log("[State] Entered PATROL - Scouting area");
    }
    
    public override void OnUpdate(AIAgent agent, GameState state)
    {
        base.OnUpdate(agent, state);
        
        if (_patrolPoints == null || _patrolPoints.Count == 0)
            return;
        
        var targetPoint = _patrolPoints[_currentPointIndex];
        agent.MoveTo(targetPoint);
        
        // Check if reached patrol point
        if (Vector3.Distance(agent.Position, targetPoint) < 1f)
        {
            _currentPointIndex = (_currentPointIndex + 1) % _patrolPoints.Count;
            agent.Wait(2f); // Pause at each point
        }
        
        // Scan for threats while patrolling
        agent.ScanForEnemies(15f);
    }
    
    private List<Vector3> GetPatrolRoute()
    {
        // Generate or retrieve patrol waypoints
        return new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 0, 10),
            new Vector3(0, 0, 10)
        };
    }
}