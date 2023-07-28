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
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private NetworkVariable<int> _selectedSlot = new NetworkVariable<int>(-1);
    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsLocalPlayer){
            playerNetworkObjectReference = new NetworkObjectReference(GetComponent<NetworkObject>());
        }
    }
    private void Update(){
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
                ThrowItemServerRpc();
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
        PickupItemServerRpc(networkObjectReference, slot);
        yield return new WaitForEndOfFrame();
        SetItemTransformSlotServerRpc(networkObject.NetworkObjectId, slot);
    }

    [ServerRpc]
    private void PickupItemServerRpc(NetworkObjectReference networkObjectReference, int slot, ServerRpcParams serverRpcParams = default){
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();

        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!networkObject.TrySetParent(transform, false)) { Debug.LogError("Failed to Parent Item: TrySetParent Failed."); return; }

        NetworkObjectReference playerReference = new NetworkObjectReference(playerNetworkObject);
        UpdateClientsOnItemChangeClientRpc(playerReference, networkObjectReference, slot);
        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.None;

    }
    [ClientRpc]
    private void UpdateClientsOnItemChangeClientRpc(NetworkObjectReference playerReference, NetworkObjectReference objectReference, int slot){

        if (!objectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!playerReference.TryGet(out NetworkObject playerNetworkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        Inventory inv = playerNetworkObject.GetComponent<Inventory>();
        inv._handItems[slot] = networkObject;
        inv.SetSelectedSlotServerRpc(slot);
    }
    [ServerRpc]
    private void ThrowItemServerRpc(ServerRpcParams rpcParams = default){
        NetworkObject playerNetworkObject = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        NetworkObject networkObject = inventory._handItems[inventory._selectedSlot.Value].GetComponent<NetworkObject>();
        Camera camera = inventory.playerCamera;
        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        int i = inventory._selectedSlot.Value == 0 ? 1 : 0;
        if (!networkObject.TryRemoveParent(true))
        {
            Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
            return;
        }
        pickUpObjectRigidbody.isKinematic = false;

        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        networkObject.transform.position = camera.transform.position;

        pickUpObjectRigidbody.AddForce(camera.transform.forward * 2, ForceMode.Impulse);

        NetworkObjectReference playerNetworkObjectReference = new NetworkObjectReference(playerNetworkObject);

        DropSelectedItemClientRpc(playerNetworkObjectReference);

        SetItemTransformSlotServerRpc(networkObject.NetworkObjectId, -1);

        SelectItemBySlotClientRpc(_selectedSlot.Value, playerNetworkObjectReference, false);

        SetSelectedSlotServerRpc(i, false);

    }
    [ServerRpc]
    private void SetSelectedSlotServerRpc(int slot, bool n = true, ServerRpcParams serverRpcParams = default){
        NetworkObject networkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = networkObject.GetComponent<Inventory>();
        inventory._selectedSlot.Value = slot;
        NetworkObjectReference reference = new NetworkObjectReference(networkObject);
        if (n)
        SelectItemBySlotClientRpc(slot, reference);
    }
    [ServerRpc]
    private void SetItemTransformSlotServerRpc(ulong id, int slot)
    {
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.Slot.Value = slot;
        print(slot);
    }
    [ClientRpc]
    private void SelectItemBySlotClientRpc(int Value, NetworkObjectReference networkObjectReference, bool other = true){
        if (!networkObjectReference.TryGet(out NetworkObject playerNetworkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        for (int i = 0; i < inventory._handItems.Length; i++)
        {
            if (other){
                if (i == Value){
                    inventory._handItems[i].transform.localScale *= 1.15f;
                }
                if (i != Value && inventory._handItems[i] != null){
                    inventory._handItems[i].transform.localScale /= 1.15f;
                }       
            }
            else
            {
                if (inventory._handItems[i] != null){
                    inventory._handItems[i].transform.localScale *= 1.15f;
                }
            }
        }
    }
    [ClientRpc]
    public void DropSelectedItemClientRpc(NetworkObjectReference senderNetworkObjectReference){
        if (!senderNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed.");
            return;
        }
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        inventory._handItems[_selectedSlot.Value].transform.localScale /= 1.15f;
        inventory._handItems[_selectedSlot.Value] = null;

    }
}
