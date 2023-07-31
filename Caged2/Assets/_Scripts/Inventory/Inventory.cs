using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [Header("Slots")]
    public Transform[] _handTransforms;
    public NetworkObject[] _handItems;
    [Header("Variables")]
    private NetworkObjectReference playerNetworkObjectReference;
    public Camera playerCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] public NetworkVariable<int> _selectedSlot = new NetworkVariable<int>(-1);
    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsLocalPlayer){
            playerNetworkObjectReference = new NetworkObjectReference(GetComponent<NetworkObject>());
        }
    }
    private void Update(){
        if (!IsOwner && IsLocalPlayer) return;
        HandleInput();
    }
    private void HandleInput()
    {
        if (UserInput.instance.RightHandPressed){
            if (_handItems[0] != null && _selectedSlot.Value != 0)
                SetSelectedSlotServerRpc(0);
        }
        if (UserInput.instance.LeftHandPressed){
            if (_handItems[1] != null && _selectedSlot.Value != 1)
                SetSelectedSlotServerRpc(1);
        }
        if (UserInput.instance.ThrowHeld){
            
        }
        if (UserInput.instance.ThrowReleased){
            if (_handItems[_selectedSlot.Value] != null){
                ThrowItem();
            }
        }
    }
    public void InteractItem(){
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 5, layerMask))
        {
            if (hit.transform.CompareTag("Item"))
            {
                for (int i = 0; i < _handItems.Length; i++)
                {
                    if (_handItems[i] != null) continue;

                    if (hit.transform.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                    {
                        NetworkObjectReference reference = new NetworkObjectReference(networkObject);
                        StartCoroutine(HandeItemPickup(i, reference));
                        return;
                    }
                    else Debug.LogError("Failed To Get Component [NetworkObject] from Item"); 
                }
                Debug.LogError("Your Hands are Full!");

            }
        }
    }
    private IEnumerator HandeItemPickup(int slot, NetworkObject networkObject){
        NetworkObjectReference networkObjectReference = new NetworkObjectReference(networkObject);
        networkObject.GetComponent<ItemTransform>().ChangeOwnershipServerRpc(networkObjectReference);
        PickupItemServerRpc(networkObjectReference, slot);
        yield return new WaitForEndOfFrame();
        SetItemTransformSlotServerRpc(networkObject.NetworkObjectId, slot);
        SetSelectedSlotServerRpc(slot);
    }
    private void ThrowItem(){
        ThrowItemServerRpc(playerCamera.transform.forward);
        int slot = _selectedSlot.Value == 0 ? 1 : 0;
        SetItemTransformSlotServerRpc(_handItems[_selectedSlot.Value].NetworkObjectId, -1);
        SetSelectedSlotServerRpc(slot);
        NetworkObject networkObject = _handItems[_selectedSlot.Value].GetComponent<NetworkObject>();
        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = false;

        networkObject.transform.position = playerCamera.transform.forward;

        pickUpObjectRigidbody.AddForce(playerCamera.transform.forward * 2, ForceMode.Impulse);

    }

    [ServerRpc]
    public void PickupItemServerRpc(NetworkObjectReference networkObjectReference, int slot, ServerRpcParams serverRpcParams = default){
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();

        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!networkObject.TrySetParent(transform, false)) { Debug.LogError("Failed to Parent Item: TrySetParent Failed."); return; }

        NetworkObjectReference playerReference = new NetworkObjectReference(playerNetworkObject);
        UpdateClientsOnItemChangeClientRpc(playerReference, networkObjectReference, slot);
        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;

    }
    [ClientRpc]
    public void UpdateClientsOnItemChangeClientRpc(NetworkObjectReference playerReference, NetworkObjectReference objectReference, int slot){

        if (!objectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!playerReference.TryGet(out NetworkObject playerNetworkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        Inventory inv = playerNetworkObject.GetComponent<Inventory>();
        inv._handItems[slot] = networkObject;
    }
    [ServerRpc]
    public void ThrowItemServerRpc(Vector3 Direction, ServerRpcParams rpcParams = default){
        NetworkObject playerNetworkObject = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        NetworkObject networkObject = inventory._handItems[inventory._selectedSlot.Value].GetComponent<NetworkObject>();
        if (!networkObject.TryRemoveParent(true))
        {
            Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
            return;
        }
       // Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        //pickUpObjectRigidbody.isKinematic = false;

        //networkObject.transform.position = Direction;

        //pickUpObjectRigidbody.AddForce(Direction * 2, ForceMode.Impulse);

        NetworkObjectReference playerNetworkObjectReference = new NetworkObjectReference(playerNetworkObject);

        DropSelectedItemClientRpc(playerNetworkObjectReference);



    }
    [ServerRpc]
    public void SetSelectedSlotServerRpc(int slot, ServerRpcParams serverRpcParams = default){
        NetworkObject networkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = networkObject.GetComponent<Inventory>();
        inventory._selectedSlot.Value = slot;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetItemTransformSlotServerRpc(ulong id, int slot)
    {
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.Slot.Value = slot;
        print(slot);
    }
    [ClientRpc]
    public void DropSelectedItemClientRpc(NetworkObjectReference senderNetworkObjectReference){
        if (!senderNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed.");
            return;
        }
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        inventory._handItems[_selectedSlot.Value] = null;

    }
}
