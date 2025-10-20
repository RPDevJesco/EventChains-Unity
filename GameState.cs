using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("World")]
    public List<AIAgent> AllAgents = new();
    public List<Vector3> CoverPositions = new();
    public List<Vector3> PatrolWaypoints = new();
    public List<Vector3> PointsOfInterest = new();
    
    [Header("Settings")]
    public float CoverSearchRadius = 20f;
    public LayerMask CoverLayer;
    
    private static GameState _instance;
    public static GameState Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find all agents in scene
        //AllAgents = FindObjectsOfType<AIAgent>().ToList();
        
        // Find cover positions
        //DiscoverCoverPositions();
        
        Debug.Log($"GameState initialized: {AllAgents.Count} agents, {CoverPositions.Count} cover positions");
    }
    
    // ============================================================================
    // AGENT QUERIES
    // ============================================================================
    
    public List<AIAgent> GetNearbyEnemies(Vector3 position, float range)
    {
        return AllAgents
            .Where(a => !a.IsDead && Vector3.Distance(a.Position, position) <= range)
            .ToList();
    }
    
    public List<AIAgent> GetNearbyAllies(Vector3 position, float range)
    {
        return AllAgents
            .Where(a => !a.IsDead && Vector3.Distance(a.Position, position) <= range)
            .ToList();
    }
    
    public List<AIAgent> GetAllEnemiesOfTeam(Team team)
    {
        return AllAgents
            .Where(a => !a.IsDead && a.Team != team)
            .ToList();
    }
    
    public List<AIAgent> GetAllAlliesOfTeam(Team team)
    {
        return AllAgents
            .Where(a => !a.IsDead && a.Team == team)
            .ToList();
    }
    
    // ============================================================================
    // SPATIAL QUERIES
    // ============================================================================
    
    public Vector3? FindNearestCover(Vector3 fromPosition, Vector3 threatPosition)
    {
        if (CoverPositions.Count == 0)
            return null;
        
        // Find cover that's between agent and threat
        var bestCover = CoverPositions
            .Where(c => IsCoverViable(c, fromPosition, threatPosition))
            .OrderBy(c => Vector3.Distance(fromPosition, c))
            .FirstOrDefault();
        
        return bestCover != Vector3.zero ? bestCover : (Vector3?)null;
    }
    
    private bool IsCoverViable(Vector3 coverPos, Vector3 agentPos, Vector3 threatPos)
    {
        // Cover should be closer to agent than threat
        var distToAgent = Vector3.Distance(coverPos, agentPos);
        var distToThreat = Vector3.Distance(coverPos, threatPos);
        
        if (distToAgent > distToThreat * 0.8f)
            return false;
        
        // Cover should provide protection from threat
        var toThreat = (threatPos - coverPos).normalized;
        var toCover = (coverPos - agentPos).normalized;
        var alignment = Vector3.Dot(toThreat, toCover);
        
        return alignment < 0; // Cover is between agent and threat
    }
    
    public bool HasCoverNear(Vector3 position, float radius = 5f)
    {
        return CoverPositions.Any(c => Vector3.Distance(c, position) <= radius);
    }
    
    public Vector3? FindSafePosition(Vector3 fromPosition, float searchRadius)
    {
        // Find position far from all threats
        var enemies = AllAgents.Where(a => !a.IsDead).ToList();
        
        if (enemies.Count == 0)
            return null;
        
        // Sample positions in a circle
        for (int i = 0; i < 16; i++)
        {
            var angle = i * (360f / 16f) * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * searchRadius;
            var testPos = fromPosition + offset;
            
            // Check if this position is far from enemies
            var nearestEnemyDist = enemies.Min(e => Vector3.Distance(e.Position, testPos));
            
            if (nearestEnemyDist > searchRadius * 0.8f)
            {
                return testPos;
            }
        }
        
        return null;
    }
    
    public List<Vector3> GetPointsOfInterest()
    {
        return PointsOfInterest;
    }
    
    // ============================================================================
    // COVER DISCOVERY
    // ============================================================================
    
    private void DiscoverCoverPositions()
    {
        CoverPositions.Clear();
        
        // Find all objects tagged as cover
        var coverObjects = GameObject.FindGameObjectsWithTag("Cover");
        
        foreach (var obj in coverObjects)
        {
            CoverPositions.Add(obj.transform.position);
        }
        
        // If no tagged cover, sample the environment
        if (CoverPositions.Count == 0)
        {
            SampleEnvironmentForCover();
        }
    }
    
    private void SampleEnvironmentForCover()
    {
        // Simple grid sampling for cover
        var bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        
        for (float x = bounds.min.x; x < bounds.max.x; x += 5f)
        {
            for (float z = bounds.min.z; z < bounds.max.z; z += 5f)
            {
                var testPos = new Vector3(x, 0, z);
                
                // Raycast to find ground
                if (Physics.Raycast(testPos + Vector3.up * 10f, Vector3.down, out var hit, 20f))
                {
                    // Check if there's an obstacle nearby (potential cover)
                    if (Physics.CheckSphere(hit.point, 1f, CoverLayer))
                    {
                        CoverPositions.Add(hit.point);
                    }
                }
            }
        }
        
        Debug.Log($"Sampled {CoverPositions.Count} potential cover positions");
    }
    
    // ============================================================================
    // UTILITIES
    // ============================================================================
    
    public void RegisterAgent(AIAgent agent)
    {
        if (!AllAgents.Contains(agent))
        {
            AllAgents.Add(agent);
        }
    }
    
    public void UnregisterAgent(AIAgent agent)
    {
        AllAgents.Remove(agent);
    }
}