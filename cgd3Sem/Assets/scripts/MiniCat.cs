using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MiniCat : NetworkBehaviour
{
    private NetworkVariable<int> clickCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void OnMouseDown()
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        RequestClickServerRpc(localClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestClickServerRpc(ulong clientId)
    {
        clickCount.Value++;

        Debug.Log($"MiniCat wurde geklickt. Aktuelle Klicks: {clickCount.Value}");

        if (clickCount.Value >= 3)
        {
            Debug.Log("MiniCat wird despawned nach 3 Klicks!");
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}