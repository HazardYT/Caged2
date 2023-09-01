using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [SerializeField] private bool allowTwoSlots;
    [SerializeField] private Transform[] inventorySlots;
    [SerializeField] private Transform[] inventoryPositions;
    [SerializeField] private Transform[] handTracking;
    private NetworkVariable<int> selectedSlot = new(0);

    private void Update(){
        if (!IsOwner && IsLocalPlayer) return;
        HandleInput();
        HandleTracking();
    }
    private void HandleTracking(){
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null){
                //if (IsServer){
                    inventorySlots[i].SetPositionAndRotation(handTracking[i].position, handTracking[i].rotation);
                //}
                //else{
                    //NetworkObjectReference networkObjectReference = new(inventorySlots[i].GetComponent<NetworkObject>());
                  //  HandleTrackingClientRpc(networkObjectReference, handTracking[i].position, handTracking[i].rotation);
                //}
            }
            
        }
    }
    [ClientRpc]
    public void HandleTrackingClientRpc(NetworkObjectReference reference, Vector3 position, Quaternion rotation){
        reference.TryGet(out NetworkObject networkObject);
        networkObject.transform.SetPositionAndRotation(position, rotation);
    }
    private void HandleInput()
    {
        if (UserInput.instance.RightHandPressed){
            if (inventorySlots[0] != null && selectedSlot.Value != 0){

            }

        }
        if (UserInput.instance.LeftHandPressed && inventorySlots.Length > 0){ 
            if (inventorySlots[1] != null && selectedSlot.Value != 1){

            }

        }
        if (UserInput.instance.DropPressed){
            if (inventorySlots[selectedSlot.Value] != null){

            }
        }
        if (UserInput.instance.ThrowHeld){
            
        }
        if (UserInput.instance.ThrowReleased){
        }
    }
    public void Interact(RaycastHit hit){
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null) continue;

            if (hit.transform.TryGetComponent(out NetworkObject networkObject))
            {
                NetworkObjectReference reference = new(networkObject);
                PickupItemServerRpc(reference, i);
                return;
            }
            else Debug.LogError("Failed To Get Component [NetworkObject] from Item"); 
        }
        Debug.LogError("Your Hands are Full!");
    }
    [ServerRpc]
    public void PickupItemServerRpc(NetworkObjectReference networkObjectReference, int slot, ServerRpcParams serverRpcParams = default){

        networkObjectReference.TryGet(out NetworkObject networkObject);

        Transform playerTransform = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.transform;
        
        GameObject spawnedObject = Instantiate(networkObject.gameObject, playerTransform.GetComponent<Inventory>().inventoryPositions[slot].position, quaternion.identity);

        NetworkObject spawnedObjectNetworkObject = spawnedObject.GetComponent<NetworkObject>();

        spawnedObjectNetworkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

        spawnedObjectNetworkObject.TrySetParent(playerTransform);

        playerTransform.GetComponent<Inventory>().inventorySlots[slot] = spawnedObject.transform;

        networkObject.Despawn();



    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    /*[Header("Slots")]
    public Transform[] _handTransforms;
    public NetworkObject[] _handItems;
    [Header("Variables")]

    public CinemachineVirtualCamera playerCamera;
    [SerializeField] private LayerMask layerMask;
    public NetworkVariable<int> selectedInventorySlot = new(-1);

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
        if (UserInput.instance.LeftHandPressed && _handItems.Length > 0){ 
            if (_handItems[1] != null && _selectedSlot.Value != 1)
                SetSelectedSlotServerRpc(1);
        }
        if (UserInput.instance.ThrowHeld){
            
        }
        if (UserInput.instance.ThrowReleased){
            if (_handItems[_selectedSlot.Value] != null){
                ThrowItem(_handItems[_selectedSlot.Value]);
            }
        }
    }
    public void Interact(RaycastHit hit){
        for (int i = 0; i < _handItems.Length; i++)
        {
            if (_handItems[i] != null) continue;

            if (hit.transform.TryGetComponent(out NetworkObject networkObject))
            {
                NetworkObjectReference reference = new(networkObject);
                StartCoroutine(HandeItemPickup(i, reference));
                return;
            }
            else Debug.LogError("Failed To Get Component [NetworkObject] from Item"); 
        }
        Debug.LogError("Your Hands are Full!");
    }
    public NetworkObject SearchInventoryForItem(string name){
        for (int i = 0; i < _handItems.Length; i++)
        {
            if (_handItems[i].transform.name == name){
                return _handItems[i];
            }
        }
        return null;
    }
    private IEnumerator HandeItemPickup(int slot, NetworkObject networkObject){
        NetworkObjectReference networkObjectReference = new(networkObject);
        PickupItemServerRpc(networkObjectReference, slot);
        yield return new WaitForEndOfFrame();
        SetSelectedSlotServerRpc(slot);
        SetItemTransformSlotServerRpc(networkObject.NetworkObjectId, slot);
    }
    private void ThrowItem(NetworkObject networkObject){
        ThrowItemServerRpc(playerCamera.transform.position, playerCamera.transform.forward);
        SetSelectedSlotServerRpc(_selectedSlot.Value == 0 ? 1 : 0);
        SetItemTransformSlotServerRpc(networkObject.NetworkObjectId, -1);
    }

    [ServerRpc]
    public void PickupItemServerRpc(NetworkObjectReference networkObjectReference, int slot, ServerRpcParams serverRpcParams = default){
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!networkObject.TrySetParent(transform, false)) { Debug.LogError("Failed to Parent Item: TrySetParent Failed."); return; }

        NetworkObjectReference playerReference = new(playerNetworkObject);
        UpdateClientsOnItemChangeClientRpc(playerReference, networkObjectReference, slot);
        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.None;
    }
    [ClientRpc]
    public void UpdateClientsOnItemChangeClientRpc(NetworkObjectReference playerReference, NetworkObjectReference objectReference, int slot){

        if (!objectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        if (!playerReference.TryGet(out NetworkObject playerNetworkObject)) { Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed."); return; }

        Inventory inv = playerNetworkObject.GetComponent<Inventory>();
        inv._handItems[slot] = networkObject;
    }
    [ServerRpc]
    public void ThrowItemServerRpc(Vector3 Direction, Vector3 Forward, ServerRpcParams rpcParams = default)
    {
        Vector3 position = Direction + Forward;
        NetworkObject playerNetworkObject = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
        Inventory inventory = playerNetworkObject.GetComponent<Inventory>();
        NetworkObject networkObject = inventory._handItems[inventory._selectedSlot.Value].GetComponent<NetworkObject>();
        if (!networkObject.TryRemoveParent())
        {
            Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
            return;
        }

        Rigidbody pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = false;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        NetworkObjectReference playerNetworkObjectReference = new(playerNetworkObject);
        DropSelectedItemClientRpc(playerNetworkObjectReference);

        // Set the position of the item immediately after removing the parent
        networkObject.transform.position = position;
        networkObject.transform.rotation = Quaternion.identity;
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
*/}
