
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public enum SkyTravelMode { mobile, pc }

public class SkyExplorerController : MonoBehaviour
{


    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Hazards (same as KillZone — lose a life + restart level)")]
    [Tooltip("If the blue water lives on its own Tilemap set to the \"Water\" layer, assign that layer here. Leave \"Nothing\" if you only use tiles / tags below.")]
    [SerializeField] private LayerMask lethalContactLayers;

    [Tooltip("Drag the water / hazard tiles from the Project (the same assets used on your Tilemap). Works when water shares a Tilemap with safe ground.")]
    [SerializeField] private TileBase[] lethalTiles;

    [Tooltip("Optional: kill when the touched tile’s sprite name contains one of these (case-insensitive), e.g. \"water\" or \"ocean\". Leave empty if unused.")]
    [SerializeField] private string[] lethalTileSpriteNameContains;

    [Tooltip("Optional extra tag on colliders that should kill (e.g. create tag \"water\" and put it on a trigger volume). Leave blank to ignore.")]
    [SerializeField] private string additionalKillTag = "";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGroundedBool = false;
    private bool canDoubleJump = false;

    [FormerlySerializedAs("playeranim")]
    public Animator explorerAnimator;

    [FormerlySerializedAs("controlmode")]
    public SkyTravelMode travelMode;
   

    private float moveX;
    public bool isPaused = false;

    public ParticleSystem footsteps;

    [FormerlySerializedAs("ImpactEffect")]
    public ParticleSystem cloudLandingEffect;
    private bool wasOnGround;


   // public GameObject projectile;
   // public Transform firePoint;

    public float fireRate = 0.5f; // Time between each shot
    private float nextFireTime = 0f; // Time of the next allowed shot


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (travelMode == SkyTravelMode.mobile)
        {
            SkyRealmUIManager.instance.EnableMobileControls();
        }


    }

    private void Update()
    {
        if (isPaused)
            return;

        isGroundedBool = IsGrounded();

        if (isGroundedBool)
        {
            canDoubleJump = true; // Reset double jump when grounded

            if (travelMode == SkyTravelMode.pc)
            {
                moveX = Input.GetAxis("Horizontal");
            }


            if (Input.GetButtonDown("Jump"))
            {
                Jump(jumpForce);
            }
        }
        else
        {
            if (canDoubleJump && Input.GetButtonDown("Jump"))
            {
                Jump(doubleJumpForce);
                canDoubleJump = false; // Disable double jump until grounded again
            }
        }

        if (!isPaused)
        {
            // Calculate rotation angle based on mouse position
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 lookDirection = mousePosition - transform.position;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

            // Handle shooting
            if (travelMode == SkyTravelMode.pc && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
            }
        }
        SetAnimations();

        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        //impactEffect

        if (!wasOnGround && isGroundedBool && cloudLandingEffect != null && footsteps != null)
        {
            cloudLandingEffect.gameObject.SetActive(true);
            cloudLandingEffect.Stop();
            cloudLandingEffect.transform.position = new Vector2(footsteps.transform.position.x, footsteps.transform.position.y - 0.2f);
            cloudLandingEffect.Play();
        }

        wasOnGround = isGroundedBool;

        
    }
