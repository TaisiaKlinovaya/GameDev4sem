using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MegaCat : NetworkBehaviour
{
    private HashSet<Team> teamsClicked = new HashSet<Team>();

    private void OnMouseDown()
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        ClickServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClickServerRpc(ulong clientId)
    {
        var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerObj == null) return;

        var player = playerObj.GetComponent<Player>();
        if (player == null) return;

        Team team = player.GetTeam();

        // Nur wenn das Team noch nicht geklickt hat
        if (!teamsClicked.Contains(team))
        {
            teamsClicked.Add(team);
            Debug.Log($"Team {team} hat MegaCat geklickt.");

            if (teamsClicked.Count >= 2)
            {
                Debug.Log("MegaCat wird despawned (2 verschiedene Teams haben geklickt)!");
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}