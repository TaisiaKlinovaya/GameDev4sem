using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    private Team selectedTeam = Team.Red;
    private CatSpawner catSpawner;
    private bool gameStarted = false;

    private void Start()
    {
        catSpawner = FindObjectOfType<CatSpawner>();

        NetworkManager.Singleton.OnServerStarted += OnGameStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnGameStarted()
    {
        gameStarted = true;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            gameStarted = true;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(50, 50, 300, 300));

        // Zeige Team-Auswahl und Netzwerk-Buttons nur, wenn das Spiel noch nicht gestartet ist
        if (!gameStarted)
        {
            GUILayout.Label("Wähle dein Team:");
            if (GUILayout.Button("Team Rot")) selectedTeam = Team.Red;
            if (GUILayout.Button("Team Grün")) selectedTeam = Team.Green;
            if (GUILayout.Button("Team Blau")) selectedTeam = Team.Blue;

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (GUILayout.Button("Host"))
                {
                    NetworkManager.Singleton.StartHost();
                    AssignTeamAfterSpawn();
                }
                if (GUILayout.Button("Client"))
                {
                    NetworkManager.Singleton.StartClient();
                    AssignTeamAfterSpawn();
                }
                if (GUILayout.Button("Server"))
                {
                    NetworkManager.Singleton.StartServer();
                }
            }
        }

        GUILayout.Label("Aktuelles Team: " + selectedTeam);

        // Zeige immer den aktuellen Modus an
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label($"Mode: {(NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsClient ? "Client" : "Server")}");
            GUILayout.Label($"Local Client ID: {NetworkManager.Singleton.LocalClientId}");

            if (GUILayout.Button("Shutdown"))
            {
                if (catSpawner != null && NetworkManager.Singleton.IsServer)
                {
                    catSpawner.StopSpawning();
                    catSpawner.DespawnAllCats();
                }

                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene(0);
                gameStarted = false;
            }
        }

        GUILayout.EndArea();
    }

    private void AssignTeamAfterSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Invoke(nameof(SetLocalPlayerTeam), 0.5f);
            }
        };
    }

    private void SetLocalPlayerTeam()
    {
        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.GetComponent<Player>();
        if (player != null)
        {
            player.SetTeam(selectedTeam);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnGameStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}