using UnityEngine;

    public class ItemTransform : MonoBehaviour
    {
        [SerializeField]
        private Transform _handTransform;
        [SerializeField]
        private OwnerNetworkTransform ownerNetworkTransform;
        public void OnTransformParentChanged(){
            Inventory inventory = transform.root.GetComponent<Inventory>();
            if (inventory._handItems[0] != null){
                _handTransform = transform.root.GetComponent<Inventory>()._handSlots[0];
            }
            else if (inventory._handItems[1] != null){
                _handTransform = transform.root.GetComponent<Inventory>()._handSlots[1];
            }
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
