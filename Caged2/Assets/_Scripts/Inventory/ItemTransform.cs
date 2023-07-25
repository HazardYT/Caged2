using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ItemTransform : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    [SerializeField] private OwnerNetworkTransform ownerNetworkTransform;
    public NetworkVariable<int> equipSlot = new NetworkVariable<int>(-1);

    public void OnTransformParentChanged()
    { 
        if (transform.parent == null){
            OnItemDropped();
            return;
        }
        else
        {
            print(equipSlot.Value + " : Curr Value");
            if (!transform.parent.TryGetComponent(out Inventory inventory)){
                Debug.LogError("Failed to get Parent of Item");
            }
            /*if (inventory._handItems[equipSlot.Value] != null){
                equipSlot.Value = equipSlot.Value == 0 ? 1 : 0;
            }
            */
            OnItemGrabbed(equipSlot.Value);
        }
    }

    public void OnItemGrabbed(int i)
    {
        _handTransform = transform.root.GetChild(0).GetChild(i);
        ownerNetworkTransform.enabled = false;
    }

    public void OnItemDropped()
    {
        _handTransform = null;
        ownerNetworkTransform.enabled = true;
    }

    private void Update()
    {
        if (_handTransform == null) return;
        UpdateTransforms();
    }

    private void UpdateTransforms()
    {
        transform.position = _handTransform.position;
        transform.rotation = _handTransform.rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetEquipSlotServerRpc(ulong id, int slot)
    {
        print("before set rpc");
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.equipSlot.Value = slot;
        print("Setting value to " + slot);
    }
}
