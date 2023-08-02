using System.Collections;
using UnityEngine;
using Unity.Netcode;


public enum DoorState{

    Opened,
    Closed,
}
public enum Keys{

    RoomKey,
    CellarKey,
}

public class Door : NetworkBehaviour
{
    private NetworkObjectReference networkObjectRef;
    public DoorState doorState = DoorState.Closed;
    public Keys neededKey;
    public Quaternion _closedRotation;
    public Quaternion _openRotation;
    public float _doorOpenSpeed;
    public float _doorCloseSpeed;
    public NetworkVariable<bool> _isLocked = new NetworkVariable<bool>();
    public NetworkVariable<bool> _doorCurrentlyMoving = new NetworkVariable<bool>(false);

    void Start(){
        _closedRotation = transform.localRotation;
        networkObjectRef = new NetworkObjectReference(this.GetComponent<NetworkObject>());
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeDoorStateServerRpc(){
        switch(doorState){
            case DoorState.Opened:
                StartCoroutine(CloseDoor());
                doorState = DoorState.Closed;
                _doorCurrentlyMoving.Value = true;
                break;
            case DoorState.Closed:
                StartCoroutine(OpenDoor());
                doorState = DoorState.Opened;
                _doorCurrentlyMoving.Value = true;
                break;
        }
        UpdateDoorChangeClientRpc(networkObjectRef, doorState);
    }
    IEnumerator OpenDoor(){
        float elapsedTime = 0f;
        while (elapsedTime < _doorOpenSpeed)
        {
            transform.localRotation = Quaternion.Slerp(_closedRotation, _openRotation, elapsedTime / _doorOpenSpeed);
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        _doorCurrentlyMoving.Value = false;
    }
    IEnumerator CloseDoor(){
        float elapsedTime = 0f;
        while (elapsedTime < _doorCloseSpeed){
            transform.localRotation = Quaternion.Slerp(_openRotation, _closedRotation, elapsedTime / _doorCloseSpeed);
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        _doorCurrentlyMoving.Value = false;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UnlockDoorServerRpc(){
        _isLocked.Value = false;
    }
    [ClientRpc]
    public void UpdateDoorChangeClientRpc(NetworkObjectReference networkObjectReference, DoorState state){
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<Door>().doorState = state;

    }
}
