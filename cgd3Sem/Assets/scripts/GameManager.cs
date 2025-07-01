using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    private Team selectedTeam = Team.Red;
    private CatSpawner catSpawner;

    private void Start()
    {
        catSpawner = FindObjectOfType<CatSpawner>();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(50, 50, 300, 300));

        GUILayout.Label("Wähle dein Team:");
        if (GUILayout.Button("Team Rot")) selectedTeam = Team.Red;
        if (GUILayout.Button("Team Grün")) selectedTeam = Team.Green;
        if (GUILayout.Button("Team Blau")) selectedTeam = Team.Blue;



        GUILayout.Label("Aktuelles Team: " + selectedTeam);

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
        else
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
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
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
                Invoke(nameof(SetLocalPlayerTeam), 0.5f); // kurze Verzögerung für Spawn
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
}
