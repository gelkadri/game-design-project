using UnityEngine;

/// <summary>
/// Falls downward; on trigger with the player, applies the same penalty as a killzone
/// (lose a life and restart the level if any hearts remain, otherwise game over).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SkyFallingAsteroid : MonoBehaviour
{
#if UNITY_EDITOR
    private void Reset()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (GetComponent<Collider2D>() == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.35f;
        }
    }
#endif

    [SerializeField] private float fallSpeed = 7f;
    [SerializeField] private float spinDegreesPerSecond = 120f;
    [SerializeField] private float destroyBelowCameraPadding = 4f;

    [Tooltip("Must match a name in Edit → Project Settings → Tags and Layers → Sorting Layers (e.g. Player draws above Platform/Background).")]
    [SerializeField] private string renderSortingLayer = "Player";

    [SerializeField] private int renderSortingOrder = 25;

    private bool _spent;
    private Rigidbody2D _rb;
    private Camera _cam;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _cam = Camera.main;

        ApplyDrawOrder();
    }

    private void Start()
    {
        // CircleCollider2D.radius is in local units (not texture pixels). Values like 32 make a screen-wide hitbox.
        ClampColliderToSpriteIfAbsurd();
    }

    private void ClampColliderToSpriteIfAbsurd()
    {
        var circle = GetComponent<CircleCollider2D>();
        var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (circle == null || sr == null || sr.sprite == null)
            return;

        float worldHalf = Mathf.Min(sr.bounds.extents.x, sr.bounds.extents.y);
        float maxLossy = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), 0.01f);
        float suggestedLocal = worldHalf / maxLossy * 0.92f;

        if (circle.radius > Mathf.Max(suggestedLocal * 2.2f, 1.75f))
        {
            float clamped = Mathf.Clamp(suggestedLocal, 0.06f, 4f);
            Debug.LogWarning($"SkyFallingAsteroid on \"{name}\": CircleCollider2D radius was {circle.radius} (too large — often mistaken for pixels). Clamped to {clamped:F2} to match the sprite.");
            circle.radius = clamped;
        }
    }

    private void ApplyDrawOrder()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            return;

        if (!string.IsNullOrEmpty(renderSortingLayer))
        {
            try
            {
                sr.sortingLayerName = renderSortingLayer;
            }
            catch
            {
                // Invalid layer name would break the whole component in the Inspector / at runtime.
            }
        }

        sr.sortingOrder = renderSortingOrder;
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);

        if (_cam != null && _cam.orthographic)
        {
            float bottom = _cam.transform.position.y - _cam.orthographicSize - destroyBelowCameraPadding;
            if (transform.position.y < bottom)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_spent)
            return;
        if (!other.CompareTag("Player"))
            return;
        if (SkyRealmGameManager.instance == null)
            return;

        _spent = true;
        foreach (var c in GetComponents<Collider2D>())
            c.enabled = false;

        SkyRealmGameManager.instance.Death();
        Destroy(gameObject, 0.15f);
    }
}
