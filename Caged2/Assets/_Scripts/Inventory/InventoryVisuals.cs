using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InventoryVisuals : NetworkBehaviour
{
    [SerializeField] private Image[] HandSlots;
    [SerializeField] private Image[] HandHighlights;

    public override void OnNetworkSpawn(){
        if (!IsOwner && !IsLocalPlayer){
            this.enabled = false;
        }
    }
    public void SelectSlot(int slot){
        int nextIndex = (slot + 1) % HandHighlights.Length;
        HandHighlights[nextIndex].enabled = false;
        HandHighlights[slot].enabled = true;
    }
    public void PickupItem(NetworkObject networkObject, int slot){
        if (networkObject.TryGetComponent(out ItemInfo info)){
            if (info.sprite != null){
                HandSlots[slot].sprite = info.sprite;
                HandSlots[slot].enabled = true;
            }
        }
    }
    public void RemoveItem(int slot){
        HandSlots[slot].sprite = null;
        HandSlots[slot].enabled = false;
        HandHighlights[slot].enabled = false;
    }
}
