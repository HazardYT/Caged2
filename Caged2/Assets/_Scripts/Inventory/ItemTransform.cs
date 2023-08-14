using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;
public class ItemTransform : NetworkBehaviour
{
    public Transform _handTransform;
    public NetworkVariable<int> Slot = new(-1);
    public NetworkTransform networkTransform;
    public bool isTracking = false;
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (parentNetworkObject != null)
        {
            Slot.OnValueChanged += OnItemGrabbed;
        }
        else if (parentNetworkObject == null)
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
        networkTransform.enabled = false;
    }

    public void OnItemDropped()
    {
        print("ON ITEM DROPPED CALLED");
        isTracking = false;
        _handTransform = null;
        Slot.OnValueChanged -= OnItemGrabbed;
        print ("Enabling NETWORK TRANSFORM");
        networkTransform.enabled = true;
    }

    private void Update()
    {
        if (!isTracking) return;
        transform.SetPositionAndRotation(_handTransform.position, _handTransform.rotation);
    }

}
