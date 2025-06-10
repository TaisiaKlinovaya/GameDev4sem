using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(50, 50, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButton();
        }
        else
        {
            StatusLabels();
        }
        GUILayout.EndArea();
    }

    static void StartButton()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = "Unknown";

        if (NetworkManager.Singleton.IsHost)
            mode = "Host";
        else if (NetworkManager.Singleton.IsServer)
            mode = "Server";
        else if (NetworkManager.Singleton.IsClient)
            mode = "Client";

        GUILayout.Label($"Mode: {mode}");
        GUILayout.Label($"Local Client ID: {NetworkManager.Singleton.LocalClientId}");

        if (GUILayout.Button("Shutdown"))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
