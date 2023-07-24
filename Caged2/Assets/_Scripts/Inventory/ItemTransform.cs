using System;
using UnityEngine;

    public class ItemTransform : MonoBehaviour
    {
        [SerializeField]
        private Transform _handTransform;
        [SerializeField]
        private OwnerNetworkTransform ownerNetworkTransform;
        public void OnTransformParentChanged(){
            Invoke(nameof(FindTransformLocation),0.1f);
        }

    private void FindTransformLocation()
    {
        Inventory inventory = transform.root.GetComponent<Inventory>();
        for (int i = 0; i < inventory._handItems.Length; i++)
        {
            if (inventory._handItems[i] == this.transform){
                _handTransform = inventory._handSlots[i];
                return;
            }
                
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