public void SetAnimations()
{
    bool running = moveX != 0 && isGroundedBool;

    if (explorerAnimator != null)
    {
        explorerAnimator.SetBool("isRun", running);
        explorerAnimator.SetBool("isJump", !isGroundedBool);
    }

    // Unity 6: do not use ParticleSystem.EmissionModule (rateOverTime) from script — it throws at runtime.
    // Toggle the footstep object instead; set emission rate once in the ParticleSystem prefab / Inspector.
    if (footsteps != null && footsteps.gameObject.activeSelf != running)
        footsteps.gameObject.SetActive(running);
}

    private void FlipSprite(float direction)
    {
        // Do not scale the root transform — that breaks SetParent(worldPositionStays)
        // on moving platforms with non-uniform scale (player looks stretched).
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0f;
            return;
        }

        if (direction > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (direction < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    private void FixedUpdate()
    {
        if (isPaused)
            return;

        // Player movement
        if (travelMode == SkyTravelMode.pc)
        {
            moveX = Input.GetAxis("Horizontal");
        }
       


        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Zero out vertical velocity
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        explorerAnimator.SetBool("isJump", true);
        if (SkyRealmGameManager.instance != null)
        {
            SkyRealmGameManager.instance.PlayJumpSound();
        }
    }

    private bool IsGrounded()
    {
        float rayLength = 0.25f;
        Vector2 rayOrigin = new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleHazardCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHandleHazardCollision(collision);
    }

    private void TryHandleHazardCollision(Collision2D collision)
    {
        if (SkyRealmGameManager.instance == null)
            return;

        if (collision.gameObject.CompareTag("killzone"))
        {
            SkyRealmGameManager.instance.Death();
            return;
        }

        if (!string.IsNullOrEmpty(additionalKillTag) && collision.gameObject.CompareTag(additionalKillTag))
        {
            SkyRealmGameManager.instance.Death();
            return;
        }

        int layerBit = 1 << collision.gameObject.layer;
        if (lethalContactLayers.value != 0 && (lethalContactLayers.value & layerBit) != 0)
        {
            SkyRealmGameManager.instance.Death();
            return;
        }

        TryDeathFromLethalTilemap(collision);
    }

    private void TryDeathFromLethalTilemap(Collision2D collision)
    {
        if (SkyRealmGameManager.instance == null)
            return;

        bool useTileRefs = lethalTiles != null && lethalTiles.Length > 0;
        bool useNameHints = lethalTileSpriteNameContains != null && lethalTileSpriteNameContains.Length > 0;
        if (!useTileRefs && !useNameHints)
            return;

        Tilemap tm = collision.collider.GetComponent<Tilemap>() ?? collision.collider.GetComponentInParent<Tilemap>();
        if (tm == null)
            return;

        // Contact points often sit on shared edges → WorldToCell picks the wrong (empty) cell.
        // Scan all tilemap cells overlapped by the player collider while touching this map.
        Collider2D playerCol = GetComponent<Collider2D>();
        if (playerCol != null)
        {
            Bounds b = playerCol.bounds;
            b.Expand(0.1f);
            if (TryDeathFromCellsInWorldBounds(tm, b, useTileRefs, useNameHints))
                return;
        }

        if (collision.contactCount > 0)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D c = collision.GetContact(i);
                Vector2 biased = c.point - c.normal * 0.12f;
                if (TryDeathFromCellsNearWorldPoint(tm, biased, useTileRefs, useNameHints))
                    return;
            }
        }
        else
        {
            Vector2 probe = collision.collider.ClosestPoint(transform.position);
            if (TryDeathFromCellsNearWorldPoint(tm, probe, useTileRefs, useNameHints))
                return;
        }
    }

    private bool TryDeathFromCellsInWorldBounds(Tilemap tm, Bounds worldBounds, bool useTileRefs, bool useNameHints)
    {
        Vector3Int min = tm.WorldToCell(worldBounds.min);
        Vector3Int max = tm.WorldToCell(worldBounds.max);
        int minX = Mathf.Min(min.x, max.x);
        int maxX = Mathf.Max(min.x, max.x);
        int minY = Mathf.Min(min.y, max.y);
        int maxY = Mathf.Max(min.y, max.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (IsLethalCell(tm, new Vector3Int(x, y, 0), useTileRefs, useNameHints))
                {
                    SkyRealmGameManager.instance.Death();
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryDeathFromCellsNearWorldPoint(Tilemap tm, Vector2 worldPoint, bool useTileRefs, bool useNameHints)
    {
        Vector3Int center = tm.WorldToCell(worldPoint);
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (IsLethalCell(tm, center + new Vector3Int(dx, dy, 0), useTileRefs, useNameHints))
                {
                    SkyRealmGameManager.instance.Death();
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsLethalCell(Tilemap tm, Vector3Int cell, bool useTileRefs, bool useNameHints)
    {
        TileBase tile = tm.GetTile(cell);
        if (tile == null)
            return false;

        if (useTileRefs)
        {
            foreach (TileBase hazard in lethalTiles)
            {
                if (hazard == null)
                    continue;
                if (ReferenceEquals(tile, hazard) || string.Equals(tile.name, hazard.name, StringComparison.Ordinal))
                    return true;
            }
        }

        if (useNameHints)
        {
            Sprite spr = tm.GetSprite(cell);
            if (spr != null)
            {
                string spriteName = spr.name;
                foreach (string hint in lethalTileSpriteNameContains)
                {
                    if (string.IsNullOrEmpty(hint))
                        continue;
                    if (spriteName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
        }

        return false;
    }

    //mobile;
    public void MobileMove(float value)
    {
        if (isPaused)
            return;
        moveX = value;
    }
    public void MobileJump()
    {
        if (isPaused)
            return;
        if (isGroundedBool)
        {
            // Perform initial jump
            Jump(jumpForce);
        }
        else
        {
            // Perform double jump if allowed
            if (canDoubleJump)
            {
                Jump(doubleJumpForce);
                canDoubleJump = false; // Disable double jump until grounded again
            }
        }
    }

    public void Shoot()
    {
        //GameObject fireBall = Instantiate(projectile, firePoint.position, Quaternion.identity);
        //fireBall.GetComponent<Rigidbody2D>().AddForce(firePoint.right * 500f);
    }

    public void MobileShoot()
    {
        if (isPaused)
            return;
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
        }
    }

}