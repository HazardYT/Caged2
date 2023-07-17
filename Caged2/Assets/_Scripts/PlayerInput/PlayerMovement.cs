using Unity.Netcode;
using UnityEngine;
using Cinemachine;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Variables")]
    [SerializeField] private NetworkMovementComponent _playerMovement;
    private CapsuleCollider capsuleCollider;
    public CinemachineVirtualCamera playerVCam;
    public Camera cam;
    public float stamina = 100f;
    private float StaminaRegenTimer = 0.0f;
    private const float StaminaTimeToRegen = 0.5f;
    public float StaminaRegenMultiplier;
    public float StaminaDecreaseMultiplier;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { 
            playerVCam.Priority = 0; 
            cam.GetComponent<AudioListener>().enabled = false; 
            return; 
        }
        playerVCam.Priority = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capsuleCollider = GetComponent<CapsuleCollider>();

    }
    public void Update()
    {
        Vector2 lookInput = UserInput.instance.lookInput;
        Vector2 movementInput = UserInput.instance.moveInput;
        if (IsClient && IsLocalPlayer){
            _playerMovement.ProcessLocalPlayerMovement(movementInput, lookInput);
            print (NetworkManager.LocalClientId + "IS CALLING LOCAL MOVEMENT LLLLLLLLLLL");
        }
        else{
            _playerMovement.ProcessSimulatedPlayerMovement();
            print(NetworkManager.LocalClientId + " IS CALLING SIM PLAYER MOVEMENT SSSSSSSS");
        }

        //Look();
        //Move();
    }
    /*public void Look()
    {
        Vector2 mouseLook = UserInput.instance.lookInput;

        if (mouseLook == Vector2.zero) { return; }

        bool usingMouse = UserInput.instance.currentLookInput is Mouse;
        float sensX = usingMouse ? mouseSensX : controllerSensX;
        float sensY = usingMouse ? mouseSensY : controllerSensY;

        float lookX = mouseLook.x * sensX * Time.fixedDeltaTime;
        float lookY = mouseLook.y * sensY * Time.fixedDeltaTime;

        xRotation -= lookY;
        LookServerRpc(lookX, xRotation);
    }
    [ServerRpc]
    public void LookServerRpc(float x, float xRot){
        xRot = Mathf.Clamp(xRot, -90, 90);
        playerCam.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.Rotate(Vector3.up, x);
    }
    public void Move()
    {   
        Vector2 move = UserInput.instance.moveInput;
        bool isRunning = UserInput.instance.SprintHeld && controller.velocity.magnitude > 0;
        float speed = isRunning ? runningSpeed : walkingSpeed;
        Vector3 movement = move.y * transform.forward + move.x * transform.right;
        if (!controller.isGrounded){
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else verticalVelocity = 0f;
        movement.y = verticalVelocity;
        MoveServerRpc(movement, speed);
        Stamina(isRunning);
    }
    
    [ServerRpc]
    public void MoveServerRpc(Vector3 value, float speed){
        controller.Move(value * speed * Time.deltaTime);
    }
    */
    
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
}
