using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        ManageInput();
    }
    private void ManageInput(){

        // Keyboard and Universal
        if (UserInput.instance.InteractPressed){
            RaycastToItem();
        }
        if (UserInput.instance.RightHandPressed){
            if (_handItems[0] != null){
            SelectHandServerRpc(0);
            }
        }
        if (UserInput.instance.LeftHandPressed){
            if (_handItems[1] != null){
            SelectHandServerRpc(1);
            }
        }
        if (UserInput.instance.ThrowHeld){
        }
        if (UserInput.instance.ThrowReleased){
            DropSelectedItemServerRpc();
        }

        //Controller Specific Input

        if (UserInput.instance.RightBumperPressed){

        }
        if (UserInput.instance.LeftBumperPressed){

        }
    }
    private void RaycastToItem(){
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 5, layerMask)){
            if (hit.transform.CompareTag("Item")){
                for (byte i = 0; i < _handItems.Length; i++)
                {
                    if (_handItems[i] != null) continue;
                        hit.transform.TryGetComponent<NetworkObject>(out NetworkObject networkObject);        
                        NetworkObjectReference reference = new NetworkObjectReference(networkObject);
                        PickupItemServerRpc(i,reference);
                        return;
                    }
                }
                Debug.LogError("Your Hands are Full!");
            }
    
    }
    [ServerRpc]
    public void PickupItemServerRpc(byte slot,NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default){
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (!networkObjectReference.TryGet(out NetworkObject networkObject)){
            Debug.LogError("Failed to Pickup item Because:  TryGet from NetworkObjectReference Failed.");
        }
        if (!networkObject.TrySetParent(inv.transform, false)){
            Debug.LogError("Failed to Pickup item Because:  TrySetParent Failed.");
        }
        _handItems[slot] = networkObject.transform;
        var pickUpObjectRigidbody = networkObject.GetComponent<Rigidbody>();
        pickUpObjectRigidbody.isKinematic = true;
        pickUpObjectRigidbody.interpolation = RigidbodyInterpolation.None;
        ConstraintSource constraintSource = new ConstraintSource {
            sourceTransform = _handSlots[slot], weight = 1
        };
        ParentConstraint constraint = networkObject.GetComponent<ParentConstraint>();
        constraint.AddSource(constraintSource);
        constraint.constraintActive = true;
        SelectHandServerRpc(slot);
    }
    [ServerRpc]
    public void SelectHandServerRpc(byte value, ServerRpcParams rpcParams = default)
    {   
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (inv._selectedHandItem != null){
            inv._selectedHandItem.localScale /= 1.25f;
            inv._selectedHandItem = null;
        }
        inv._selectedHandItem = _handItems[value];
        inv._selectedSlot = value;
        inv._selectedHandItem.localScale *= 1.25f;
    }
    [ServerRpc]
    public void DropSelectedItemServerRpc(ServerRpcParams rpcParams = default){
        Inventory inv = NetworkManager.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Inventory>();
        if (inv._selectedHandItem != null){ 
            if (!inv._handItems[inv._selectedSlot].GetComponent<NetworkObject>().TryRemoveParent(true)){
                Debug.LogError("Failed to Drop item Because:  TryRemoveParent Failed.");
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
            pickUpObjectRigidbody.AddForce(transform.forward * 2, ForceMode.Impulse);
            for (byte i = 0; i < _handItems.Length; i++)
            {
                if (_handItems[i] != null){
                    SelectHandServerRpc(i);
                }
            }
        }
        else{
            Debug.LogError("You have no Selected items to drop!");
        }
    }


}
