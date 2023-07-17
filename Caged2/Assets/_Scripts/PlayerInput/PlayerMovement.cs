using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Variables")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _mouseSensX;
    [SerializeField] private float _mouseSensY;
    [SerializeField] private float _controllerSensX;
    [SerializeField] private float _controllerSensY;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator anim;
    private CapsuleCollider capsuleCollider;
    public Transform playerCam;
    public float stamina = 100f;
    private float StaminaRegenTimer = 0.0f;
    private const float StaminaTimeToRegen = 0.5f;
    public float StaminaRegenMultiplier;
    public float StaminaDecreaseMultiplier;
    private string CurrentAnimState;
    float xRotation;
    float verticalVelocity = 0;
    private float gravity = 9.61f;

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

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = playerCam.gameObject.GetComponent<CinemachineVirtualCamera>();
        if (!IsOwner) { 
        cvm.Priority = 0; 
        playerCam.parent.GetComponentInChildren<AudioListener>().enabled = false; 
        return; }
        cvm.Priority = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capsuleCollider = GetComponent<CapsuleCollider>();

    }
    public void Update()
    {
        Vector2 lookInput = UserInput.instance.lookInput;
        Vector2 movementInput = UserInput.instance.moveInput;
        Look();
        Move();
    }
    public void Look()
    {
        Vector2 mouseLook = UserInput.instance.lookInput;

        if (mouseLook == Vector2.zero) { return; }

        bool usingMouse = UserInput.instance.currentLookInput is Mouse;
        float sensX = usingMouse ? _mouseSensX : _controllerSensX;
        float sensY = usingMouse ? _mouseSensY : _controllerSensY;

        float lookX = mouseLook.x * sensX * Time.deltaTime;
        float lookY = mouseLook.y * sensY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up, lookX);
    }
    public void Move()
    {   
        Vector2 move = UserInput.instance.moveInput;
        bool isRunning = UserInput.instance.SprintHeld && controller.velocity.magnitude > 0 && !UserInput.instance.CrouchHeld;
        bool isCrouched = UserInput.instance.CrouchHeld && !UserInput.instance.SprintHeld;
        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 movement = move.y * transform.forward + move.x * transform.right;
        if (!controller.isGrounded){
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else verticalVelocity = 0f;
        movement.y = verticalVelocity;
        controller.Move(movement * speed * Time.deltaTime);
        Stamina(isRunning);
        HandleAnimationParams(move, isCrouched, isRunning);
    }
    
    public void Stamina(bool isRunning)
    {
        if (isRunning)
        {
            stamina = Mathf.Clamp(stamina - (StaminaDecreaseMultiplier * Time.deltaTime), 0.0f, 100f);
            StaminaRegenTimer = 0.0f;
        }
        else if (stamina < 100f)
        {
            if (StaminaRegenTimer >= StaminaTimeToRegen)
            {
                stamina = Mathf.Clamp(stamina + (StaminaRegenMultiplier * Time.deltaTime), 0.0f, 100f);
            }
            else
            {
                StaminaRegenTimer += Time.deltaTime;
            }
        }
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
        anim.CrossFadeInFixedTime(state, 10 * Time.fixedDeltaTime);
        // play animation with a blend time
        CurrentAnimState = state;
        // set the incoming state to currentstate
    }
}
