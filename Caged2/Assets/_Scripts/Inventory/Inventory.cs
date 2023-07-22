using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Animations;

public class Inventory : NetworkBehaviour
{
    public Camera cam;
    public LayerMask layerMask;
    [SerializeField] private InventoryVisuals visuals;
    [SerializeField] private Transform[] _handItems;
    [SerializeField] private Transform[] _handSlots;
    [SerializeField] private Transform _selectedHandItem;
    [SerializeField] private byte _selectedSlot;

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
            SelectHandServerRpc(0);
        }

        if (UserInput.instance.LeftHandPressed && _handItems[1] != null)
        {
            SelectHandServerRpc(1);
        }

        if (UserInput.instance.ThrowHeld)
        {
            // Perform any actions when the throw button is held down.
        }

        if (UserInput.instance.ThrowReleased)
        {
            DropSelectedItemServerRpc();
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
                for (byte i = 0; i < _handItems.Length; i++)
                {
                    if (_handItems[i] != null) continue;

                    if (hit.transform.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                    {
                        NetworkObjectReference reference = new NetworkObjectReference(networkObject);
                        PickupItemServerRpc(i, reference);
                        return;
                    }
                }

                Debug.LogError("Your Hands are Full!");
            }
        }
    }

    // Server RPC to pick up an item and place it in the player's hand
    [ServerRpc(RequireOwnership = false)]
    public void PickupItemServerRpc(byte slot, NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default)
    {
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (!networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            Debug.LogError("Failed to Pickup item Because: TryGet from NetworkObjectReference Failed.");
            return;
        }

        if (!networkObject.TrySetParent(inv.transform, false))
        {
            Debug.LogError("Failed to Pickup item Because: TrySetParent Failed.");
            return;
        }

        _handItems[slot] = networkObject.transform;
        var pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.None;
        ConstraintSource constraintSource = new ConstraintSource
        {
            sourceTransform = _handSlots[slot],
            weight = 1
        };
        ParentConstraint constraint = networkObject.GetComponent<ParentConstraint>();
        constraint.AddSource(constraintSource);
        constraint.constraintActive = true;
        networkObject.GetComponent<OwnerNetworkTransform>().enabled = false;
        networkObject.GetComponent<NetworkRigidbody>().enabled = false;
        SelectHandServerRpc(slot);
    }

    // Server RPC to select the item in the player's hand
    [ServerRpc(RequireOwnership = false)]
    public void SelectHandServerRpc(byte value, ServerRpcParams rpcParams = default)
    {
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (inv._selectedHandItem != null)
        {
            inv._selectedHandItem.localScale /= 1.25f;
            inv._selectedHandItem = null;
        }

        inv._selectedHandItem = _handItems[value];
        inv._selectedSlot = value;
        inv._selectedHandItem.localScale *= 1.25f;
    }

    // Server RPC to drop the selected item from the player's hand
    [ServerRpc(RequireOwnership = false)]
    public void DropSelectedItemServerRpc(ServerRpcParams rpcParams = default)
    {
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (inv._selectedHandItem != null)
        {
            NetworkObject networkObject = inv._handItems[inv._selectedSlot].GetComponent<NetworkObject>();
            if (!networkObject.TryRemoveParent(true))
            {
                Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
                return;
            }

            inv._selectedHandItem.localScale /= 1.25f;
            var pickUpObjectRigidbody = inv._handItems[inv._selectedSlot].GetComponent<Rigidbody>();
            pickUpObjectRigidbody.isKinematic = false;
            pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            ParentConstraint constraint = inv._handItems[inv._selectedSlot].GetComponent<ParentConstraint>();
            constraint.RemoveSource(0);
            constraint.constraintActive = false;
            inv._selectedHandItem = null;
            inv._handItems[inv._selectedSlot] = null;
            networkObject.GetComponent<OwnerNetworkTransform>().enabled = true;
            networkObject.GetComponent<NetworkRigidbody>().enabled = true;
            networkObject.transform.position = cam.transform.position;
            pickUpObjectRigidbody.AddForce(cam.transform.forward * 4, ForceMode.Impulse);
            for (byte i = 0; i < _handItems.Length; i++)
            {
                if (_handItems[i] != null)
                {
                    SelectHandServerRpc(i);
                }
            }
        }
        else
        {
            Debug.LogError("You have no Selected items to drop!");
        }
    }
}
