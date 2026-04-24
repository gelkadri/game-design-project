using UnityEngine;

public class SkyAsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRangeX = 8f;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float intervalVariation = 0.5f;

    private float timer;
    private Transform player;

    private void Start()
    {
        SkyExplorerController ctrl = FindObjectOfType<SkyExplorerController>();
        if (ctrl != null)
            player = ctrl.transform;

        timer = spawnInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnAsteroid();
            timer = spawnInterval + Random.Range(-intervalVariation, intervalVariation);
        }
    }

    private void SpawnAsteroid()
    {
        if (asteroidPrefab == null || player == null) return;

        float randomX = player.position.x + Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, player.position.y + spawnHeight, 0f);

        Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);
    }
}
