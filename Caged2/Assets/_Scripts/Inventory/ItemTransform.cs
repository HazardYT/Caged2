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
            equipSlot.OnValueChanged += OnItemGrabbed;
        }
        else
        {
            OnItemDropped();
            return;
        }
    }
    public void OnItemGrabbed(int previousValue, int newValue)
    {
        _handTransform = transform.root.GetChild(0).GetChild(newValue);
        ownerNetworkTransform.enabled = false;
    }

    public void OnItemDropped()
    {
        _handTransform = null;
        ownerNetworkTransform.enabled = true;
        equipSlot.OnValueChanged -= OnItemGrabbed;
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
