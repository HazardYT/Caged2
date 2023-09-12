using Unity.Netcode;
using UnityEngine;

public class KeyLock : NetworkBehaviour, IInteractable
{
    [SerializeField] private NetworkBehaviour interactable;
    [SerializeField] private KeyList neededKey;
    public NetworkVariable<bool> IsLocked = new(true);
    public void Interact(RaycastHit hit, Inventory inventory)
    {
        if (IsLocked.Value) { 
            Debug.LogError("Door is Locked");
            for (int i = 0; i < inventory.inventorySlots.Length; i++)
            {
                if (inventory.HasCorrectKey(neededKey, i)){
                    NetworkObjectReference networkObjectReference = new(hit.transform.GetComponent<NetworkObject>());
                    UnlockKeyLockServerRpc(networkObjectReference);
                    int nextIndex = (inventory.selectedSlot.Value + 1) % inventory.inventorySlots.Length;
                    inventory.RemoveItemFromSlotServerRpc(i);
                    inventory.visuals.RemoveItem(i);
                    if (inventory.inventorySlots[nextIndex] != null){
                    inventory.SelectSlotServerRpc(nextIndex);
                    inventory.visuals.SelectSlot(nextIndex);
                    }
                    Debug.Log("Door is Unlocked");
                }
            }
        }
        else{
            // Check if the interactable object has the IInteractable interface
            IInteractable interactableComponent = interactable as IInteractable;
            interactableComponent?.Interact(hit);
        }

    }
    [ServerRpc]
    public void UnlockKeyLockServerRpc(NetworkObjectReference objectReference){
        if (!objectReference.TryGet(out NetworkObject networkObject)) { Debug.LogError("NO NETWORK OBJECT REF FOUND ON KEYLOCK"); return; }
        networkObject.GetComponent<KeyLock>().IsLocked.Value = false;
    }
}
