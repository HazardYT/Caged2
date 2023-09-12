using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Safe : NetworkBehaviour, IInteractable
{
    public int safeCode;
    public DoorState safeState = DoorState.Closed;
    private NetworkObjectReference networkObjectRef;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private Canvas safeCanvas;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private float _safeSpeed;
    [SerializeField] private Quaternion _safeOpenRotation;
    [SerializeField] private Quaternion _safeCloseRotation;
    public NetworkVariable<bool> _safeInteracting = new();
    public NetworkVariable<bool> _safeCurrentlyMoving = new();
    public NetworkVariable<bool> _safeLocked = new();
    
    void Start(){
        _safeCloseRotation = transform.localRotation;
        networkObjectRef = new NetworkObjectReference(GetComponent<NetworkObject>());
    }
    public void Interact(RaycastHit hit, Inventory inventory){
        if (!_safeCurrentlyMoving.Value && !_safeLocked.Value){
            ChangeDoorStateServerRpc();
        }
        else if (!_safeCurrentlyMoving.Value && _safeLocked.Value){
            OpenCodeInterface();
            InteractingServerRpc(true);
        }
    }
    [ServerRpc]
    public void InteractingServerRpc(bool i, ServerRpcParams serverRpcParams = default){
        NetworkObject networkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        networkObject.GetComponent<PlayerMovement>().movementLocked.Value = i;
    }
    void OpenCodeInterface(){
        safeCanvas.enabled = true;
        eventSystem.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseCodeInterface(){
        safeCanvas.enabled = false;
        eventSystem.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InteractingServerRpc(false);
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeDoorStateServerRpc(){
        switch(safeState){
            case DoorState.Opened:
                StartCoroutine(CloseSafe());
                safeState = DoorState.Closed;
                _safeCurrentlyMoving.Value = true;
                break;
            case DoorState.Closed:
                StartCoroutine(OpenSafe());
                safeState = DoorState.Opened;
                _safeCurrentlyMoving.Value = true;
                break;
        }
        UpdateDoorChangeClientRpc(networkObjectRef, safeState);
    }
    public void ButtonPress(string i){
        if (codeText.text.Length < 4){
            codeText.text += i;
        }
    }
    public void VerifyCode(){
        int code = int.Parse(codeText.text);
        if (code != safeCode){
            ResetScreen();
        }
        else {
            ChangeDoorStateServerRpc();
            UnlockSafeServerRpc();
            InteractingServerRpc(false);
            CloseCodeInterface();
        }
    }
    public void ResetScreen(){
        codeText.text = "";
    }
    [ServerRpc]
    public void UnlockSafeServerRpc(){
        _safeLocked.Value = false;
    }
    IEnumerator OpenSafe(){
        float elapsedTime = 0f;
        while (elapsedTime < _safeSpeed)
        {
            transform.localRotation = Quaternion.Slerp(_safeCloseRotation, _safeOpenRotation, elapsedTime / _safeSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        _safeCurrentlyMoving.Value = false;
    }
    IEnumerator CloseSafe(){
        float elapsedTime = 0f;
        while (elapsedTime < _safeSpeed)
        {
            transform.localRotation = Quaternion.Slerp(_safeOpenRotation, _safeCloseRotation, elapsedTime / _safeSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        _safeCurrentlyMoving.Value = false;
    }
    [ClientRpc]
    public void UpdateDoorChangeClientRpc(NetworkObjectReference networkObjectReference, DoorState state){
        networkObjectReference.TryGet(out NetworkObject networkObject);
        Safe safe = networkObject.GetComponent<Safe>();
        safe.safeState = state;
        safe.CloseCodeInterface();
    }
}
