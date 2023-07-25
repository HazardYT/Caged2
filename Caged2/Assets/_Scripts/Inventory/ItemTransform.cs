using UnityEngine;
using Unity.Netcode;

public class ItemTransform : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    [SerializeField] private OwnerNetworkTransform ownerNetworkTransform;
    public NetworkVariable<int> equipSlot = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (transform.parent != null){
            equipSlot.OnValueChanged += ItemStateCheck;
            print(equipSlot.Value + " : Curr Value");
        }
        else
        {
            OnItemDropped();
            return;
        }
    }
    public void ItemStateCheck(int previousValue, int newValue){
        OnItemGrabbed(newValue);
        print($"Prev Value: {previousValue} \nNew Value: {newValue}");
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
        equipSlot.OnValueChanged -= ItemStateCheck;
        SetEquipSlotServerRpc(GetComponent<NetworkObject>().NetworkObjectId, -1);
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
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.equipSlot.Value = slot;
    }
}
