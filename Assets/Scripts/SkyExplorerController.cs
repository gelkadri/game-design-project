
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public enum SkyTravelMode { mobile, pc }

public class SkyExplorerController : MonoBehaviour
{


    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private bool isGroundedBool = false;
    private bool canDoubleJump = false;

    [FormerlySerializedAs("playeranim")]
    public Animator explorerAnimator;

    [FormerlySerializedAs("controlmode")]
    public SkyTravelMode travelMode;
   

    private float moveX;
    public bool isPaused = false;

    public ParticleSystem footsteps;
    private ParticleSystem.EmissionModule footEmissions;

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
        footEmissions = footsteps.emission;

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

        if (!wasOnGround && isGroundedBool)
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
    if (moveX != 0 && isGroundedBool)
    {
        explorerAnimator.SetBool("isRun", true);
        footEmissions.rateOverTime = 35f;
    }
    else
    {
        explorerAnimator.SetBool("isRun", false);
        footEmissions.rateOverTime = 0f;
    }

    if (!isGroundedBool)
    {
        explorerAnimator.SetBool("isJump", true);
    }
    else
    {
        explorerAnimator.SetBool("isJump", false);
    }
}

    private void FlipSprite(float direction)
    {
        if (direction > 0)
        {
            // Moving right, flip sprite to the right
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction < 0)
        {
            // Moving left, flip sprite to the left
            transform.localScale = new Vector3(-1, 1, 1);
        }
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
        if(collision.gameObject.tag == "killzone")
        {
            SkyRealmGameManager.instance.Death();
        }
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