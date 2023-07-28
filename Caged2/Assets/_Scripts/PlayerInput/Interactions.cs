using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Interactions : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    void Update()
    {
        if (!IsOwner) return;
        if (UserInput.instance.InteractPressed){
            inventory.InteractItem();
        }
    }
}
