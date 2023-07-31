using UnityEngine;
using Unity.Netcode;

public class ItemTransform : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    public NetworkVariable<int> Slot = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (parentNetworkObject != null)
        {
            Slot.OnValueChanged += OnItemGrabbed;
        }
        else
        {
            OnItemDropped();
            return;
        }
    }
    public void OnItemGrabbed(int previousValue, int newValue)
    {
        Inventory inventory = transform.parent.GetComponent<Inventory>();
        _handTransform = inventory._handTransforms[newValue];
        GetComponent<OwnerNetworkTransform>().enabled = false;
    }

    public void OnItemDropped()
    {
        _handTransform = null;
        GetComponent<OwnerNetworkTransform>().enabled = true;
        Slot.OnValueChanged -= OnItemGrabbed;
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
    public void ChangeOwnershipServerRpc(NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default){
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)) {Debug.LogError("Cant get Network object out of Reference in ChangeOwnership on Item Transform"); return; }
        networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    }
}
