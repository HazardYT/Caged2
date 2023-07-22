using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform[] spawnPoints;
    int spawnedPlayers = 0;
    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        foreach(var client in ServerManager.Instance.ClientData){
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if(character != null){
                var characterInstance = Instantiate(character.Prefab, spawnPoints[spawnedPlayers].position, spawnPoints[spawnedPlayers].rotation);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
            spawnedPlayers++;
        }
    }
}
