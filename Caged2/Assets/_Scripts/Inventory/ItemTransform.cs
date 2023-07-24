using UnityEngine;

    public class ItemTransform : MonoBehaviour
    {
        [SerializeField]
        private Transform _handTransform;
        [SerializeField]
        private OwnerNetworkTransform ownerNetworkTransform;
        public void OnItemPickup(byte i){
            _handTransform = transform.root.GetComponent<Inventory>()._handSlots[i];
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
