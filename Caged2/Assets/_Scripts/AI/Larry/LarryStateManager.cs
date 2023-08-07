using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System;

public enum State{
    Wander,
    Chase,
    Attack,
}
public class LarryStateManager : NetworkBehaviour
{
    public State CurrentAIState;
    private LarryBaseState currentState;
    public LarryWanderState WanderState = new();
    public LarryChaseState ChaseState = new();
    public LarryAttackState AttackState = new();
    [Header("Bools")]
    public bool _isActive;
    public bool _walkPointSet;

    [Header("Variables")]
    public NavMeshAgent agent;
    public Transform _Target;
    public Vector3 _walkPointPosition;

    [Header("Parameters")]
    public Vector2 _wanderSpeedRange;
    public Vector2 _wanderWaitRange;
    public float _checkRadius;
    public float _wanderDistance;
    public float _chaseRadius;
    public float _attackRadius;
    public int _maxWalkPointAttempts;
    [SerializeField] private int _colliderCount;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Collider[] results;

    //public override void OnNetworkSpawn()
    void Start()
    {
        results = new Collider[_colliderCount];
        currentState = WanderState;
        CurrentAIState = State.Wander;
        currentState.EnterState(this);
    }
    void Update(){ 
        //if (!_isActive || !IsServer) return;
        Physics.OverlapSphereNonAlloc(transform.position, _checkRadius, results, layerMask);
        currentState.UpdateState(this, results);
    }
    public void SwitchState(LarryBaseState state){
        currentState = state;
        state.EnterState(this);
    }
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _checkRadius);
    }
}

