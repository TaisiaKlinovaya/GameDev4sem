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
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float leanAmount = 3f; 
    [SerializeField] private float leanSpeed = 10f;
    [SerializeField] private float stepFrequency = 0.7f;

    private Vector3 inputMovement;
    private float currentLean = 0f;
    private float leanTimer = 0f;
    private bool isLeaningRight = true;
    private float movementMagnitude = 0f;

    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material blueMaterial;

    [Header("Camera Settings")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private float cameraDistance = 5f;
    [SerializeField] private float cameraHeight = 2f;
    [SerializeField] private float cameraRotationSpeed = 100f;

    private float currentCameraAngle = 180f;
    private bool isRotatingCamera = false;
    private Vector2 lastMousePosition;

    private Renderer playerRenderer;

    private NetworkVariable<Team> team = new NetworkVariable<Team>(
        Team.Red, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> points = new NetworkVariable<int>(
        10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameObject orbPrefab;
    public GameObject pillarPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            ApplyMaterial(team.Value);
        }
        team.OnValueChanged += OnTeamChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        team.OnValueChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        if (playerRenderer != null)
        {
            ApplyMaterial(current);
        }
    }

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true).gameObject;
        }

        if (IsOwner && playerCamera != null)
        {
            SetupPlayerCamera();
        }
        else if (playerCamera != null)
        {
            playerCamera.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleCameraRotation();
        HandleLeanAnimation();

        if (Input.GetKeyDown(KeyCode.O)) TrySpawnOrbServerRpc();
        if (Input.GetKeyDown(KeyCode.P)) TrySpawnPillarServerRpc();
    }

    private void HandleMovement()
    {
        inputMovement.x = Input.GetAxis("Horizontal");
        inputMovement.z = Input.GetAxis("Vertical");
        movementMagnitude = inputMovement.magnitude;

        Vector3 normalizedMovement = inputMovement.normalized;
        transform.position += normalizedMovement * Time.deltaTime * speed;

        if (IsServer) MoveClientRpc(transform.position);
    }

    private void HandleLeanAnimation()
    {
        bool isMoving = movementMagnitude > 0.1f;

        if (isMoving)
        {
            leanTimer += Time.deltaTime;
            if (leanTimer >= stepFrequency)
            {
                isLeaningRight = !isLeaningRight;
                leanTimer = 0f;
            }

            // Sanfte Neigung basierend auf Bewegungsgeschwindigkeit
            float targetLean = isLeaningRight ? leanAmount : -leanAmount;
            targetLean *= Mathf.Clamp01(movementMagnitude);

            currentLean = Mathf.Lerp(currentLean, targetLean, leanSpeed * Time.deltaTime);
        }
        else
        {
            // Zurück zur aufrechten Position
            currentLean = Mathf.Lerp(currentLean, 0f, leanSpeed * 2 * Time.deltaTime);
            leanTimer = 0f;
        }

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, currentLean);
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isRotatingCamera = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isRotatingCamera = false;
        }

        if (isRotatingCamera)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            float deltaX = currentMousePosition.x - lastMousePosition.x;

            currentCameraAngle += deltaX * cameraRotationSpeed * Time.deltaTime;
            UpdateCameraPosition();

            lastMousePosition = currentMousePosition;
        }
    }

    private void SetupPlayerCamera()
    {
        if (playerCamera == null) return;

        playerCamera.SetActive(true);
        UpdateCameraPosition();
        playerCamera.transform.LookAt(transform.position + Vector3.up * cameraHeight);
    }

    private void UpdateCameraPosition()
    {
        if (playerCamera == null) return;

        Vector3 offset = new Vector3(
            Mathf.Sin(currentCameraAngle * Mathf.Deg2Rad) * cameraDistance,
            cameraHeight,
            Mathf.Cos(currentCameraAngle * Mathf.Deg2Rad) * cameraDistance
        );

        playerCamera.transform.position = transform.position + offset;
        playerCamera.transform.LookAt(transform.position + Vector3.up * cameraHeight);
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
        if (playerRenderer == null) return;

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