using UnityEngine;
using Unity.Netcode;

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
                else{
                    if (!hit.transform.TryGetComponent<IInteractable>(out var interaction)) return;
                    interaction.Interact(hit);
                }
            }
        }
    }
}
