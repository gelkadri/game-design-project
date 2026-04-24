using UnityEngine;

public enum SkyPlatformDirection { Horizontal, Vertical, Custom }

public class SkyMovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public SkyPlatformDirection direction = SkyPlatformDirection.Horizontal;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    [Header("Custom Path (only used when Direction = Custom)")]
    public Vector2 customOffset = new Vector2(3f, 2f);

    [Header("Timing")]
    [Tooltip("Seconds the platform waits at each end before reversing")]
    public float waitTime = 0f;

    [Header("Riding player")]
    [Tooltip("Optional empty child with local Scale 1,1,1. Assign if this object is stretched (non-uniform scale) so the player does not inherit stretch.")]
    [SerializeField] private Transform carryParent;

    private Transform CarryMount => carryParent != null ? carryParent : transform;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 targetPosition;
    private float waitTimer;
    private bool waiting;

    private Vector3 lastPosition;

    private void Start()
    {
        startPosition = transform.position;

        switch (direction)
        {
            case SkyPlatformDirection.Horizontal:
                endPosition = startPosition + Vector3.right * moveDistance;
                break;
            case SkyPlatformDirection.Vertical:
                endPosition = startPosition + Vector3.up * moveDistance;
                break;
            case SkyPlatformDirection.Custom:
                endPosition = startPosition + (Vector3)customOffset;
                break;
        }

        targetPosition = endPosition;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                waiting = false;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            targetPosition = (targetPosition == endPosition) ? startPosition : endPosition;

            if (waitTime > 0f)
            {
                waiting = true;
                waitTimer = waitTime;
            }
        }

        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Transform playerTf = collision.rigidbody != null ? collision.rigidbody.transform : collision.transform;
        playerTf.SetParent(CarryMount, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Transform playerTf = collision.rigidbody != null ? collision.rigidbody.transform : collision.transform;
        if (playerTf.parent == CarryMount)
            playerTf.SetParent(null, true);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 end;

        switch (direction)
        {
            case SkyPlatformDirection.Horizontal:
                end = start + Vector3.right * moveDistance;
                break;
            case SkyPlatformDirection.Vertical:
                end = start + Vector3.up * moveDistance;
                break;
            default:
                end = start + (Vector3)customOffset;
                break;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.15f);
        Gizmos.DrawWireSphere(end, 0.15f);
    }
}
