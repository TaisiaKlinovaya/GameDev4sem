using UnityEngine;
using Unity.Netcode;

public class CatSpawner : NetworkBehaviour
{
    public GameObject cat1Prefab;
    public GameObject cat2Prefab;
    public float spawnInterval = 5f;

    private float timer;
    public bool isGameRunning = true;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsServer || !isGameRunning) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomCat();
        }
    }

    private void SpawnRandomCat()
    {
        GameObject catPrefab = Random.value < 0.5f ? cat1Prefab : cat2Prefab;
        Vector3 randomPosition = GetRandomScreenPosition();

        GameObject catInstance = Instantiate(catPrefab, randomPosition, Quaternion.identity);
        catInstance.GetComponent<NetworkObject>().Spawn();
    }

    private Vector3 GetRandomScreenPosition()
    {
        float x = Random.Range(0.1f, 0.9f);
        float y = Random.Range(0.1f, 0.9f);
        Vector3 screenPos = new Vector3(x * Screen.width, y * Screen.height, 10f);

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    public void StopSpawning()
    {
        isGameRunning = false;
    }

    public void DespawnAllCats()
    {
        foreach (var cat in GameObject.FindGameObjectsWithTag("Cat"))
        {
            if (cat.TryGetComponent<NetworkObject>(out var netObj))
                netObj.Despawn();
            else
                Destroy(cat);
        }
    }
}
