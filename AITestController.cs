// ============================================================================
// AI TEST CONTROLLER - Unity MonoBehaviour
// ============================================================================
// Attach this to a GameObject in your Unity scene to test the AI system
// Configure the test scenario in the Inspector and press Play

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using EventChain;

public class AITestController : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private TestScenario _scenario = TestScenario.SimpleCombat;
    [SerializeField] private bool _autoRun = true;
    [SerializeField] private float _thinkInterval = 1f;
    [SerializeField] private int _maxFrames = 30;
    
    [Header("Agent Prefabs")]
    [SerializeField] private GameObject _agentPrefab;
    
    [Header("Visualization")]
    [SerializeField] private bool _showDebugGizmos = true;
    [SerializeField] private bool _showAgentLabels = true;
    
    [Header("Runtime Info")]
    [SerializeField] private int _currentFrame = 0;
    [SerializeField] private List<AIAgent> _testAgents = new();
    [SerializeField] private GameState _gameState;
    
    private FiniteStateMachine _fsm;
    private BehaviorTree _behaviorTree;
    private bool _isInitialized = false;
    
    // ============================================================================
    // UNITY LIFECYCLE
    // ============================================================================
    
    private void Start()
    {
        // Prevent multiple initializations
        if (_isInitialized)
        {
            Debug.LogWarning("[Test] Already initialized, skipping Start()");
            return;
        }
        
        _isInitialized = true;
        
        // Ensure GameState exists
        _gameState = GameState.Instance;
        if (_gameState == null)
        {
            var go = new GameObject("GameState");
            _gameState = go.AddComponent<GameState>();
        }
        
        // Initialize AI systems
        _fsm = new FiniteStateMachine();
        _behaviorTree = new BehaviorTree();
        
        // Setup test scenario
        SetupScenario(_scenario);
        
        // Start test if auto-run enabled
        if (_autoRun)
        {
            StartCoroutine(RunTestCoroutine());
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || _testAgents == null) return;
        
        foreach (var agent in _testAgents)
        {
            if (agent == null) continue;
            
            // Draw agent sphere
            Gizmos.color = GetTeamColor(agent.Team);
            Gizmos.DrawWireSphere(agent.Position, 0.5f);
            
            // Draw health bar
            DrawHealthBar(agent);
            
            // Draw target line
            if (agent.CurrentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(agent.Position, agent.CurrentTarget.Position);
            }
            
            // Draw current waypoint
            if (agent.CurrentWaypoint != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(agent.Position, agent.CurrentWaypoint);
                Gizmos.DrawWireSphere(agent.CurrentWaypoint, 0.3f);
            }
        }
        
        // Draw cover positions
        if (_gameState != null)
        {
            Gizmos.color = Color.green;
            foreach (var cover in _gameState.CoverPositions)
            {
                Gizmos.DrawWireCube(cover, Vector3.one * 0.5f);
            }
        }
    }
    
    private void OnGUI()
    {
        if (!_showAgentLabels) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        
        foreach (var agent in _testAgents)
        {
            if (agent == null) continue;
            
            Vector3 screenPos = Camera.main.WorldToScreenPoint(agent.Position + Vector3.up * 2);
            
            if (screenPos.z > 0)
            {
                string label = $"{agent.Name}\n" +
                              $"State: {agent.CurrentState}\n" +
                              $"HP: {agent.Health:F0}/{agent.MaxHealth:F0}\n" +
                              $"Ammo: {agent.Ammo:F0}";
                
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 80), label, style);
            }
        }
        
        // Control panel
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("AI Test Controller");
        
        GUILayout.Label($"Frame: {_currentFrame}/{_maxFrames}");
        GUILayout.Label($"Scenario: {_scenario}");
        GUILayout.Label($"Agents Alive: {_testAgents.Count(a => !a.IsDead)}/{_testAgents.Count}");
        
        if (GUILayout.Button("Run Single Frame"))
        {
            RunSingleFrameSync();
            _currentFrame++;
        }
        
        if (GUILayout.Button("Reset Test"))
        {
            ResetTest();
        }
        
        GUILayout.EndArea();
    }
    
    // ============================================================================
    // TEST SCENARIOS
    // ============================================================================
    
    private void SetupScenario(TestScenario scenario)
    {
        Debug.Log($"[Test] Setting up scenario: {scenario}");
        
        ClearAgents();
        
        switch (scenario)
        {
            case TestScenario.SimpleCombat:
                SetupSimpleCombat();
                break;
            
            case TestScenario.Outnumbered:
                SetupOutnumbered();
                break;
            
            case TestScenario.TeamFight:
                SetupTeamFight();
                break;
            
            case TestScenario.PatrolEncounter:
                SetupPatrolEncounter();
                break;
            
            case TestScenario.LowHealthRetreat:
                SetupLowHealthRetreat();
                break;
            
            case TestScenario.Collaboration:
                SetupCollaboration();
                break;
        }
        
        Debug.Log($"[Test] Created {_testAgents.Count} agents");
    }
    
    private void SetupSimpleCombat()
    {
        // 1v1 basic combat test
        CreateTestAgent("Hero", Team.Player, new Vector3(0, 0, 0), 100f);
        CreateTestAgent("Enemy", Team.Enemy, new Vector3(15, 0, 0), 100f);
        
        // Add cover
        _gameState.CoverPositions.Add(new Vector3(5, 0, 0));
        _gameState.CoverPositions.Add(new Vector3(10, 0, 0));
    }
    
    private void SetupOutnumbered()
    {
        // Player outnumbered - should trigger retreat
        CreateTestAgent("Hero", Team.Player, new Vector3(0, 0, 0), 60f);
        
        CreateTestAgent("Enemy1", Team.Enemy, new Vector3(10, 0, 5), 100f);
        CreateTestAgent("Enemy2", Team.Enemy, new Vector3(10, 0, -5), 100f);
        CreateTestAgent("Enemy3", Team.Enemy, new Vector3(15, 0, 0), 100f);
        
        _gameState.CoverPositions.Add(new Vector3(-10, 0, 0));
    }
    
    private void SetupTeamFight()
    {
        // 2v2 team combat
        CreateTestAgent("Player1", Team.Player, new Vector3(0, 0, 0), 100f);
        CreateTestAgent("Player2", Team.Player, new Vector3(2, 0, 2), 100f);
        
        CreateTestAgent("Enemy1", Team.Enemy, new Vector3(15, 0, 0), 100f);
        CreateTestAgent("Enemy2", Team.Enemy, new Vector3(17, 0, 2), 100f);
        
        _gameState.CoverPositions.Add(new Vector3(5, 0, 0));
        _gameState.CoverPositions.Add(new Vector3(10, 0, 0));
    }
    
    private void SetupPatrolEncounter()
    {
        // Guard on patrol, player sneaking
        CreateTestAgent("Infiltrator", Team.Player, new Vector3(0, 0, 0), 100f);
        
        var guard = CreateTestAgent("Guard", Team.Enemy, new Vector3(20, 0, 0), 100f);
        guard.CurrentState = AIState.Patrol;
        
        // Set patrol waypoints
        _gameState.PatrolWaypoints.Clear();
        _gameState.PatrolWaypoints.Add(new Vector3(20, 0, 0));
        _gameState.PatrolWaypoints.Add(new Vector3(30, 0, 0));
        _gameState.PatrolWaypoints.Add(new Vector3(30, 0, 10));
        _gameState.PatrolWaypoints.Add(new Vector3(20, 0, 10));
        
        guard.CurrentWaypoint = _gameState.PatrolWaypoints[0];
    }
    
    private void SetupLowHealthRetreat()
    {
        // Test healing and retreat behavior
        var player = CreateTestAgent("WoundedHero", Team.Player, new Vector3(0, 0, 0), 20f);
        player.CanHeal = true;
        
        CreateTestAgent("Enemy", Team.Enemy, new Vector3(12, 0, 0), 100f);
        
        _gameState.CoverPositions.Add(new Vector3(-5, 0, 0));
    }
    
    private void SetupCollaboration()
    {
        // Test support/collaboration state
        var player1 = CreateTestAgent("Medic", Team.Player, new Vector3(0, 0, 0), 100f);
        player1.CanHeal = true;
        
        var player2 = CreateTestAgent("Soldier", Team.Player, new Vector3(3, 0, 0), 40f);
        
        CreateTestAgent("Enemy1", Team.Enemy, new Vector3(15, 0, 0), 100f);
        CreateTestAgent("Enemy2", Team.Enemy, new Vector3(18, 0, 3), 100f);
    }
    
    // ============================================================================
    // AGENT CREATION
    // ============================================================================
    
    private AIAgent CreateTestAgent(string name, Team team, Vector3 position, float health)
    {
        GameObject agentObj;
        
        if (_agentPrefab != null)
        {
            agentObj = Instantiate(_agentPrefab, position, Quaternion.identity);
            agentObj.name = name;
        }
        else
        {
            // Create basic GameObject if no prefab provided
            agentObj = new GameObject(name);
            agentObj.transform.position = position;
            
            // Add visual representation
            var cube = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cube.transform.SetParent(agentObj.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one * 0.5f;
            
            var renderer = cube.GetComponent<Renderer>();
            renderer.material.color = GetTeamColor(team);
        }
        
        // Add or get AIAgent component
        var agent = agentObj.GetComponent<AIAgent>();
        if (agent == null)
        {
            agent = agentObj.AddComponent<AIAgent>();
        }
        
        // Configure agent
        agent.Name = name;
        agent.AgentID = _testAgents.Count;
        agent.Team = team;
        agent.Health = health;
        agent.MaxHealth = 100f;
        agent.Ammo = 30f;
        agent.MaxAmmo = 30f;
        agent.GrenadeCount = 2;
        agent.CombatRating = 5f;
        agent.CurrentState = AIState.Idle;
        
        _testAgents.Add(agent);
        _gameState.RegisterAgent(agent);
        
        return agent;
    }
    
    private void ClearAgents()
    {
        foreach (var agent in _testAgents)
        {
            if (agent != null)
            {
                Destroy(agent.gameObject);
            }
        }
        
        _testAgents.Clear();
        
        if (_gameState != null)
        {
            _gameState.AllAgents.Clear();
            _gameState.CoverPositions.Clear();
            _gameState.PatrolWaypoints.Clear();
        }
    }
    
    // ============================================================================
    // TEST EXECUTION
    // ============================================================================
    
    private IEnumerator RunTestCoroutine()
    {
        Debug.Log("[Test] Starting automated test...");
        
        _currentFrame = 0;
        
        while (_currentFrame < _maxFrames)
        {
            yield return new WaitForSeconds(_thinkInterval);
            
            if (!IsTestComplete())
            {
                // Run frame synchronously to avoid async deadlock
                RunSingleFrameSync();
                _currentFrame++;
            }
            else
            {
                Debug.Log("[Test] Test completed early - win condition met");
                break;
            }
        }
        
        Debug.Log("[Test] Test finished!");
        PrintFinalResults();
    }
    
    private void RunSingleFrameSync()
    {
        Debug.Log($"\n========== FRAME {_currentFrame} ==========");
        
        // Update all alive agents
        var aliveAgents = _testAgents.Where(a => !a.IsDead).ToList();
        
        foreach (var agent in aliveAgents)
        {
            ExecuteAgentThinkCycleSync(agent);
        }
        
        PrintFrameStatus();
    }
    
    private void ExecuteAgentThinkCycleSync(AIAgent agent)
    {
        Debug.Log($"\n[{agent.Name}] ===== THINK CYCLE START =====");
        Debug.Log($"  Current State: {agent.CurrentState}");
        Debug.Log($"  Health: {agent.Health:F0}/{agent.MaxHealth:F0} ({agent.Health/agent.MaxHealth:P0})");
        Debug.Log($"  Ammo: {agent.Ammo:F0}/{agent.MaxAmmo:F0}");
        
        try
        {
            // Create context
            var context = new AIContext
            {
                Agent = agent,
                State = _gameState
            };
            
            // Execute each layer directly without async
            // Strategic Layer
            new GatherIntelligenceEvent().ExecuteAsync(context).Wait();
            new DetermineStrategyEvent().ExecuteAsync(context).Wait();
            new ValidateStrategyEvent().ExecuteAsync(context).Wait();
            
            Debug.Log($"[{agent.Name}] Strategy: {context.Data.Strategy}");
            
            // Operational Layer
            new CheckStateTransitionsEvent().ExecuteAsync(context).Wait();
            new ProcessFSMEvent(_fsm).ExecuteAsync(context).Wait();
            new ValidateStateEvent().ExecuteAsync(context).Wait();
            
            Debug.Log($"[{agent.Name}] State: {context.Data.CurrentState}");
            
            // Tactical Layer
            new ExecuteBehaviorTreeEvent(_behaviorTree).ExecuteAsync(context).Wait();
            new ValidateBehaviorEvent().ExecuteAsync(context).Wait();
            
            Debug.Log($"[{agent.Name}] Generated {context.Data.Actions.Count} actions");
            
            // Analysis
            new EvaluatePerformanceEvent().ExecuteAsync(context).Wait();
            new LearnFromOutcomeEvent().ExecuteAsync(context).Wait();
            
            // Update agent state
            agent.CurrentState = context.Data.CurrentState;
            
            // Execute actions
            ExecuteActions(agent, context.Data.Actions);
            
            Debug.Log($"[{agent.Name}] ===== THINK CYCLE END =====\n");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{agent.Name}] Think cycle error: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private void ExecuteActions(AIAgent agent, List<AIAction> actions)
    {
        if (actions == null || actions.Count == 0) return;
        
        // Sort by priority
        var sortedActions = actions.OrderByDescending(a => a.Priority).ToList();
        
        Debug.Log($"[{agent.Name}] Executing {sortedActions.Count} actions:");
        
        foreach (var action in sortedActions)
        {
            Debug.Log($"  - {action.Type} (Priority: {action.Priority:F2})");
            
            switch (action.Type)
            {
                case "Move":
                    agent.MoveTo(action.TargetPosition);
                    break;
                
                case "Attack":
                    if (action.TargetAgent != null)
                    {
                        agent.Shoot(action.TargetAgent);
                    }
                    break;
                
                case "Aim":
                    if (action.TargetAgent != null)
                    {
                        agent.AimAt(action.TargetAgent.Position);
                    }
                    break;
                
                case "Heal":
                    agent.UseMedKit();
                    break;
                
                case "Sprint":
                    agent.Sprint(true);
                    break;
                
                case "TakeCover":
                    agent.MoveTo(action.TargetPosition);
                    agent.IsInCover = true;
                    break;
            }
        }
    }
    
    // ============================================================================
    // TEST UTILITIES
    // ============================================================================
    
    private bool IsTestComplete()
    {
        // Test is complete if only one team remains
        var playerCount = _testAgents.Count(a => !a.IsDead && a.Team == Team.Player);
        var enemyCount = _testAgents.Count(a => !a.IsDead && a.Team == Team.Enemy);
        
        return playerCount == 0 || enemyCount == 0;
    }
    
    private void PrintFrameStatus()
    {
        Debug.Log("\n----- FRAME STATUS -----");
        
        foreach (var agent in _testAgents)
        {
            string status = agent.IsDead ? "DEAD" : "ALIVE";
            Debug.Log($"  {agent.Name} ({agent.Team}): {status} | " +
                     $"HP: {agent.Health:F0} | State: {agent.CurrentState}");
        }
    }
    
    private void PrintFinalResults()
    {
        Debug.Log("\n========================================");
        Debug.Log("FINAL TEST RESULTS");
        Debug.Log("========================================");
        
        var playerCount = _testAgents.Count(a => !a.IsDead && a.Team == Team.Player);
        var enemyCount = _testAgents.Count(a => !a.IsDead && a.Team == Team.Enemy);
        
        if (playerCount > enemyCount)
        {
            Debug.Log("RESULT: Player Team Victory!");
        }
        else if (enemyCount > playerCount)
        {
            Debug.Log("RESULT: Enemy Team Victory!");
        }
        else
        {
            Debug.Log("RESULT: Draw!");
        }
        
        Debug.Log($"\nSurvivors:");
        Debug.Log($"  Players: {playerCount}");
        Debug.Log($"  Enemies: {enemyCount}");
        
        Debug.Log("\nAgent Details:");
        foreach (var agent in _testAgents)
        {
            Debug.Log($"  {agent.Name}: {(agent.IsDead ? "KIA" : $"Survived with {agent.Health:F0} HP")}");
        }
        
        Debug.Log("========================================\n");
    }
    
    private void ResetTest()
    {
        _currentFrame = 0;
        SetupScenario(_scenario);
        StopAllCoroutines();
        
        if (_autoRun)
        {
            StartCoroutine(RunTestCoroutine());
        }
    }
    
    // ============================================================================
    // VISUALIZATION HELPERS
    // ============================================================================
    
    private Color GetTeamColor(Team team)
    {
        return team switch
        {
            Team.Player => Color.blue,
            Team.Enemy => Color.red,
            Team.Neutral => Color.gray,
            _ => Color.white
        };
    }
    
    private void DrawHealthBar(AIAgent agent)
    {
        if (agent.IsDead) return;
        
        Vector3 pos = agent.Position + Vector3.up * 1.5f;
        float healthPercent = agent.Health / agent.MaxHealth;
        
        Gizmos.color = Color.black;
        Gizmos.DrawLine(pos - Vector3.right * 0.5f, pos + Vector3.right * 0.5f);
        
        Gizmos.color = healthPercent > 0.5f ? Color.green : (healthPercent > 0.25f ? Color.yellow : Color.red);
        Gizmos.DrawLine(pos - Vector3.right * 0.5f, pos + Vector3.right * (healthPercent - 0.5f));
    }
}