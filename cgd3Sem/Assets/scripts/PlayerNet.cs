using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public enum Team
{
    Red,
    Green,
    Blue
}

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;
    private Vector3 inputMovement;

    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material blueMaterial;

    private Renderer playerRenderer;

    private NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Red, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Punktestand synchronisiert, jeder Spieler startet mit 10 Punkten
    private NetworkVariable<int> points = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameObject orbPrefab;
    public GameObject pillarPrefab;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        team.OnValueChanged += (oldVal, newVal) => ApplyMaterial(newVal);
        ApplyMaterial(team.Value);
    }

    void Update()
    {
        if (!IsOwner) return;

        inputMovement.x = Input.GetAxis("Horizontal");
        inputMovement.y = Input.GetAxis("Vertical");
        transform.position += inputMovement * Time.deltaTime * speed;

        if (!IsServer || IsHost)
        {
            MoveServerRpc(transform.position);
        }

        // Beispiel Input zum Spawnen:
        if (Input.GetKeyDown(KeyCode.O)) TrySpawnOrbServerRpc();
        if (Input.GetKeyDown(KeyCode.P)) TrySpawnPillarServerRpc();
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 newPos)
    {
        MoveClientRpc(newPos);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 newPos)
    {
        if (!IsOwner)
        {
            transform.position = newPos;
        }
    }

    private void ApplyMaterial(Team team)
    {
        switch (team)
        {
            case Team.Red:
                playerRenderer.material = redMaterial;
                break;
            case Team.Green:
                playerRenderer.material = greenMaterial;
                break;
            case Team.Blue:
                playerRenderer.material = blueMaterial;
                break;
        }
    }

    // Team setzen wie vorher
    public void SetTeam(Team selectedTeam)
    {
        if (IsServer)
        {
            team.Value = selectedTeam;
        }
        else
        {
            SubmitTeamRequestServerRpc(selectedTeam);
        }
    }

    [ServerRpc]
    private void SubmitTeamRequestServerRpc(Team selectedTeam)
    {
        team.Value = selectedTeam;
    }

    // Orb spawnen (kostet 3 Punkte)
    [ServerRpc(RequireOwnership = false)]
    private void TrySpawnOrbServerRpc(ServerRpcParams rpcParams = default)
    {
        if (points.Value >= 3)
        {
            points.Value -= 3;

            Vector3 spawnPos = transform.position + transform.forward * 2f + Vector3.up;
            var orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
            orb.GetComponent<NetworkObject>().Spawn();
        }
    }

    // Pillar spawnen (kostet 2 Punkte)
    [ServerRpc(RequireOwnership = false)]
    private void TrySpawnPillarServerRpc(ServerRpcParams rpcParams = default)
    {
        if (points.Value >= 2)
        {
            points.Value -= 2;

            Vector3 spawnPos = transform.position + transform.right * 2f + Vector3.up;
            var pillar = Instantiate(pillarPrefab, spawnPos, Quaternion.identity);
            pillar.GetComponent<NetworkObject>().Spawn();
        }
    }

    // Optional: Punktestand öffentlich lesbar
    public int GetPoints()
    {
        return points.Value;
    }

    void OnGUI()
    {
        if (IsOwner)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Punkte: " + points.Value);
        }
    }
}
