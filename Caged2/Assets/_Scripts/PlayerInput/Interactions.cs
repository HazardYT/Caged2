using UnityEngine;
using Unity.Netcode;

public class Interactions : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    private GameManager _gameManager;
    public override void OnNetworkSpawn()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (IsServer){
            _gameManager.ToggleTimer();
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        if (UserInput.instance.InteractPressed){
            inventory.InteractItem();
        }
    }
}
