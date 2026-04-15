using UnityEngine;

/// <summary>
/// Spawns falling asteroids at random horizontal positions above the camera.
/// Add this to an empty GameObject in levels where you want the hazard (e.g. Level 2 only).
/// </summary>
public class SkyFallingAsteroidSpawner : MonoBehaviour
{
    [Tooltip("If null, a simple grey circle template is created once and kept disabled under this spawner.")]
    [SerializeField] private GameObject asteroidPrefab;

    private GameObject _spawnPrefab;

    [SerializeField] private float minSpawnInterval = 0.65f;
    [SerializeField] private float maxSpawnInterval = 1.75f;

    [Tooltip("Extra distance above the top of the camera to spawn.")]
    [SerializeField] private float spawnHeightAboveCamera = 2.5f;

    [Tooltip("Horizontal margin inside the camera view (world units).")]
    [SerializeField] private float horizontalMargin = 1f;

    [Range(0f, 1f)]
    [Tooltip("0 = random across the screen; 1 = always above a point ahead of the player. ~0.45–0.6 feels fair.")]
    [SerializeField] private float spawnAheadOfPlayerBlend = 0.52f;

    [Tooltip("How far ahead of the player (world units) meteors tend to target, along run direction.")]
    [SerializeField] private float aheadLeadWorldUnits = 3.25f;

    [Tooltip("Multiplies the prefab asset's root scale when spawning (ignored if Use Forced Spawn Scale is on).")]
    [SerializeField] private float spawnSizeMultiplier = 0.25f;

    [Tooltip("If enabled, spawned meteors always use Forced Spawn Scale. Use this when your first asteroid looks right in the scene (e.g. 0.3) but the Project prefab root is still (1,1,1) — otherwise every clone is huge.")]
    [SerializeField] private bool useForcedSpawnScale = true;

    [Tooltip("Local scale applied to every spawned meteor when Use Forced Spawn Scale is on. Match your scene asteroid: Transform → Scale (e.g. 0.3, 0.3, 1).")]
    [SerializeField] private Vector3 forcedSpawnScale = new Vector3(0.3f, 0.3f, 1f);

    /// <summary>Prefab root local scale captured once in Awake (often (1,1,1) on the asset even if a scene instance was scaled down).</summary>
    private Vector3 _cachedPrefabLocalScale = Vector3.one;

    private float _nextSpawnTime;

    private void Awake()
    {
        if (asteroidPrefab != null)
        {
            _spawnPrefab = asteroidPrefab;
        }
        else
        {
            _spawnPrefab = CreateRuntimePlaceholderAsteroid();
            _spawnPrefab.name = "AsteroidTemplate";
            _spawnPrefab.transform.SetParent(transform, false);
            _spawnPrefab.SetActive(false);
        }

        _cachedPrefabLocalScale = _spawnPrefab != null ? _spawnPrefab.transform.localScale : Vector3.one;
    }

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (SkyRealmGameManager.instance == null || !SkyRealmGameManager.instance.IsLevelPlayable)
            return;

        if (Time.time < _nextSpawnTime)
            return;

        SpawnOne();
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        _nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnOne()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
            return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 cpos = cam.transform.position;
        float left = cpos.x - halfW + horizontalMargin;
        float right = cpos.x + halfW - horizontalMargin;
        float x = PickSpawnX(left, right);
        float y = cpos.y + halfH + spawnHeightAboveCamera;
        Vector3 spawnPos = new Vector3(x, y, 0f);

        // Parent must be null: if the template is a scene object under this spawner, Instantiate() would parent
        // clones here too — stacked parent scales made only the first rock look correct and triggers huge.
        Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        GameObject spawned = Instantiate(_spawnPrefab, spawnPos, rot, null);
        if (useForcedSpawnScale)
            spawned.transform.localScale = forcedSpawnScale;
        else
            spawned.transform.localScale = _cachedPrefabLocalScale * Mathf.Max(0.01f, spawnSizeMultiplier);
        spawned.SetActive(true);
    }

    private float PickSpawnX(float left, float right)
    {
        float randomX = Random.Range(left, right);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || spawnAheadOfPlayerBlend <= 0f)
            return randomX;

        float px = player.transform.position.x;
        float face = GetPlayerFacingSign(player);
        float aheadX = Mathf.Clamp(px + face * aheadLeadWorldUnits, left, right);
        return Mathf.Lerp(randomX, aheadX, spawnAheadOfPlayerBlend);
    }

    private static float GetPlayerFacingSign(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.12f)
            return Mathf.Sign(rb.linearVelocity.x);

        float sx = player.transform.localScale.x;
        if (Mathf.Abs(sx) < 0.01f)
            return 1f;
        return Mathf.Sign(sx);
    }

    /// <summary>Editor / runtime fallback so the feature works before you assign art.</summary>
    public static GameObject CreateRuntimePlaceholderAsteroid()
    {
        const int size = 24;
        var tex = new Texture2D(size, size);
        var pixels = new Color[size * size];
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float r = size * 0.45f;
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float d = Vector2.Distance(new Vector2(px + 0.5f, py + 0.5f), c);
                pixels[py * size + px] = d <= r ? new Color(0.35f, 0.32f, 0.38f) : Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);

        var go = new GameObject("FallingAsteroid");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        // Final layer/order applied in SkyFallingAsteroid.Awake (defaults to Player layer).

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.32f;

        go.AddComponent<Rigidbody2D>();
        go.AddComponent<SkyFallingAsteroid>();
        return go;
    }
}
