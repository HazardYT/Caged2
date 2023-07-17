using UnityEngine;
using Unity.Netcode;

public class NetworkMovementComponent : NetworkBehaviour
{
    [SerializeField] private CharacterController _cc;

    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _sensitivity;

    [SerializeField] private Transform _camSocket;
    [SerializeField] private Transform _vcam;
    [SerializeField] private Animator anim;
    private Transform _vcamTransform;
    private string CurrentAnimState;
    private float _tickDeltaTime = 0;
    private int _tick = 0;
    private float ServerTickRate = 60f;
    private float _tickRate;
    private const int BUFFER_SIZE = 1024;

    private InputState[] _inputStates = new InputState[BUFFER_SIZE];

    private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];
    public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
    public TransformState _previousTransformState;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation; 
    private const float _positionLerpSpeed = 8f;
    private const float _rotationLerpSpeed = 8f;

    public const string IDLE = "Idle";
    public const string WALK_FORWARD = "Walk Forward";
    public const string WALK_BACKWARD = "Walk Backward";
    public const string WALK_LEFT = "Walk Left";
    public const string WALK_RIGHT = "Walk Right";
    public const string CROUCH_IDLE = "Crouch Idle";
    public const string CROUCH_FORWARD = "Crouch Forward";
    public const string CROUCH_BACKWARD = "Crouch Backward";
    public const string CROUCH_LEFT = "Crouch Left";
    public const string CROUCH_RIGHT = "Crouch Right";
    public const string RUN_FORWARD = "Run Forward";
    public const string RUN_BACKWARD = "Run Backward";
    public const string RUN_LEFT = "Run Left";
    public const string RUN_RIGHT = "Run Right";

    void Start(){
        _tickRate = 1f / ServerTickRate;
    }
    private void OnEnable(){
        ServerTransformState.OnValueChanged += OnValueChanged;
        TransformState state = new TransformState(){
        Tick = _tick,
        Position = transform.position,
        Rotation = transform.rotation,
        HasStartedMoving = false
        };

        _previousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }
    public override void OnNetworkSpawn()
    {
        _vcamTransform = _vcam.transform;   
    }
    private void OnValueChanged(TransformState previousValue, TransformState newValue)
    {
        _previousTransformState = previousValue;
        _targetPosition = newValue.Position;
        _targetRotation = newValue.Rotation;
    }
    public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput){
        bool isCrouched = UserInput.instance.CrouchHeld && !UserInput.instance.SprintHeld;
        bool isRunning = UserInput.instance.SprintHeld && !UserInput.instance.CrouchHeld;
        _tickDeltaTime += Time.deltaTime;
        if (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;

            if (!IsServer){
                MovePlayerServerRpc(_tick, movementInput, lookInput, isCrouched, isRunning);
                MovePlayer(movementInput, isCrouched, isRunning);
                RotatePlayer(lookInput);
            }
            else
            {
                MovePlayer(movementInput, isCrouched, isRunning);
                RotatePlayer(lookInput);

                TransformState state = new TransformState(){
                    Tick = _tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                _previousTransformState = ServerTransformState.Value;
                ServerTransformState.Value = state;
            }

            InputState inputState = new InputState(){
                Tick = _tick,
                movementInput = movementInput,
                lookInput = lookInput,
            };

            TransformState transformState = new TransformState(){
                Tick = _tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };

            _inputStates[bufferIndex] = inputState;
            _transformStates[bufferIndex] = transformState;

            _tickDeltaTime -= _tickRate;
            _tick++;
        }
    }

public void ProcessSimulatedPlayerMovement(){
    _tickDeltaTime += Time.deltaTime;
    if(_tickDeltaTime > _tickRate)
    {
        if(ServerTransformState.Value.HasStartedMoving){

            // Interpolate the position and rotation for smoother movement
            transform.position = Vector3.Lerp(transform.position, ServerTransformState.Value.Position, _positionLerpSpeed * _tickDeltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, ServerTransformState.Value.Rotation, _rotationLerpSpeed * _tickDeltaTime);
        }

        _tickDeltaTime -= _tickRate;
        _tick++;
    }
}

    private void MovePlayer(Vector2 movementInput, bool isCrouched, bool isRunning)
    {
        HandleAnimationParams(movementInput, isCrouched, isRunning);
        Vector3 movement = movementInput.x * transform.right + movementInput.y * transform.forward;

            movement.y = 0;
        if (!_cc.isGrounded){
            movement.y = -9.61f;
        }
        float speed = isRunning ? _runSpeed : _walkSpeed;
        _cc.Move(movement * speed * _tickRate);
        if (_cc.isGrounded){
            if (isCrouched){
                _cc.height = 2;
                _cc.center = new Vector3(0,-0.75f,0);
            }
            else{
                _cc.height = 3;
                _cc.center = new Vector3(0,-0.25f,0);
            }
        }
    }
    private float _currentVerticalRotation = 0f;
    private void RotatePlayer(Vector2 lookInput)
    {
        // Calculate the new vertical rotation
        float newVerticalRotation = _currentVerticalRotation + (-lookInput.y * _sensitivity * _tickRate);

        // Clamp the new vertical rotation
        newVerticalRotation = Mathf.Clamp(newVerticalRotation, -90f, 90f);

        // Calculate the actual rotation difference after clamping
        float verticalRotationDifference = newVerticalRotation - _currentVerticalRotation;

        // Update the current vertical rotation
        _currentVerticalRotation = newVerticalRotation;

        // Apply the rotations
        _vcamTransform.RotateAround(_vcamTransform.position, _vcamTransform.right, verticalRotationDifference);
        transform.RotateAround(transform.position, transform.up, lookInput.x * _sensitivity * _tickRate);
    }
    [ServerRpc]
    private void MovePlayerServerRpc(int tick, Vector2 movementInput, Vector2 lookInput, bool isCrouched, bool isRunning){
        MovePlayer(movementInput, isCrouched, isRunning);
        RotatePlayer(lookInput);

        TransformState state = new TransformState()
        {
            Tick = tick,
            Position = transform.position,
            Rotation = transform.rotation,
            HasStartedMoving = true
        };
        _previousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }
    public void HandleAnimationParams(Vector2 movement, bool isCrouched, bool isRunning)
    {
        if (!isCrouched){
            if (movement.y > 0)
            {
                if (movement.x > 0) // IF walking forward and right or left overtake the forward
                {
                    if (isRunning) ChangeAnimationState(RUN_RIGHT);
                    else ChangeAnimationState(WALK_RIGHT); // walk right
                }
                else if (movement.x < 0)
                {
                    if (isRunning) ChangeAnimationState(RUN_LEFT);
                    else ChangeAnimationState(WALK_LEFT); // walk left
                }
                else
                {
                    if (isRunning) ChangeAnimationState(RUN_FORWARD);
                    else ChangeAnimationState(WALK_FORWARD); // walk forward
                }
            }
            else if (movement.y < 0) // IF walking backward and right or left overtake the backward
            {
                if (movement.x > 0)
                {
                    if (isRunning) ChangeAnimationState(RUN_RIGHT);
                    else ChangeAnimationState(WALK_RIGHT); // walk right
                }
                else if (movement.x < 0)
                {
                    if (isRunning) ChangeAnimationState(RUN_LEFT);
                    else ChangeAnimationState(WALK_LEFT); // walk left
                }
                else
                {
                    if (isRunning) ChangeAnimationState(RUN_BACKWARD);
                    else ChangeAnimationState(WALK_BACKWARD); // walk back
                }
            }
            else if (movement.x > 0) // walk right
            {
                if (isRunning) ChangeAnimationState(RUN_RIGHT);
                else ChangeAnimationState(WALK_RIGHT);
            }
            else if (movement.x < 0) // walk left
            {
                if (isRunning) ChangeAnimationState(RUN_LEFT);
                else ChangeAnimationState(WALK_LEFT);
            }
            else // idle
            {
                ChangeAnimationState(IDLE);
            }
        }
        else{
            if (movement.y > 0)
            {
                if (movement.x > 0) // IF walking forward and right or left overtake the forward
                {
                    ChangeAnimationState(CROUCH_RIGHT); // walk right
                }
                else if (movement.x < 0)
                {
                    ChangeAnimationState(CROUCH_LEFT); // walk left
                }
                else
                {
                    ChangeAnimationState(CROUCH_FORWARD); // walk forward
                }
            }
            else if (movement.y < 0) // IF walking backward and right or left overtake the backward
            {
                if (movement.x > 0)
                {
                    ChangeAnimationState(CROUCH_RIGHT); // walk right
                }
                else if (movement.x < 0)
                {
                    ChangeAnimationState(CROUCH_LEFT); // walk left
                }
                else
                {
                    ChangeAnimationState(CROUCH_BACKWARD); // walk back
                }
            }
            else if (movement.x > 0) // walk right
            {
                ChangeAnimationState(CROUCH_RIGHT);
            }
            else if (movement.x < 0) // walk left
            {
                ChangeAnimationState(CROUCH_LEFT);
            }
            else // idle
            {
                ChangeAnimationState(CROUCH_IDLE);
            }            
        }
    }
    private void ChangeAnimationState(string state){
        if (CurrentAnimState == state) {return;}
        // make sure the state isnt the same
        anim.CrossFadeInFixedTime(state, 0.3f);
        // play animation with a blend time
        CurrentAnimState = state;
        // set the incoming state to currentstate
    }
}