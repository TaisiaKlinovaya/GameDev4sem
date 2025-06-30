using Unity.Netcode;
using UnityEngine;

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

    private NetworkVariable<Team> team = new NetworkVariable<Team>(
        Team.Red, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Punkte bleiben NetworkVariable, aber keine direkte Punktevergabe hier!
    private NetworkVariable<int> points = new NetworkVariable<int>(
        10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        inputMovement.y = Input.GetAxis("Vertical"); // z statt y, da 3D

        transform.position += inputMovement * Time.deltaTime * speed;

        if (IsServer)
        {
            MoveClientRpc(transform.position);
        }

        // Spawn Eingaben
        if (Input.GetKeyDown(KeyCode.O)) TrySpawnOrbServerRpc();
        if (Input.GetKeyDown(KeyCode.P)) TrySpawnPillarServerRpc();
    }
    void OnGUI()
    {
        if (IsOwner)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Punkte: " + points.Value);
        }
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

    [ServerRpc(RequireOwnership = false)]
    private void TrySpawnOrbServerRpc(ServerRpcParams rpcParams = default)
    {
        if (points.Value >= 3)
        {
            points.Value -= 3;
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            var orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);

            var networkObj = orb.GetComponent<NetworkObject>();
            if (networkObj != null)
            {
                networkObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
            }

            var interactable = orb.GetComponent<OrbAndPillar>();
            if (interactable != null)
            {
                interactable.SetTeam(team.Value);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrySpawnPillarServerRpc(ServerRpcParams rpcParams = default)
    {
        if (points.Value >= 2)
        {
            points.Value -= 2;
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            var pillar = Instantiate(pillarPrefab, spawnPos, Quaternion.identity);

            var networkObj = pillar.GetComponent<NetworkObject>();
            if (networkObj != null)
            {
                networkObj.Spawn();
            }

            var interactable = pillar.GetComponent<OrbAndPillar>();
            if (interactable != null)
            {
                interactable.SetTeam(team.Value);
            }
        }
    }
    public int GetPoints()
    {
        return points.Value;
    }

    public Team GetTeam()
    {
        return team.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPointsServerRpc(int amount)
    {
        points.Value += amount;
    }
}
