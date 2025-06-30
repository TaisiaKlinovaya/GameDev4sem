using Unity.Netcode;
using UnityEngine;

public class CatSpawner : NetworkBehaviour
{
    public GameObject cat1Prefab;
    public GameObject cat2Prefab;
    public float spawnInterval = 5f;

    private float timer = 0f;

    void Update()
    {
        if (!IsServer) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomCat();
        }
    }

    void SpawnRandomCat()
    {
        GameObject prefabToSpawn = Random.value < 0.5f ? cat1Prefab : cat2Prefab;

        Vector3 position = GetRandomScreenPosition();
        GameObject cat = Instantiate(prefabToSpawn, position, Quaternion.identity);
        cat.GetComponent<NetworkObject>().Spawn();
    }

    Vector3 GetRandomScreenPosition()
    {
        Vector2 screenPos = new Vector2(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        return worldPos;
    }
}
