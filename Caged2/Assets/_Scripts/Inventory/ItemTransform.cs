using UnityEngine;
using Unity.Netcode;

public class ItemTransform : NetworkBehaviour
{
    public Transform _handTransform;
    public NetworkVariable<int> Slot = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool isTracking = false;
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
        if (newValue == -1) return;
        isTracking = true;
        Inventory inventory = transform.parent.GetComponent<Inventory>();
        _handTransform = inventory._handTransforms[newValue];
        GetComponent<OwnerNetworkTransform>().enabled = false;
    }

    public void OnItemDropped()
    {
        isTracking = false;
        Slot.OnValueChanged -= OnItemGrabbed;
        _handTransform = null;
        GetComponent<OwnerNetworkTransform>().enabled = true;
    }

    private void Update()
    {
        if (!isTracking) return;
        UpdateTransforms();
    }

    private void UpdateTransforms()
    {
        transform.position = _handTransform.position;
        transform.rotation = _handTransform.rotation;
    }
}
