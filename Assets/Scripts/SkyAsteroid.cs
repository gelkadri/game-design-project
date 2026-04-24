using UnityEngine;

public class SkyAsteroid : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private GameObject impactEffect;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.down * fallSpeed;
        rb.angularVelocity = Random.Range(-rotateSpeed, rotateSpeed);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (SkyHeartManager.instance != null)
                SkyHeartManager.instance.HurtPlayer();

            SpawnImpact();
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (SkyHeartManager.instance != null)
                SkyHeartManager.instance.HurtPlayer();
        }

        SpawnImpact();
        Destroy(gameObject);
    }

    private void SpawnImpact()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);
    }
}
