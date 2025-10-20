using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    private readonly Dictionary<AIState, IState> _states = new();
    private IState _currentState;
    private AIState _currentStateEnum;
    
    public AIState CurrentState => _currentStateEnum;
    public float LastTransitionConfidence { get; private set; }
    
    public FiniteStateMachine()
    {
        // Register all states
        RegisterState(AIState.Idle, new IdleState());
        RegisterState(AIState.Patrol, new PatrolState());
        RegisterState(AIState.Investigate, new InvestigateState());
        RegisterState(AIState.Combat, new CombatState());
        RegisterState(AIState.Flee, new FleeState());
        RegisterState(AIState.Collaborate, new CollaborateState());
    }
    
    private void RegisterState(AIState stateEnum, IState state)
    {
        _states[stateEnum] = state;
    }
    
    public void TransitionTo(AIState newState)
    {
        if (_currentStateEnum == newState && _currentState != null)
        {
            LastTransitionConfidence = 1.0f; // Already in this state
            return;
        }
        
        // Exit current state
        _currentState?.OnExit();
        
        // Enter new state
        _currentStateEnum = newState;
        _currentState = _states[newState];
        _currentState.OnEnter();
        
        LastTransitionConfidence = 0.95f; // Successful transition
        
        Debug.Log($"[FSM] Transitioned to {newState}");
    }
    
    public void Update(AIAgent agent, GameState state)
    {
        _currentState?.OnUpdate(agent, state);
    }
}