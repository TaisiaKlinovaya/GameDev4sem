using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class RandomPoints : MonoBehaviour
{
    public float interval = 1f;
    public int pointsPerInterval = 1;

    private float timer = 0f;

    void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            GiveRandomPlayerPoints();
        }
    }

    private void GiveRandomPlayerPoints()
    {
        var clientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        if (clientIds.Count == 0)
            return;

        ulong randomClientId = clientIds[Random.Range(0, clientIds.Count)];

        var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(randomClientId);
        if (playerObject != null)
        {
            var player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                player.AddPointsServerRpc(pointsPerInterval);
            }
        }
    }
}
