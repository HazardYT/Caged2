using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ItemTransform : NetworkBehaviour
{
    [SerializeField] private Transform _handTransform;
    [SerializeField] private OwnerNetworkTransform ownerNetworkTransform;
    public NetworkVariable<byte> equipSlot = new NetworkVariable<byte>(0);

    public void OnTransformParentChanged()
    { 
        byte j = equipSlot.Value;
        print(j + " : Curr Value");
        StartCoroutine(ItemStateCheck(j));
    }
    public IEnumerator ItemStateCheck(byte j){
        yield return new WaitUntil(() => (j != equipSlot.Value));
        if (transform.parent == null) 
            OnItemDropped();
        else 
            print(equipSlot.Value + " : New Value");
            OnItemGrabbed(equipSlot.Value);
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

    [ServerRpc]
    public void SetEquipSlotServerRpc(ulong id, byte slot)
    {
        print("before set rpc");
        ItemTransform itemTransform = NetworkManager.SpawnManager.SpawnedObjects[id].transform.GetComponent<ItemTransform>();
        itemTransform.equipSlot.Value = slot;
        print("Setting value to " + slot);
    }
}
