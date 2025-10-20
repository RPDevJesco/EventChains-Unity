using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BehaviorTree
{
    private readonly BTNode _rootNode;
    public float LastExecutionConfidence { get; private set; }
    
    public BehaviorTree()
    {
        // Build the behavior tree
        _rootNode = BuildTree();
    }
    
    private BTNode BuildTree()
    {
        // Root selector - tries each branch until one succeeds
        return new BTSelector("Root",
            
            // Emergency behaviors (highest priority)
            new BTSequence("Emergency Response",
                new BTCondition("Is Critical Health?", ctx => ctx.Agent.Health / ctx.Agent.MaxHealth < 0.2f),
                new BTAction("Use Med Kit", ctx => ctx.Agent.UseMedKit())
            ),
            
            // Combat behaviors
            new BTSequence("Combat",
                new BTCondition("In Combat?", ctx => ctx.State == AIState.Combat),
                new BTSelector("Combat Options",
                    
                    // Reload if needed
                    new BTSequence("Reload",
                        new BTCondition("Need Reload?", ctx => ctx.Agent.Ammo < 5),
                        new BTAction("Take Cover", ctx => TakeCover(ctx)),
                        new BTAction("Reload Weapon", ctx => ctx.Agent.Reload())
                    ),
                    
                    // Throw grenade if grouped enemies
                    new BTSequence("Grenade",
                        new BTCondition("Has Grenade?", ctx => ctx.Agent.GrenadeCount > 0),
                        new BTCondition("Grouped Enemies?", ctx => HasGroupedEnemies(ctx)),
                        new BTAction("Throw Grenade", ctx => ThrowGrenade(ctx))
                    ),
                    
                    // Standard combat
                    new BTSequence("Engage",
                        new BTAction("Find Target", ctx => AcquireTarget(ctx)),
                        new BTAction("Aim", ctx => AimAtTarget(ctx)),
                        new BTAction("Shoot", ctx => ShootTarget(ctx))
                    )
                )
            ),
            
            // Flee behaviors
            new BTSequence("Flee",
                new BTCondition("Should Flee?", ctx => ctx.State == AIState.Flee),
                new BTAction("Find Escape Route", ctx => FindEscapeRoute(ctx)),
                new BTAction("Sprint Away", ctx => SprintAway(ctx))
            ),
            
            // Patrol behaviors
            new BTSequence("Patrol",
                new BTCondition("On Patrol?", ctx => ctx.State == AIState.Patrol),
                new BTSelector("Patrol Actions",
                    new BTSequence("Move to Waypoint",
                        new BTCondition("Has Waypoint?", ctx => ctx.Agent.HasPatrolWaypoint()),
                        new BTAction("Move", ctx => MoveToWaypoint(ctx))
                    ),
                    new BTAction("Get Next Waypoint", ctx => GetNextWaypoint(ctx))
                )
            ),
            
            // Investigate behaviors
            new BTSequence("Investigate",
                new BTCondition("Investigating?", ctx => ctx.State == AIState.Investigate),
                new BTAction("Search Area", ctx => SearchArea(ctx))
            ),
            
            // Collaborate behaviors
            new BTSequence("Collaborate",
                new BTCondition("Collaborating?", ctx => ctx.State == AIState.Collaborate),
                new BTSelector("Support Options",
                    new BTSequence("Heal Ally",
                        new BTCondition("Can Heal?", ctx => ctx.Agent.CanHeal),
                        new BTCondition("Ally Needs Heal?", ctx => HasWoundedAlly(ctx)),
                        new BTAction("Heal", ctx => HealAlly(ctx))
                    ),
                    new BTAction("Provide Cover Fire", ctx => ProvideCoverFire(ctx))
                )
            ),
            
            // Default idle
            new BTAction("Idle", ctx => DoIdle(ctx))
        );
    }
    
    public async Task<List<AIAction>> Tick(AIAgent agent, GameState state, AIState currentState)
    {
        var context = new BTContext
        {
            Agent = agent,
            GameState = state,
            State = currentState,
            Actions = new List<AIAction>()
        };
        
        var result = await _rootNode.Execute(context);
        
        LastExecutionConfidence = result == BTNodeResult.Success ? 0.9f : 0.5f;
        
        return context.Actions;
    }
    
    // ============================================================================
    // BEHAVIOR IMPLEMENTATIONS
    // ============================================================================
    
    private BTNodeResult TakeCover(BTContext ctx)
    {
        var cover = ctx.GameState.FindNearestCover(ctx.Agent.Position, ctx.Agent.CurrentTarget?.Position ?? ctx.Agent.Position);
        if (cover.HasValue)
        {
            ctx.Actions.Add(new AIAction
            {
                Type = "Move",
                TargetPosition = cover.Value,
                Priority = 0.9f
            });
            return BTNodeResult.Success;
        }
        return BTNodeResult.Failure;
    }
    
    private bool HasGroupedEnemies(BTContext ctx)
    {
        var enemies = ctx.GameState.GetNearbyEnemies(ctx.Agent.Position, 30f);
        
        // Check if 3+ enemies within 5m of each other
        for (int i = 0; i < enemies.Count; i++)
        {
            int nearbyCount = enemies.Count(e => 
                Vector3.Distance(enemies[i].Position, e.Position) < 5f);
            
            if (nearbyCount >= 3)
                return true;
        }
        
        return false;
    }
    
    private BTNodeResult ThrowGrenade(BTContext ctx)
    {
        var enemies = ctx.GameState.GetNearbyEnemies(ctx.Agent.Position, 30f);
        
        // Find cluster center
        var clusterCenter = enemies
            .Aggregate(Vector3.zero, (sum, e) => sum + e.Position) / enemies.Count;
        
        ctx.Actions.Add(new AIAction
        {
            Type = "ThrowGrenade",
            TargetPosition = clusterCenter,
            Priority = 0.95f
        });
        
        return BTNodeResult.Success;
    }
    
    private BTNodeResult AcquireTarget(BTContext ctx)
    {
        if (ctx.Agent.CurrentTarget != null && !ctx.Agent.CurrentTarget.IsDead)
            return BTNodeResult.Success;
        
        var enemies = ctx.GameState.GetNearbyEnemies(ctx.Agent.Position, 30f);
        ctx.Agent.CurrentTarget = enemies.OrderBy(e => 
            Vector3.Distance(ctx.Agent.Position, e.Position)).FirstOrDefault();
        
        return ctx.Agent.CurrentTarget != null ? BTNodeResult.Success : BTNodeResult.Failure;
    }
    
    private BTNodeResult AimAtTarget(BTContext ctx)
    {
        if (ctx.Agent.CurrentTarget == null)
            return BTNodeResult.Failure;
        
        ctx.Actions.Add(new AIAction
        {
            Type = "Aim",
            TargetAgent = ctx.Agent.CurrentTarget,
            Priority = 0.8f
        });
        
        return BTNodeResult.Success;
    }
    
    private BTNodeResult ShootTarget(BTContext ctx)
    {
        if (ctx.Agent.CurrentTarget == null || ctx.Agent.Ammo <= 0)
            return BTNodeResult.Failure;
        
        ctx.Actions.Add(new AIAction
        {
            Type = "Attack",
            TargetAgent = ctx.Agent.CurrentTarget,
            Priority = 0.85f
        });
        
        return BTNodeResult.Success;
    }
    
    private BTNodeResult FindEscapeRoute(BTContext ctx)
    {
        var safePos = ctx.GameState.FindSafePosition(ctx.Agent.Position, 30f);
        if (safePos.HasValue)
        {
            ctx.Agent.EscapeTarget = safePos.Value;
            return BTNodeResult.Success;
        }
        return BTNodeResult.Failure;
    }
    
    private BTNodeResult SprintAway(BTContext ctx)
    {
        ctx.Actions.Add(new AIAction
        {
            Type = "Move",
            TargetPosition = ctx.Agent.EscapeTarget,
            Priority = 1.0f
        });
        
        ctx.Actions.Add(new AIAction
        {
            Type = "Sprint",
            Priority = 1.0f
        });
        
        return BTNodeResult.Success;
    }
    
    private BTNodeResult MoveToWaypoint(BTContext ctx)
    {
        ctx.Actions.Add(new AIAction
        {
            Type = "Move",
            TargetPosition = ctx.Agent.CurrentWaypoint,
            Priority = 0.5f
        });
        
        return BTNodeResult.Success;
    }
    
    private BTNodeResult GetNextWaypoint(BTContext ctx)
    {
        ctx.Agent.AdvanceToNextWaypoint();
        return BTNodeResult.Success;
    }
    
    private BTNodeResult SearchArea(BTContext ctx)
    {
        ctx.Actions.Add(new AIAction
        {
            Type = "Search",
            TargetPosition = ctx.Agent.InvestigationPoint,
            Priority = 0.6f
        });
        
        return BTNodeResult.Success;
    }
    
    private bool HasWoundedAlly(BTContext ctx)
    {
        var allies = ctx.GameState.GetNearbyAllies(ctx.Agent.Position, 15f);
        return allies.Any(a => a.Health / a.MaxHealth < 0.5f);
    }
    
    private BTNodeResult HealAlly(BTContext ctx)
    {
        var wounded = ctx.GameState.GetNearbyAllies(ctx.Agent.Position, 15f)
            .OrderBy(a => a.Health / a.MaxHealth)
            .FirstOrDefault();
        
        if (wounded != null)
        {
            ctx.Actions.Add(new AIAction
            {
                Type = "Heal",
                TargetAgent = wounded,
                Priority = 0.9f
            });
            return BTNodeResult.Success;
        }
        
        return BTNodeResult.Failure;
    }
    
    private BTNodeResult ProvideCoverFire(BTContext ctx)
    {
        var allies = ctx.GameState.GetNearbyAllies(ctx.Agent.Position, 15f);
        foreach (var ally in allies)
        {
            if (ally.CurrentTarget != null)
            {
                ctx.Actions.Add(new AIAction
                {
                    Type = "Attack",
                    TargetAgent = ally.CurrentTarget,
                    Priority = 0.7f
                });
                return BTNodeResult.Success;
            }
        }
        
        return BTNodeResult.Failure;
    }
    
    private BTNodeResult DoIdle(BTContext ctx)
    {
        ctx.Actions.Add(new AIAction
        {
            Type = "Idle",
            Priority = 0.1f
        });
        
        return BTNodeResult.Success;
    }
}