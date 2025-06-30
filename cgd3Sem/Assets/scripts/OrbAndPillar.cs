using Unity.Netcode;
using UnityEngine;

public class OrbAndPillar : NetworkBehaviour
{
    private NetworkVariable<Team> ownerTeam = new NetworkVariable<Team>(
        Team.Red,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        ApplyColor(ownerTeam.Value);
        ownerTeam.OnValueChanged += (oldTeam, newTeam) => ApplyColor(newTeam);
    }

    public void SetTeam(Team team)
    {
        if (IsServer)
        {
            ownerTeam.Value = team;
        }
    }

    private void ApplyColor(Team team)
    {
        var rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("Kein Renderer gefunden für Orb oder Pillar");
            return;
        }

        switch (team)
        {
            case Team.Red:
                rend.material.color = Color.red;
                break;
            case Team.Green:
                rend.material.color = Color.green;
                break;
            case Team.Blue:
                rend.material.color = Color.blue;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var player = other.GetComponent<Player>();
        if (player != null)
        {
            Team playerTeam = player.GetTeam();

            if (playerTeam != ownerTeam.Value)
            {
                player.AddPointsServerRpc(1);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

}
