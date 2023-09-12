using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Interactions : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask layerMask; 
    public override void OnNetworkSpawn()
    {
        GameManager _gameManager = FindObjectOfType<GameManager>();
        if (IsServer){
            _gameManager.ToggleTimer();
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        if (UserInput.instance.InteractPressed){
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 5, layerMask)){
                if (hit.transform.CompareTag("Item")){
                    inventory.Interact(hit);
                    return;
                }
                else if (hit.transform.TryGetComponent(out KeyLock keyLock)){
                    keyLock.Interact(hit, inventory);
                }
                else{
                    if (hit.transform.TryGetComponent<IInteractable>(out var interaction))
                    interaction.Interact(hit, inventory);
                }
            }
        }
    }
}
