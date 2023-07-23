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
            DropSelectedItem();
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
                        PickupItem(i, reference);
                        return;
                    }
                }

                Debug.LogError("Your Hands are Full!");
            }
        }
    }

    // pick up an item and place it in the player's hand
    public void PickupItem(byte slot, NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default)
    {
        if (!networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            Debug.LogError("Failed to Pickup item Because: TryGet from NetworkObjectReference Failed.");
            return;
        }

        if (!networkObject.TrySetParent(transform, false))
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
        SelectHand(slot);
    }
    // Server RPC to select the item in the player's hand
    public void SelectHand(byte value, ServerRpcParams rpcParams = default)
    {
        if (_selectedHandItem != null)
        {
            _selectedHandItem.localScale /= 1.25f;
            _selectedHandItem = null;
        }

        _selectedHandItem = _handItems[value];
        _selectedSlot = value;
        _selectedHandItem.localScale *= 1.25f;
    }

    // Server RPC to drop the selected item from the player's hand
    public void DropSelectedItem(ServerRpcParams rpcParams = default)
    {
        if (_selectedHandItem != null)
        {
            NetworkObject networkObject = _handItems[_selectedSlot].GetComponent<NetworkObject>();
            if (!networkObject.TryRemoveParent(true))
            {
                Debug.LogError("Failed to Drop item Because: TryRemoveParent Failed.");
                return;
            }

            _selectedHandItem.localScale /= 1.25f;
            var pickUpObjectRigidbody = _handItems[_selectedSlot].GetComponent<Rigidbody>();
            pickUpObjectRigidbody.isKinematic = false;
            pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            ParentConstraint constraint = _handItems[_selectedSlot].GetComponent<ParentConstraint>();
            constraint.RemoveSource(0);
            constraint.constraintActive = false;
            _selectedHandItem = null;
            _handItems[_selectedSlot] = null;
            networkObject.transform.position = cam.transform.position;
            pickUpObjectRigidbody.AddForce(cam.transform.forward * 4, ForceMode.Impulse);
            for (byte i = 0; i < _handItems.Length; i++)
            {
                if (_handItems[i] != null)
                {
                    SelectHand(i);
                }
            }
        }
        else
        {
            Debug.LogError("You have no Selected items to drop!");
        }
    }
}
