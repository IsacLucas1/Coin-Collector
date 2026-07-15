using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public TMP_InputField joinInput; 
    public GameObject mainMenuPanel; 
    public TMP_Text codeDisplayText;
    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("COD GENERAT: " + joinCode);
            
            if (codeDisplayText != null)
            {
                codeDisplayText.text = "COD CAMERĂ: " + joinCode;
            }
            
            joinInput.text = joinCode; 
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            mainMenuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Eroare la Create: " + e);
        }
    }
    public async void JoinGame()
    {
        string code = joinInput.text; // Luăm codul scris de tine în căsuță

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Nu ai introdus niciun cod!");
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
            mainMenuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cod greșit sau eroare: " + e);
        }
    }
}