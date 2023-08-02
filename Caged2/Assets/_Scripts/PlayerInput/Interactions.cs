using UnityEngine;
using Unity.Netcode;

public class Interactions : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask layerMask;
    private GameManager _gameManager;
    public override void OnNetworkSpawn()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (IsHost){
            _gameManager.ToggleTimer();
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        if (UserInput.instance.InteractPressed){
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 5, layerMask)){
                if (hit.transform.CompareTag("Item")){
                    inventory.InteractItem(hit);
                }
                if (hit.transform.CompareTag("Door")){
                    Door door = hit.transform.GetComponent<Door>();
                    if (!door._doorCurrentlyMoving.Value && !door._isLocked.Value){
                        door.ChangeDoorStateServerRpc();
                    }
                    if (door._isLocked.Value){
                        if (inventory.SearchInventoryForItem(door.neededKey.ToString()))
                        door.UnlockDoorServerRpc();
                    }
                }
            }
        }
    }
}
