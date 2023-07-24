using UnityEngine;
using Unity.Netcode;

public class ItemTransform : NetworkBehaviour
{
    [SerializeField]
    private Transform _handTransform;
    [SerializeField]
    private OwnerNetworkTransform ownerNetworkTransform;
    public NetworkVariable<byte> equipSlot = new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void OnTransformParentChanged()
    { 
        if (transform.parent == null) 
            OnItemDropped();
        else 
            OnItemGrabbed(equipSlot.Value);
    }

    [ServerRpc]
    public void SetEquipSlotServerRpc(ulong id, byte slot)
    {
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.equipSlot.Value = slot;
    }

    public void OnItemGrabbed(byte i)
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
}
