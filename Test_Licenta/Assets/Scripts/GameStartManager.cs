using Unity.Netcode;
using UnityEngine;

public class GameStartManager : NetworkBehaviour
{
    public static GameStartManager Instance;
    
    [Header("UI")]
    public GameObject waitingPanel;
    public bool isGameActive = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        isGameActive = false;
        waitingPanel.SetActive(true);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            StartGameClientRpc();
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        isGameActive = true;           
        waitingPanel.SetActive(false); 
        Debug.Log("JOCUL A ÎNCEPUT!");
    }
    
    // Curățenie când se oprește jocul
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
