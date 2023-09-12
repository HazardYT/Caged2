using UnityEngine;

public interface IInteractable
{
    void Interact(RaycastHit hit = default, Inventory inventory = default);
}
