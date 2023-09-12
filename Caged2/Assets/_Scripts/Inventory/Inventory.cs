using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class Inventory : NetworkBehaviour
{
    public Transform[] inventorySlots;
    [SerializeField] private Transform[] inventoryPositions;
    [SerializeField] private Transform[] handTracking;
    [SerializeField] private Transform dropPosition;
    [SerializeField] private Transform playerCam;
    public InventoryVisuals visuals;
    public NetworkVariable<int> selectedSlot = new(0);

    private void Update(){
        if (!IsOwner && IsLocalPlayer) return;
        HandleInput();
    }
    private void HandleInput()
    {
        if (UserInput.instance.RightHandPressed){
            if (inventorySlots[0] != null && selectedSlot.Value != 0){
                SelectSlotServerRpc(0);
                visuals.SelectSlot(0);
            }

        }
        if (UserInput.instance.LeftHandPressed && inventorySlots.Length > 0){ 
            if (inventorySlots[1] != null && selectedSlot.Value != 1){
                SelectSlotServerRpc(1);
                visuals.SelectSlot(1);
            }

        }
        if (UserInput.instance.DropPressed){
            if (inventorySlots[selectedSlot.Value] != null){
                NetworkObjectReference networkObjectReference = new(inventorySlots[selectedSlot.Value].GetComponent<NetworkObject>());
                visuals.RemoveItem(selectedSlot.Value);
                DropItemServerRpc(networkObjectReference);
                int nextIndex = (selectedSlot.Value + 1) % inventorySlots.Length;
                if (inventorySlots[nextIndex] != null){
                    SelectSlotServerRpc(nextIndex);
                    visuals.SelectSlot(nextIndex);
                }
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
                visuals.PickupItem(networkObject, i);
                visuals.SelectSlot(i);
                return;
            }
            else Debug.LogError("Failed To Get Component [NetworkObject] from Item"); 
        }
        Debug.LogError("Your Hands are Full!");
    }
    public bool HasCorrectKey(KeyList key, int slot){
        if (inventorySlots[slot] == null){
            return false; }
        if (inventorySlots[slot].TryGetComponent(out Key invKey)){
            if (key == invKey.KeyType){
                return true;
            }
            return false;
        }
        return false;
    }
    [ServerRpc]
    public void RemoveItemFromSlotServerRpc(int slot, ServerRpcParams serverRpcParams = default){
        if (inventorySlots[slot] != null){
            NetworkObjectReference playerReference = new(NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject);
            inventorySlots[slot].GetComponent<NetworkObject>().Despawn();
            RemoveItemFromSlotClientRpc(playerReference, slot);
        }
    }
    [ClientRpc]
    public void RemoveItemFromSlotClientRpc(NetworkObjectReference playerReference, int slot){
        playerReference.TryGet(out NetworkObject networkPlayer);
        networkPlayer.GetComponent<Inventory>().inventorySlots[slot] = null;  
    }
    [ServerRpc]
    public void PickupItemServerRpc(NetworkObjectReference networkObjectReference, int slot, ServerRpcParams serverRpcParams = default){

        networkObjectReference.TryGet(out NetworkObject networkObject);

        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        
        GameObject spawnedObject = Instantiate(networkObject.gameObject, playerNetworkObject.GetComponent<Inventory>().inventoryPositions[slot].position, Quaternion.identity);

        spawnedObject.name = networkObject.name;

        NetworkObject spawnedObjectNetworkObject = spawnedObject.GetComponent<NetworkObject>();

        spawnedObjectNetworkObject.Spawn();

        networkObject.Despawn();

        spawnedObjectNetworkObject.TrySetParent(playerNetworkObject.transform);
        
        playerNetworkObject.GetComponent<Inventory>().selectedSlot.Value = slot;

        NetworkObjectReference playerReference = new(playerNetworkObject);
        NetworkObjectReference objectReference = new(spawnedObjectNetworkObject);

        PickupItemClientRpc(playerReference, objectReference, slot);
    }
    [ClientRpc]
    public void PickupItemClientRpc(NetworkObjectReference playerReference, NetworkObjectReference objectReference, int slot){
        objectReference.TryGet(out NetworkObject networkObject);
        playerReference.TryGet(out NetworkObject networkPlayer);
        Inventory inventory = networkPlayer.GetComponent<Inventory>();
        inventory.inventorySlots[slot] = networkObject.transform;
        if (networkObject.TryGetComponent(out ParentConstraint parentConstraint))
        {
            ConstraintSource constraintSource = new()
            {
                sourceTransform = inventory.inventoryPositions[slot],
                weight = 1
            };
            parentConstraint.AddSource(constraintSource);
        }   
    }
    [ServerRpc]
    public void DropItemServerRpc(NetworkObjectReference objectReference, ServerRpcParams serverRpcParams = default){
        objectReference.TryGet(out NetworkObject networkObject);

        NetworkObject networkPlayer = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

        GameObject spawnedObject = Instantiate(networkObject.gameObject, networkPlayer.GetComponent<Inventory>().dropPosition.position, networkPlayer.GetComponent<Inventory>().dropPosition.rotation);

        spawnedObject.name = networkObject.name;

        NetworkObject spawnedNetworkObject = spawnedObject.GetComponent<NetworkObject>();

        spawnedNetworkObject.Spawn();

        networkObject.Despawn();

        NetworkObjectReference playerReference = new(networkPlayer);
        NetworkObjectReference newobjectReference = new(spawnedNetworkObject);

        DropItemClientRpc(playerReference, newobjectReference);
    }
    [ClientRpc]
    public void DropItemClientRpc(NetworkObjectReference playerReference, NetworkObjectReference objectReference){
        objectReference.TryGet(out NetworkObject networkObject);
        playerReference.TryGet(out NetworkObject networkPlayer);

        Inventory inventory = networkPlayer.GetComponent<Inventory>();

        Rigidbody rb = networkObject.GetComponent<Rigidbody>();       

        inventory.inventorySlots[inventory.selectedSlot.Value] = null;

        networkObject.GetComponent<ParentConstraint>().RemoveSource(0);

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false;
    }
    [ServerRpc]
    public void SelectSlotServerRpc(int slot, ServerRpcParams serverRpcParams = default){
        NetworkObject networkPlayer = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
        networkPlayer.GetComponent<Inventory>().selectedSlot.Value = slot;
    }
}
