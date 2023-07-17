using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkMovementComponent : NetworkBehaviour
{
    [SerializeField] private CharacterController _cc;

    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _sensitivity;

    [SerializeField] private Transform _camSocket;
    [SerializeField] private Transform _vcam;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Color _color;
    private Transform _vcamTransform;

    public float _tickDeltaTime = 0;
    private int _tick = 0;
    private float ServerTickRate = 90f;
    private float _tickRate;
    private const int BUFFER_SIZE = 1024;
    private PlayerMovement _playerMovement;
    private InputState[] _inputStates = new InputState[BUFFER_SIZE];
    private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];
    public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
    public TransformState _previousTransformState;
    private float _positionLerpSpeed = 12f;
    private float _rotationLerpSpeed = 12f;

    private int _lastProcessedTick = -0;
    void Start(){
        _tickRate = 1f / ServerTickRate;
    }
    private void OnEnable(){
        ServerTransformState.OnValueChanged += OnServerStateChanged;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _playerMovement = GetComponent<PlayerMovement>();
        _vcamTransform = _vcam.transform;
    }
    private void OnServerStateChanged(TransformState previousValue, TransformState serverState)
    {
        if (!IsLocalPlayer) return;

        if (_previousTransformState == null)
        {
            _previousTransformState = serverState;
        }

        TransformState calculatedState = _transformStates.First(localState => localState.Tick == serverState.Tick);
        if (calculatedState.Position != serverState.Position)
        {
            Debug.Log("Correcting Client Position");
            
            TeleportPlayer(serverState);

            IEnumerable<InputState> inputs = _inputStates.Where(input => input.Tick > serverState.Tick);

            inputs = from input in inputs orderby input.Tick select input;

            foreach(InputState inputState in inputs)
            {
                MovePlayer(inputState.movementInput, inputState.crouchInput, inputState.sprintInput);
                RotatePlayer(inputState.lookInput);

                TransformState newTransformState = new TransformState(){
                    Tick = inputState.Tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true,
                    isCrouching = inputState.crouchInput,
                    isRunning = inputState.sprintInput
                };

                for (int i = 0; i < _transformStates.Length; i++)
                {
                    if (_transformStates[i].Tick == inputState.Tick){
                        _transformStates[i] = newTransformState;
                        break;
                    }
                }
            }
        }
    }
    private void TeleportPlayer(TransformState state)
    {
        _cc.enabled = false;
        transform.position = state.Position;
        transform.rotation = state.Rotation;
        _cc.enabled = true;

        for (int i = 0; i < _transformStates.Length; i++)
        {
            if (_transformStates[i].Tick == state.Tick)
            {
                _transformStates[i] = state;
                break;
            }
        }
    }
    public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput, bool isCrouched = false, bool isRunning = false){
        _tickDeltaTime += Time.deltaTime;
        if (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;

            if (!IsServer){
                MovePlayerServerRpc(_tick, movementInput, lookInput, isCrouched, isRunning);
                MovePlayer(movementInput, isCrouched, isRunning);
                RotatePlayer(lookInput);
                SaveState(movementInput, lookInput, isCrouched, isRunning, bufferIndex);
            }
            else
            {
                MovePlayer(movementInput, isCrouched, isRunning);
                RotatePlayer(lookInput);

                TransformState state = new TransformState(){
                    Tick = _tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true,
                    isCrouching = isCrouched,
                    isRunning = isRunning
                };

                SaveState(movementInput, lookInput, isCrouched, isRunning, bufferIndex);
                
                _previousTransformState = ServerTransformState.Value;
                ServerTransformState.Value = state;
            }

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
        private void SaveState(Vector2 movementInput, Vector2 lookInput, bool crouchInput, bool runningInput, int bufferIndex)
        {
            InputState inputState = new InputState()
            {
                Tick = _tick,
                movementInput = movementInput,
                lookInput = lookInput,
                crouchInput = crouchInput,
                sprintInput = runningInput
                
            };

            TransformState transformState = new TransformState()
            {
                Tick = _tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true,
                isCrouching = crouchInput,
                isRunning = runningInput
            };

            _inputStates[bufferIndex] = inputState;
            _transformStates[bufferIndex] = transformState;
        }
    private void MovePlayer(Vector2 movementInput, bool isCrouched = false, bool isRunning = false)
    {
        _playerMovement.HandleAnimationParams(movementInput, isCrouched, isRunning);
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

        if (_lastProcessedTick + 1 != tick)
        {
            Debug.Log("I missed a tick");
            Debug.Log($"Received Tick {tick}");
        }
        _lastProcessedTick = tick;
        MovePlayer(movementInput, isCrouched, isRunning);
        RotatePlayer(lookInput);

        TransformState state = new TransformState()
        {
            Tick = tick,
            Position = transform.position,
            Rotation = transform.rotation,
            HasStartedMoving = true,
            isCrouching = isCrouched,
            isRunning = isRunning
        };
        _previousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }

    private void OnDrawGizmos(){
        if (ServerTransformState.Value != null){
            Gizmos.color = _color;
            Gizmos.DrawMesh(_meshFilter.mesh, ServerTransformState.Value.Position);
        }
    }

}