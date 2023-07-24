using UnityEngine;
using Unity.Netcode;
    public class ItemTransform : NetworkBehaviour
    {
        [SerializeField]
        private Transform _handTransform;
        [SerializeField]
        private OwnerNetworkTransform ownerNetworkTransform;
        public void OnTransformParentChanged()
        { 
            if (transform.parent == null) OnItemDropped();
            else OnItemGrabbed();
            /*Inventory inventory = parentNetworkObject.GetComponent<Inventory>();
            for (int i = 0; i < inventory._handItems.Length; i++)
            {
                if (inventory._handItems[i] == this.transform){
                    _handTransform = inventory._handSlots[i];
                    return;
                }
            }
            */
        } 
        public void OnItemGrabbed(){
            _handTransform = transform.root.GetChild(0).GetChild(0);
            ownerNetworkTransform.enabled = false;
        }
        public void OnItemDropped(){
            _handTransform = null;
            ownerNetworkTransform.enabled = true;
        }
        private void Update()
        {
            if (_handTransform == null) return;
            UpdateTransforms();
        }
        private void UpdateTransforms(){
            transform.position = _handTransform.position;
            transform.rotation = _handTransform.rotation;
        }
    }
