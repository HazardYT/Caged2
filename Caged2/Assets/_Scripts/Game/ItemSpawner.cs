using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [Header("Items")]
    public GameObject JailKeyPrefab;
    public GameObject RoomKeyPrefab;
    public GameObject BasementKeyPrefab;
    public GameObject ScrewDriverPrefab;
    public GameObject CrowbarPrefab;
    public GameObject BaseballBatPrefab;
    public GameObject BearTrapPrefab;
    public GameObject MeatPrefab;
    public GameObject FusePrefab;
    public GameObject CandlePrefab;
    public GameObject DollPrefab;
    public GameObject BatteryPrefab;

    [Header("Spawn Points")]
    public List<Transform> JailKeySpawnPoints = new();
    public List<Transform> RoomKeySpawnPoints = new();
    public List<Transform> BasementKeySpawnPoints = new();
    public List<Transform> ScrewDriverSpawnPoints = new();
    public List<Transform> CrowbarSpawnPoints = new();
    public List<Transform> BaseballBatSpawnPoints = new();
    public List<Transform> BearTrapSpawnPoints = new();
    public List<Transform> MeatSpawnPoints = new();
    public List<Transform> FuseSpawnPoints = new();
    public List<Transform> CandleSpawnPoints = new();
    public List<Transform> DollSpawnPoints = new();
    public List<Transform> BatterySpawnPoints = new();

    [Header("Valueables")]
    public List<Transform> ValueablesSpawnPoints = new();


    void Start(){
        if (IsServer){
            StartCoroutine(SpawnItem(JailKeyPrefab, JailKeySpawnPoints, 3));
            StartCoroutine(SpawnItem(BasementKeyPrefab, BasementKeySpawnPoints, 3));
            StartCoroutine(SpawnItem(BatteryPrefab, BatterySpawnPoints, 3));
        }
    }

    IEnumerator SpawnItem(GameObject obj, List<Transform> spawnpoints, int amount){
        for (int i = 0; i < amount; i++)
        {
            int r = Random.Range(0, spawnpoints.Count);
            GameObject spawnObject = Instantiate(obj, spawnpoints[r].position, spawnpoints[r].rotation);
            spawnObject.GetComponent<NetworkObject>().Spawn();
            spawnObject.name = obj.name;
            yield return new WaitForEndOfFrame();            
        }
    }
}
