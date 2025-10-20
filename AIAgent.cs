using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    [Header("Identity")]
    public string Name = "Agent";
    public int AgentID;
    public Team Team;
    
    [Header("Stats")]
    public float Health = 100f;
    public float MaxHealth = 100f;
    public float Ammo = 30f;
    public float MaxAmmo = 30f;
    public int GrenadeCount = 2;
    public float CombatRating = 5f;
    
    [Header("Capabilities")]
    public bool CanMove = true;
    public bool CanAttack = true;
    public bool CanHeal = true;
    public float FireRate = 0.5f;
    public float MoveSpeed = 5f;
    public float RotationSpeed = 180f;
    
    [Header("State")]
    public AIState CurrentState = AIState.Idle;
    public AIAgent CurrentTarget;
    public Vector3 CurrentWaypoint;
    public Vector3 InvestigationPoint;
    public Vector3 EscapeTarget;
    public Vector3 LastKnownPosition;
    public bool IsInCover;
    public bool IsDead => Health <= 0;
    public bool IsStunned;
    
    [Header("Memory")]
    public List<AIDecisionRecord> DecisionHistory = new();
    
    // Component references
    private UnityEngine.AI.NavMeshAgent _navAgent;
    private Animator _animator;
    
    public Vector3 Position => transform.position;
    
    private void Awake()
    {
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
        if (_navAgent != null)
        {
            _navAgent.speed = MoveSpeed;
        }
    }
    
    // ============================================================================
    // MOVEMENT
    // ============================================================================
    
    public void MoveTo(Vector3 position)
    {
        if (!CanMove || _navAgent == null) return;
        
        _navAgent.SetDestination(position);
        PlayAnimation("Walk");
    }
    
    public void Sprint(bool enable)
    {
        if (_navAgent == null) return;
        
        _navAgent.speed = enable ? MoveSpeed * 1.5f : MoveSpeed;
        PlayAnimation(enable ? "Sprint" : "Walk");
    }
    
    public void Wait(float seconds)
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
        }
        
        StartCoroutine(WaitCoroutine(seconds));
    }
    
    private IEnumerator WaitCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (_navAgent != null)
        {
            _navAgent.isStopped = false;
        }
    }
    
    public BTNodeResult MoveTo_BT(Vector3 position)
    {
        if (!CanMove) return BTNodeResult.Failure;
        
        MoveTo(position);
        return BTNodeResult.Success;
    }
    
    public BTNodeResult Sprint_BT(bool enable)
    {
        Sprint(enable);
        return BTNodeResult.Success;
    }
    
    public BTNodeResult Shoot_BT(AIAgent target)
    {
        if (!CanAttack || Ammo <= 0 || target == null || target.IsDead)
            return BTNodeResult.Failure;
        
        Shoot(target);
        return BTNodeResult.Success;
    }
    
    public BTNodeResult AimAt_BT(Vector3 targetPosition)
    {
        AimAt(targetPosition);
        return BTNodeResult.Success;
    }
    
    // ============================================================================
    // COMBAT
    // ============================================================================
    
    public void AimAt(Vector3 targetPosition)
    {
        var direction = (targetPosition - Position).normalized;
        direction.y = 0; // Keep level
        
        if (direction != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                RotationSpeed * Time.deltaTime
            );
        }
    }
    
    public void Shoot(AIAgent target)
    {
        if (!CanAttack || Ammo <= 0 || target == null || target.IsDead)
            return;
        
        Ammo--;
        PlayAnimation("Shoot");
        
        // Simple hit detection
        var distance = Vector3.Distance(Position, target.Position);
        var accuracy = Mathf.Clamp01(1f - (distance / 30f)); // Less accurate at distance
        
        if (UnityEngine.Random.value < accuracy)
        {
            var damage = UnityEngine.Random.Range(10f, 25f);
            target.TakeDamage(damage);
            Debug.Log($"{Name} hit {target.Name} for {damage:F1} damage");
        }
        else
        {
            Debug.Log($"{Name} missed {target.Name}");
        }
    }
    
    public BTNodeResult Reload()
    {
        if (Ammo >= MaxAmmo) 
            return BTNodeResult.Failure;
        
        PlayAnimation("Reload");
        StartCoroutine(ReloadCoroutine());
        return BTNodeResult.Success;
    }
    
    private IEnumerator ReloadCoroutine()
    {
        CanAttack = false;
        yield return new WaitForSeconds(2f);
        Ammo = MaxAmmo;
        CanAttack = true;
        Debug.Log($"{Name} reloaded");
    }
    
    public void TakeDamage(float damage)
    {
        Health = Mathf.Max(0, Health - damage);
        
        if (Health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log($"{Name} has been eliminated");
        CurrentState = AIState.Idle;
        CanMove = false;
        CanAttack = false;
        PlayAnimation("Death");
        
        if (_navAgent != null)
        {
            _navAgent.enabled = false;
        }
    }
    
    // ============================================================================
    // ABILITIES
    // ============================================================================
    
    public BTNodeResult UseMedKit()
    {
        if (Health >= MaxHealth) 
            return BTNodeResult.Failure;
        
        var healAmount = Mathf.Min(50f, MaxHealth - Health);
        Health += healAmount;
        
        Debug.Log($"{Name} used med kit, healed {healAmount:F1} HP");
        PlayAnimation("Heal");
        
        return BTNodeResult.Success;
    }
    
    public BTNodeResult ThrowGrenade(Vector3 targetPosition)
    {
        if (GrenadeCount <= 0)
            return BTNodeResult.Failure;
        
        GrenadeCount--;
        PlayAnimation("Throw");
        
        Debug.Log($"{Name} threw grenade at {targetPosition}");
        
        // You would spawn actual grenade GameObject here
        return BTNodeResult.Success;
    }
    
    // ============================================================================
    // PERCEPTION
    // ============================================================================
    
    public void ScanForEnemies(float range)
    {
        var enemies = Physics.OverlapSphere(Position, range)
            .Select(c => c.GetComponent<AIAgent>())
            .Where(a => a != null && a.Team != Team && !a.IsDead)
            .ToList();
        
        if (enemies.Count > 0 && CurrentTarget == null)
        {
            CurrentTarget = enemies[0];
            Debug.Log($"{Name} spotted enemy: {CurrentTarget.Name}");
        }
    }
    
    public void LookAtNearest(List<Vector3> pointsOfInterest)
    {
        if (pointsOfInterest == null || pointsOfInterest.Count == 0)
            return;
        
        var nearest = pointsOfInterest
            .OrderBy(p => Vector3.Distance(Position, p))
            .FirstOrDefault();
        
        AimAt(nearest);
    }
    
    public void SearchArea(Vector3 center, float radius)
    {
        // Simple area search - look around
        var searchPoints = new[]
        {
            center + Vector3.forward * radius,
            center + Vector3.right * radius,
            center + Vector3.back * radius,
            center + Vector3.left * radius
        };
        
        LookAtNearest(searchPoints.ToList());
    }
    
    public void CallOut(Vector3 enemyPosition)
    {
        Debug.Log($"{Name} calling out enemy at {enemyPosition}");
        // Notify nearby allies
    }
    
    // ============================================================================
    // ANIMATION
    // ============================================================================
    
    public void PlayAnimation(string animationName)
    {
        if (_animator == null) return;
        
        _animator.SetTrigger(animationName);
    }
}