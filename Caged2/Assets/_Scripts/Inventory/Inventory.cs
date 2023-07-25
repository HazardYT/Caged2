using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public Camera cam;
    public LayerMask layerMask;
    public Transform[] _handSlots;
    public Transform[] _handItems;
    [SerializeField] private Transform _selectedHandItem;
    [SerializeField] private InventoryVisuals visuals;
    public NetworkVariable<int> _selectedSlot = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (!IsOwner) return;
        ManageInput();
    }

    // Process player input
    private void ManageInput()
    {
        // Keyboard and Universal Input
        if (UserInput.instance.InteractPressed)
        {
            RaycastToItem();
        }

        if (UserInput.instance.RightHandPressed && _handItems[0] != null)
        {
            SelectHand(0);
        }

        if (UserInput.instance.LeftHandPressed && _handItems[1] != null)
        {
            SelectHand(1);
        }

        if (UserInput.instance.ThrowHeld)
        {
            // Perform any actions when the throw button is held down.
        }

        if (UserInput.instance.ThrowReleased)
        {
            if (_selectedHandItem != null)
                DropSelectedItemServerRpc();
            else 
                Debug.LogError("You have no Selected items to drop!");
        }

        // Controller Specific Input
        if (UserInput.instance.RightBumperPressed)
        {
            // Perform any actions when the right bumper is pressed.
        }

        if (UserInput.instance.LeftBumperPressed)
        {
            // Perform any actions when the left bumper is pressed.
        }
    }

    // Raycast to detect and pick up items in the scene
    private void RaycastToItem()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 5, layerMask))
        {
            if (hit.transform.CompareTag("Item"))
            {
                for (int i = 0; i < _handItems.Length; i++)
                {
                    if (_handItems[i] != null) continue;

                    if (hit.transform.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                    {
                        NetworkObjectReference reference = new NetworkObjectReference(networkObject);
                        if (IsClient){
                        _handItems[i] = networkObject.transform;
                        }
                        print("Attempting Calling Equip " + i);
                        networkObject.GetComponent<ItemTransform>().SetEquipSlotServerRpc(networkObject.NetworkObjectId, i);
                        print("Called Equip");
                        PickupItemServerRpc(i, reference);
                        return;
                    }
                    else Debug.LogError("Failed To Get Component [NetworkObject] from Item");
                }

                Debug.LogError("Your Hands are Full!");
            }
        }
    }
    public void SelectHand(int value)
    {
        if (_selectedHandItem != null)
        {
            _selectedHandItem.localScale /= 1.25f;
            _selectedHandItem = null;
        }

        _selectedHandItem = _handItems[value];
        _selectedSlot.Value = value;
        _selectedHandItem.localScale *= 1.25f;
    }
    [ServerRpc]
    public void PickupItemServerRpc(int slot, NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default)
    {
        Transform transform = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.transform;

        if (!networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed.");
            return;
        }
        _handItems[slot] = networkObject.transform;
        NetworkObjectReference playerNetworkObjectReference = new NetworkObjectReference(transform.GetComponent<NetworkObject>());
        UpdateHandItemsClientRpc(playerNetworkObjectReference, networkObjectReference, slot);
        var pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.None;
        if (!networkObject.TrySetParent(transform, false))
        {
            Debug.LogError("Failed to Parent Item: TrySetParent Failed.");
            return;
        }
        SelectHand(slot);
    }
    [ClientRpc]
    public void UpdateHandItemsClientRpc(NetworkObjectReference senderNetworkObjectReference, NetworkObjectReference networkObjectReference, int slot){
        if (!networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed.");
            return;
        }
        if (!senderNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            Debug.LogError("Failed to Get Item NetworkObject: TryGet from NetworkObjectReference Failed.");
            return;
        }
        playerNetworkObject.GetComponent<Inventory>()._handItems[slot] = networkObject.transform;

    }
    [ServerRpc]
    public void DropSelectedItemServerRpc(ServerRpcParams rpcParams = default)
    {
        NetworkObject networkObject = _handItems[_selectedSlot.Value].GetComponent<NetworkObject>();
        if (!networkObject.TryRemoveParent(true))
        {
            Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
            return;
        }
        _selectedHandItem.localScale /= 1.25f;
        var pickUpObjectRigidbody = _handItems[_selectedSlot.Value].GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = false;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        networkObject.GetComponent<ItemTransform>().OnItemDropped();
        _handItems[_selectedSlot.Value] = null;
        _selectedHandItem = null;
        networkObject.transform.position = cam.transform.position;
        pickUpObjectRigidbody.AddForce(cam.transform.forward * 4, ForceMode.Impulse);
        for (int i = 0; i < _handItems.Length; i++)
        {
            if (_handItems[i] != null)
            {
                SelectHand(i);
            }
        }
    }
    [ServerRpc]
    public void SetSelectedSlotServerRpc(int slot, ServerRpcParams rpcParams = default){
        Inventory inventory = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        inventory._selectedSlot.Value = slot;
    }
}
