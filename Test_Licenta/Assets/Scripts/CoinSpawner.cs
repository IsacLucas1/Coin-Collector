using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject coinPrefab;
    private GameObject currentCoin;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCoin();
        }
    }

    private void Update()
    {
        if(NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            return;
        }
            
        if(IsServer && currentCoin == null)
        {
            SpawnCoin();
        }
    }

    void SpawnCoin()
    {
        Vector3 randomPosition = new Vector3(Random.Range(-8f, 8f), 0.5f, Random.Range(-8f, 8f));
        
        currentCoin = Instantiate(coinPrefab, randomPosition, Quaternion.identity);
        currentCoin.GetComponent<NetworkObject>().Spawn();
    }
}
