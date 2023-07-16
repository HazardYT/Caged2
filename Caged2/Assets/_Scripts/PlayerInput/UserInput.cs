using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
public class UserInput : NetworkBehaviour
{
    public static UserInput instance;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool CrouchReleased { get; private set; }


    public InputDevice currentLookInput = Mouse.current;
    private PlayerInput _playerInput;
    private InputAction _sprintAction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _crouchAction;


    public override void OnNetworkSpawn()
    {
        //if (!IsOwner) return;
        if (instance == null) { instance = this; }
        _playerInput = GetComponent<PlayerInput>();
        SetupInputActions();
    }
    void Start(){
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
        if (_lookAction.ReadValue<Vector2>() != Vector2.zero) 
        { currentLookInput = _lookAction.activeControl.device; } else return;
    }
    private void UpdateInputs()
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        lookInput = _lookAction.ReadValue<Vector2>();
        SprintHeld = _sprintAction.IsPressed();
        CrouchHeld = _crouchAction.IsPressed();
        CrouchReleased = _crouchAction.WasReleasedThisFrame();

        if (lookInput != Vector2.zero && currentLookInput != _lookAction.activeControl.device) 
        { currentLookInput = _lookAction.activeControl.device; } else return;
    }
}
