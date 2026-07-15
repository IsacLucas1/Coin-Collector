using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement player))
        {
            if (IsServer)
            {
                player.Score += 10;
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
