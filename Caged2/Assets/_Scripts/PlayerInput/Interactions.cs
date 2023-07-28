using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    void Update()
    {
        if (UserInput.instance.InteractPressed){
            inventory.InteractItem();
        }
    }
}
