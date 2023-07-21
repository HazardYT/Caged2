using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public enum ControlScheme
{
    KeyboardMouse,
    Controller
}

public class UserInput : NetworkBehaviour
{
    public static UserInput instance;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool CrouchReleased { get; private set; }
    public bool RightHandPressed { get; private set; }
    public bool LeftHandPressed { get; private set; }
    public bool Slot3Pressed { get; private set; }
    public bool RightBumperPressed { get; private set; }
    public bool LeftBumperPressed { get; private set; }
    public bool ThrowHeld { get; private set; }
    public bool ThrowReleased { get; private set; }
    public bool InteractPressed { get; private set; }

    public ControlScheme currentInputDevice = ControlScheme.KeyboardMouse;
    private PlayerInput _playerInput;
    private InputAction _sprintAction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _crouchAction;
    private InputAction _rightHandAction;
    private InputAction _leftHandAction;
    private InputAction _slot3Action;
    private InputAction _rightBumperAction;
    private InputAction _leftBumperAction;
    private InputAction _throwAction;
    private InputAction _interactAction;

    public override void OnNetworkSpawn()
    {
        //if (!IsOwner) return;
        if (instance == null) { instance = this; }

        _playerInput = GetComponent<PlayerInput>();
        SetupInputActions();
    }
    private void Update()
    {
        //if (!IsOwner) return;
        UpdateInputs();
    }
    public void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _sprintAction = _playerInput.actions["Sprint"];
        _crouchAction = _playerInput.actions["Crouch"];
        _leftHandAction = _playerInput.actions["RightHand"];
        _rightHandAction = _playerInput.actions["LeftHand"];
        _slot3Action = _playerInput.actions["Slot3"];
        _rightBumperAction = _playerInput.actions["RightBumper"];
        _leftBumperAction = _playerInput.actions["LeftBumper"];
        _throwAction = _playerInput.actions["Throw"];
        _interactAction = _playerInput.actions["Interact"];
    }
    private void UpdateInputs()
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        lookInput = _lookAction.ReadValue<Vector2>();
        SprintHeld = _sprintAction.IsPressed();
        CrouchHeld = _crouchAction.IsPressed();
        CrouchReleased = _crouchAction.WasReleasedThisFrame();
        RightHandPressed = _rightHandAction.WasPressedThisFrame();
        LeftHandPressed = _leftHandAction.WasPressedThisFrame();
        Slot3Pressed = _slot3Action.WasPressedThisFrame();
        ThrowHeld = _throwAction.IsPressed();
        ThrowReleased = _throwAction.WasReleasedThisFrame();
        InteractPressed = _interactAction.WasPressedThisFrame();
        /*if( moveInput != Vector2.zero && _moveAction.activeControl.device is Keyboard){
            currentInputDevice = ControlScheme.KeyboardMouse;
        }
        else{
            currentInputDevice = ControlScheme.Controller;
        }*/
    }
}
