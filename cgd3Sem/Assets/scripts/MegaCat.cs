using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class MegaCat : NetworkBehaviour
{
    private HashSet<Team> teamsClicked = new HashSet<Team>();

    private void OnMouseDown()
    {
        if (!IsOwner) return;

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        ClickServerRpc(clientId);
    }

    [ServerRpc]
    private void ClickServerRpc(ulong clientId)
    {
        var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId)?.GetComponent<Player>();
        if (player == null) return;

        teamsClicked.Add(player.GetTeam());

        if (teamsClicked.Count >= 2)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
